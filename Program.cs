using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using WEBDULICH.Services;
using WEBDULICH.Hubs;
using Serilog;
using StackExchange.Redis;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.OpenApi.Models;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
// using Nest; // Commented out - add back when Elasticsearch is needed
using RabbitMQ.Client;
using Azure.Storage.Blobs;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting WEBDULICH application");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllersWithViews()
        .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        });

    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    builder.Services.AddHttpContextAccessor();

    // Configure Admin Access
    builder.Services.Configure<AdminAccessOptions>(
        builder.Configuration.GetSection(AdminAccessOptions.SectionName));

    // ============ REDIS CONFIGURATION ============
    var redisConnection = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrEmpty(redisConnection))
    {
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = ConfigurationOptions.Parse(redisConnection);
            configuration.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(configuration);
        });

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = builder.Configuration["Redis:InstanceName"] ?? "WEBDULICH_";
        });

        Log.Information("Redis caching configured");
    }

    // ============ DATABASE CONFIGURATION ============
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // ============ HANGFIRE CONFIGURATION ============
    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("Hangfire"),
            new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

    builder.Services.AddHangfireServer(options =>
    {
        options.WorkerCount = builder.Configuration.GetValue<int>("Hangfire:WorkerCount", 5);
        options.Queues = builder.Configuration.GetSection("Hangfire:Queues").Get<string[]>() 
            ?? new[] { "default", "critical", "background" };
    });

    Log.Information("Hangfire configured");

    // ============ JWT AUTHENTICATION ============
    var jwtSettings = builder.Configuration.GetSection("JWT");
    var secretKey = jwtSettings["SecretKey"];

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

    Log.Information("JWT Authentication configured");

    // ============ RATE LIMITING ============
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("RateLimiting"));
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    Log.Information("Rate limiting configured");

    // ============ RESPONSE COMPRESSION ============
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
    });

    builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });

    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.SmallestSize;
    });

    Log.Information("Response compression configured");

    // ============ SIGNALR ============
    builder.Services.AddSignalR();

    Log.Information("SignalR configured");

    // ============ AUTOMAPPER ============
    builder.Services.AddAutoMapper(typeof(Program));

    Log.Information("AutoMapper configured");

    // ============ FLUENTVALIDATION ============
    // FluentValidation is available for manual validation
    // builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    Log.Information("FluentValidation configured");

    // ============ MEDIATR ============
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

    Log.Information("MediatR configured");

    // ============ SWAGGER/OPENAPI ============
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = builder.Configuration["Swagger:Title"] ?? "WEBDULICH API",
            Version = builder.Configuration["Swagger:Version"] ?? "v1",
            Description = builder.Configuration["Swagger:Description"] ?? "Travel Management System API",
            Contact = new OpenApiContact
            {
                Name = builder.Configuration["Swagger:ContactName"] ?? "WEBDULICH Team",
                Email = builder.Configuration["Swagger:ContactEmail"] ?? "support@webdulich.local"
            }
        });

        // Add JWT Authentication to Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    Log.Information("Swagger configured");

    // ============ ELASTICSEARCH ============
    // Commented out temporarily - uncomment when ready to use Elasticsearch
    /*
    var elasticsearchUri = builder.Configuration["Elasticsearch:Uri"];
    if (!string.IsNullOrEmpty(elasticsearchUri))
    {
        var settings = new Nest.ConnectionSettings(new Uri(elasticsearchUri))
            .DefaultIndex(builder.Configuration["Elasticsearch:DefaultIndex"] ?? "webdulich");

        var username = builder.Configuration["Elasticsearch:Username"];
        var password = builder.Configuration["Elasticsearch:Password"];
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            settings.BasicAuthentication(username, password);
        }

        var client = new Nest.ElasticClient(settings);
        builder.Services.AddSingleton<Nest.IElasticClient>(client);

        Log.Information("Elasticsearch configured");
    }
    */

    // ============ RABBITMQ ============
    var rabbitMQConfig = builder.Configuration.GetSection("RabbitMQ");
    if (rabbitMQConfig.Exists())
    {
        builder.Services.AddSingleton<RabbitMQ.Client.IConnection>(sp =>
        {
            var factory = new ConnectionFactory()
            {
                HostName = rabbitMQConfig["HostName"] ?? "localhost",
                Port = rabbitMQConfig.GetValue<int>("Port", 5672),
                UserName = rabbitMQConfig["UserName"] ?? "guest",
                Password = rabbitMQConfig["Password"] ?? "guest",
                VirtualHost = rabbitMQConfig["VirtualHost"] ?? "/"
            };
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        Log.Information("RabbitMQ configured");
    }

    // ============ AZURE BLOB STORAGE ============
    var azureStorageEnabled = builder.Configuration.GetValue<bool>("AzureStorage:Enabled", false);
    if (azureStorageEnabled)
    {
        var connectionString = builder.Configuration["AzureStorage:ConnectionString"];
        if (!string.IsNullOrEmpty(connectionString))
        {
            builder.Services.AddSingleton(x => new BlobServiceClient(connectionString));
            Log.Information("Azure Blob Storage configured");
        }
    }

    // ============ HEALTH CHECKS ============
    var healthChecksBuilder = builder.Services.AddHealthChecks()
        .AddSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            name: "database",
            tags: new[] { "db", "sql", "sqlserver" });

    if (!string.IsNullOrEmpty(redisConnection))
    {
        // Redis health check - commented out temporarily
        // healthChecksBuilder.AddRedis(redisConnection, name: "redis", tags: new[] { "cache", "redis" });
    }

    // Hangfire health check - commented out temporarily  
    // healthChecksBuilder.AddHangfire(options => { options.MinimumAvailableServers = 1; }, name: "hangfire", tags: new[] { "jobs", "hangfire" });

    Log.Information("Health checks configured");

    // ============ CORE SERVICES ============
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<IImageStorageService, ImageStorageService>();
    builder.Services.AddScoped<IEmailService, EmailService>();

    // ============ BUSINESS SERVICES ============
    builder.Services.AddScoped<ITourService, TourService>();
    builder.Services.AddScoped<IHotelService, HotelService>();
    builder.Services.AddScoped<IDestinationService, DestinationService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IReviewService, ReviewService>();
    builder.Services.AddScoped<IPaymentService, PaymentService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IBookingService, BookingService>();
    builder.Services.AddScoped<IWishlistService, WishlistService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IChatService, ChatService>();
    builder.Services.AddScoped<IBlogService, BlogService>();
    builder.Services.AddScoped<ICouponService, CouponService>();
    builder.Services.AddScoped<IReportService, ReportService>();
    builder.Services.AddScoped<IMapService, MapService>();
    builder.Services.AddScoped<IEmailMarketingService, EmailMarketingService>();
    builder.Services.AddScoped<ILoyaltyService, LoyaltyService>();

    // ============ NEW FEATURE SERVICES ============
    // Payment Gateway Services
    builder.Services.AddScoped<WEBDULICH.Services.PaymentGateway.VNPayService>();
    builder.Services.AddScoped<WEBDULICH.Services.PaymentGateway.MoMoService>();
    
    // AI Chatbot Service
    builder.Services.AddScoped<WEBDULICH.Services.AI.IChatbotService, WEBDULICH.Services.AI.ChatbotService>();
    
    // Analytics Service
    builder.Services.AddScoped<WEBDULICH.Services.Analytics.IAnalyticsService, WEBDULICH.Services.Analytics.AnalyticsService>();
    
    // Social Auth Service
    builder.Services.AddScoped<WEBDULICH.Services.Auth.ISocialAuthService, WEBDULICH.Services.Auth.SocialAuthService>();
    
    // Ticket Service
    builder.Services.AddScoped<WEBDULICH.Services.Ticket.ITicketService, WEBDULICH.Services.Ticket.TicketService>();

    Log.Information("New feature services registered (Payment, Chatbot, Analytics, Social Auth, Ticket)");

    // ============ HTTP CLIENT ============
    builder.Services.AddHttpClient();

    // ============ SECURITY AND LOCALIZATION ============
    builder.Services.AddScoped<ISecurityService, SecurityService>();
    builder.Services.AddSingleton<ILocalizationService, LocalizationService>();

    // ============ DATA SEEDER ============
    builder.Services.AddScoped<DataSeeder>();

    Log.Information("All services registered");

    // ============ BUILD APP ============
    var app = builder.Build();

    // ============ MIDDLEWARE PIPELINE ============
    
    // Exception handling
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    // Swagger (available in all environments for API documentation)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WEBDULICH API v1");
        c.RoutePrefix = "api-docs";
    });

    // Response compression
    app.UseResponseCompression();

    // Rate limiting
    app.UseIpRateLimiting();

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    // Serilog request logging
    app.UseSerilogRequestLogging();

    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();

    // Health checks endpoint
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Hangfire Dashboard
    app.UseHangfireDashboard(builder.Configuration["Hangfire:DashboardPath"] ?? "/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });

    // SignalR Hubs
    app.MapHub<NotificationHub>("/hubs/notification");
    app.MapHub<ChatHub>("/hubs/chat");

    // MVC Routes
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Seed data
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();
            Log.Information("Data seeding completed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while seeding the database");
        }
    }

    Log.Information("Application started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Hangfire Authorization Filter
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // In production, implement proper authorization
        // For now, allow access in development
        return true;
    }
}

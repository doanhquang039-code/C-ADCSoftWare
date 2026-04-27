using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using WEBDULICH.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<AdminAccessOptions>(
    builder.Configuration.GetSection(AdminAccessOptions.SectionName));

// Core services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();

// Business services
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IDestinationService, DestinationService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IUserService, UserService>();

// New services
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<ICouponService, CouponService>();

// Security and Localization services
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();

// Add DataSeeder
builder.Services.AddScoped<DataSeeder>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed data
//using (var scope = app.Services.CreateScope())
//{
//    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
//    await seeder.SeedAsync();
//}

app.Run();

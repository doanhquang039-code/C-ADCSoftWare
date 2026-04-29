# WEBDULICH - Modern Technologies Implementation

## 🚀 Overview
This document describes the modern technologies integrated into the WEBDULICH Travel Management System.

## 📦 Integrated Technologies

### 1. **Redis** - Distributed Caching & Session Management
- **Package**: StackExchange.Redis 2.8.16
- **Purpose**: High-performance caching, session storage, distributed locks
- **Configuration**: `appsettings.json` → `ConnectionStrings:Redis`
- **Usage**:
  ```csharp
  // Inject IDistributedCache
  await _cache.SetStringAsync("key", "value", new DistributedCacheEntryOptions
  {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
  });
  ```

### 2. **SignalR** - Real-time Communication
- **Package**: Microsoft.AspNetCore.SignalR 1.1.0
- **Purpose**: Real-time notifications, chat, live updates
- **Hubs Created**:
  - `NotificationHub` - `/hubs/notification`
  - `ChatHub` - `/hubs/chat`
- **Usage**:
  ```javascript
  const connection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/notification")
      .build();
  
  connection.on("ReceiveNotification", (message) => {
      console.log(message);
  });
  ```

### 3. **Hangfire** - Background Job Processing
- **Packages**: Hangfire.AspNetCore 1.8.17, Hangfire.SqlServer 1.8.17
- **Purpose**: Scheduled tasks, background jobs, recurring jobs
- **Dashboard**: `/hangfire`
- **Jobs Created**:
  - `EmailJob` - Email sending tasks
  - `DataCleanupJob` - Data maintenance tasks
- **Usage**:
  ```csharp
  // Fire-and-forget
  BackgroundJob.Enqueue(() => Console.WriteLine("Hello"));
  
  // Delayed
  BackgroundJob.Schedule(() => Console.WriteLine("Delayed"), TimeSpan.FromMinutes(5));
  
  // Recurring
  RecurringJob.AddOrUpdate("daily-cleanup", () => CleanupOldData(), Cron.Daily);
  ```

### 4. **Serilog** - Structured Logging
- **Package**: Serilog.AspNetCore 8.0.3
- **Purpose**: Advanced logging with structured data
- **Sinks**: Console, File
- **Log Location**: `Logs/webdulich-{Date}.log`
- **Usage**:
  ```csharp
  _logger.LogInformation("User {UserId} created booking {BookingId}", userId, bookingId);
  ```

### 5. **AutoMapper** - Object Mapping
- **Package**: AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1
- **Purpose**: DTO/ViewModel mapping
- **Profiles Created**: `TourProfile`
- **Usage**:
  ```csharp
  var viewModel = _mapper.Map<TourViewModel>(tour);
  ```

### 6. **FluentValidation** - Input Validation
- **Package**: FluentValidation.AspNetCore 11.3.0
- **Purpose**: Fluent validation rules
- **Validators Created**: `TourValidator`, `HotelValidator`, `BookingValidator`
- **Usage**:
  ```csharp
  public class TourValidator : AbstractValidator<TourViewModel>
  {
      public TourValidator()
      {
          RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
          RuleFor(x => x.Price).GreaterThan(0);
      }
  }
  ```

### 7. **MediatR** - CQRS Pattern
- **Package**: MediatR 12.4.1
- **Purpose**: Command/Query separation, mediator pattern
- **Commands**: `CreateBookingCommand`
- **Queries**: `GetTourByIdQuery`, `GetAllToursQuery`
- **Usage**:
  ```csharp
  var tour = await _mediator.Send(new GetTourByIdQuery(tourId));
  var booking = await _mediator.Send(new CreateBookingCommand { ... });
  ```

### 8. **Swagger/OpenAPI** - API Documentation
- **Package**: Swashbuckle.AspNetCore 6.9.0
- **Purpose**: Interactive API documentation
- **URL**: `/api-docs`
- **Features**: JWT authentication support, interactive testing

### 9. **Health Checks** - Application Monitoring
- **Package**: AspNetCore.HealthChecks.UI.Client 8.0.1
- **Purpose**: Monitor application health
- **Endpoint**: `/health`
- **Checks**:
  - SQL Server database connectivity
  - Redis connectivity
  - Hangfire job processing
- **Usage**:
  ```bash
  curl http://localhost:5000/health
  ```

### 10. **JWT Authentication** - Secure API Access
- **Package**: Microsoft.AspNetCore.Authentication.JwtBearer 8.0.11
- **Purpose**: Token-based authentication
- **Configuration**: `appsettings.json` → `JWT` section
- **Usage**:
  ```csharp
  [Authorize]
  public class SecureController : Controller { }
  ```

### 11. **Rate Limiting** - API Protection
- **Package**: AspNetCoreRateLimit 5.0.0
- **Purpose**: Prevent API abuse
- **Configuration**: `appsettings.json` → `RateLimiting` section
- **Default Limits**:
  - 60 requests per minute
  - 1000 requests per hour

### 12. **Response Compression** - Performance Optimization
- **Package**: Microsoft.AspNetCore.ResponseCompression 2.2.0
- **Purpose**: Compress HTTP responses
- **Algorithms**: Brotli (fastest), Gzip (smallest)

### 13. **Elasticsearch** - Full-Text Search
- **Package**: NEST 7.17.5
- **Purpose**: Advanced search capabilities
- **Configuration**: `appsettings.json` → `Elasticsearch` section
- **Usage**:
  ```csharp
  var searchResponse = await _elasticClient.SearchAsync<Tour>(s => s
      .Query(q => q.Match(m => m.Field(f => f.Name).Query(searchTerm)))
  );
  ```

### 14. **RabbitMQ** - Message Queue
- **Package**: RabbitMQ.Client 7.2.1
- **Purpose**: Asynchronous messaging, event-driven architecture
- **Configuration**: `appsettings.json` → `RabbitMQ` section
- **Usage**:
  ```csharp
  using var connection = _connectionFactory.CreateConnection();
  using var channel = connection.CreateModel();
  channel.QueueDeclare(queue: "bookings", durable: true, exclusive: false);
  ```

### 15. **Azure Blob Storage** - Cloud Storage
- **Package**: Azure.Storage.Blobs 12.27.0
- **Purpose**: Cloud file storage
- **Configuration**: `appsettings.json` → `AzureStorage` section
- **Usage**:
  ```csharp
  var containerClient = _blobServiceClient.GetBlobContainerClient("images");
  await containerClient.UploadBlobAsync("image.jpg", stream);
  ```

## 🐳 Docker Infrastructure

### Start All Services
```bash
cd WEBDULICH
docker-compose up -d
```

### Services Available
- **SQL Server**: localhost:1433
- **Redis**: localhost:6379
- **RabbitMQ**: localhost:5672 (Management UI: localhost:15672)
- **Elasticsearch**: localhost:9200
- **Kibana**: localhost:5601

### Stop All Services
```bash
docker-compose down
```

## 🔧 Configuration

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "SQL Server connection",
    "Redis": "Redis connection",
    "Hangfire": "Hangfire database connection"
  },
  "JWT": { "SecretKey", "Issuer", "Audience", "ExpiryInMinutes" },
  "RateLimiting": { "Rules" },
  "RabbitMQ": { "HostName", "Port", "UserName", "Password" },
  "Elasticsearch": { "Uri", "DefaultIndex" },
  "AzureStorage": { "ConnectionString", "ContainerName" },
  "Hangfire": { "DashboardPath", "WorkerCount", "Queues" },
  "Serilog": { "Logging configuration" }
}
```

## 📊 Monitoring & Management

### Hangfire Dashboard
- URL: `http://localhost:5000/hangfire`
- Features: Job monitoring, recurring jobs, failed jobs retry

### Swagger API Documentation
- URL: `http://localhost:5000/api-docs`
- Features: Interactive API testing, JWT authentication

### Health Checks
- URL: `http://localhost:5000/health`
- Returns: JSON with health status of all components

### RabbitMQ Management
- URL: `http://localhost:15672`
- Credentials: guest/guest

### Kibana (Elasticsearch UI)
- URL: `http://localhost:5601`

## 🚀 Getting Started

### 1. Start Infrastructure
```bash
docker-compose up -d
```

### 2. Update Connection Strings
Edit `appsettings.json` with your connection strings

### 3. Run Migrations
```bash
dotnet ef database update
```

### 4. Run Application
```bash
dotnet run
```

### 5. Access Application
- Web: `http://localhost:5000`
- API Docs: `http://localhost:5000/api-docs`
- Hangfire: `http://localhost:5000/hangfire`
- Health: `http://localhost:5000/health`

## 📝 Example Usage Scenarios

### 1. Send Real-time Notification
```csharp
await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", "Booking confirmed!");
```

### 2. Schedule Background Job
```csharp
BackgroundJob.Schedule<EmailJob>(x => x.SendBookingConfirmation(bookingId, email), TimeSpan.FromMinutes(5));
```

### 3. Cache Data
```csharp
var cacheKey = $"tour_{tourId}";
var cachedTour = await _cache.GetStringAsync(cacheKey);
if (cachedTour == null)
{
    var tour = await _context.Tours.FindAsync(tourId);
    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(tour));
}
```

### 4. Search with Elasticsearch
```csharp
var results = await _elasticClient.SearchAsync<Tour>(s => s
    .Query(q => q
        .MultiMatch(m => m
            .Fields(f => f.Field(t => t.Name).Field(t => t.Description))
            .Query(searchTerm)
        )
    )
);
```

### 5. Publish Message to RabbitMQ
```csharp
var message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(booking));
_channel.BasicPublish(exchange: "", routingKey: "bookings", body: message);
```

## 🔐 Security Considerations

1. **JWT Secret**: Change the JWT secret key in production
2. **Database Passwords**: Use strong passwords for SQL Server
3. **Redis**: Enable authentication in production
4. **RabbitMQ**: Change default credentials
5. **Hangfire Dashboard**: Implement proper authorization
6. **Rate Limiting**: Adjust limits based on your needs

## 📈 Performance Tips

1. **Redis**: Use for frequently accessed data
2. **Response Compression**: Reduces bandwidth by 60-80%
3. **Hangfire**: Offload long-running tasks
4. **Elasticsearch**: Index large datasets for fast search
5. **SignalR**: Use for real-time features only

## 🐛 Troubleshooting

### Redis Connection Failed
- Ensure Redis is running: `docker ps`
- Check connection string in appsettings.json

### Hangfire Dashboard Not Loading
- Check Hangfire database connection
- Verify SQL Server is accessible

### SignalR Not Connecting
- Check CORS settings
- Verify hub URLs are correct

### Elasticsearch Not Responding
- Increase Docker memory allocation
- Check Elasticsearch logs: `docker logs webdulich-elasticsearch`

## 📚 Additional Resources

- [Hangfire Documentation](https://docs.hangfire.io/)
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/)
- [Serilog Documentation](https://serilog.net/)
- [AutoMapper Documentation](https://docs.automapper.org/)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Elasticsearch .NET Documentation](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/index.html)
- [RabbitMQ .NET Documentation](https://www.rabbitmq.com/dotnet.html)

## ✅ Implementation Status

All 15 modern technologies have been successfully integrated:
- ✅ Redis (Caching)
- ✅ SignalR (Real-time)
- ✅ Hangfire (Background Jobs)
- ✅ Serilog (Logging)
- ✅ AutoMapper (Mapping)
- ✅ FluentValidation (Validation)
- ✅ MediatR (CQRS)
- ✅ Swagger (API Docs)
- ✅ Health Checks (Monitoring)
- ✅ JWT (Authentication)
- ✅ Rate Limiting (Protection)
- ✅ Response Compression (Performance)
- ✅ Elasticsearch (Search)
- ✅ RabbitMQ (Messaging)
- ✅ Azure Blob Storage (Cloud Storage)

## 🎉 Next Steps

1. Test each technology integration
2. Customize configurations for your environment
3. Implement business logic using these technologies
4. Monitor performance and adjust settings
5. Deploy to production with proper security measures

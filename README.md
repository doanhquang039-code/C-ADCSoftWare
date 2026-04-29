# 🌍 WEBDULICH - Travel Management System

## 📋 Mục Đích Dự Án

**WEBDULICH** là một hệ thống quản lý du lịch toàn diện được xây dựng bằng **ASP.NET Core 8.0**, cung cấp giải pháp hoàn chỉnh cho việc đặt tour, quản lý khách sạn, thanh toán trực tuyến và nhiều tính năng hiện đại khác.

### 🎯 Mục Tiêu Chính:
- ✅ Quản lý tour du lịch và điểm đến
- ✅ Đặt phòng khách sạn trực tuyến
- ✅ Thanh toán an toàn với VNPay và MoMo
- ✅ Hỗ trợ khách hàng 24/7 với AI Chatbot
- ✅ Phân tích dữ liệu kinh doanh chi tiết
- ✅ Đăng nhập nhanh với mạng xã hội
- ✅ Vé điện tử với QR code

---

## 🚀 Công Nghệ Sử Dụng

### Backend Framework
- **ASP.NET Core 8.0** - Web framework chính
- **Entity Framework Core** - ORM cho database
- **C# 12** - Programming language

### Database
- **SQL Server** - Primary database
- **Redis** - Caching & session storage

### Real-time & Background Jobs
- **SignalR** - Real-time communication (notifications, chat)
- **Hangfire** - Background job processing
- **RabbitMQ** - Message queue

### Authentication & Security
- **JWT Bearer** - API authentication
- **OAuth 2.0** - Social login (Google, Facebook, Apple)
- **AspNetCoreRateLimit** - Rate limiting
- **HTTPS** - Secure communication

### Logging & Monitoring
- **Serilog** - Structured logging
- **Health Checks** - Application health monitoring

### API & Documentation
- **Swagger/OpenAPI** - API documentation
- **RESTful API** - API architecture

### Data Processing
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **MediatR** - CQRS pattern

### Storage & Cloud
- **Azure Blob Storage** - File storage (optional)
- **Elasticsearch** - Search engine (optional)

### Performance
- **Response Compression** - Brotli & Gzip
- **Async/Await** - Asynchronous programming
- **Database Indexing** - Query optimization

---

## 🎨 Tính Năng Chính

### 1. 🏖️ Quản Lý Tour & Điểm Đến
- Tạo và quản lý tour du lịch
- Quản lý điểm đến
- Phân loại theo category
- Upload hình ảnh
- Đánh giá và review
- Wishlist

### 2. 🏨 Quản Lý Khách Sạn
- Danh sách khách sạn
- Thông tin chi tiết
- Đặt phòng trực tuyến
- Quản lý booking

### 3. 💳 Thanh Toán Trực Tuyến
- **VNPay** - Cổng thanh toán Việt Nam
- **MoMo** - Ví điện tử
- Thanh toán an toàn với HMAC signature
- Hoàn tiền tự động
- Kiểm tra trạng thái thanh toán
- IPN (Instant Payment Notification)

### 4. 🤖 AI Chatbot
- Hỗ trợ khách hàng 24/7
- 15+ intents (Greeting, SearchTour, BookTour, etc.)
- Entity extraction (location, date, price, people)
- Conversation history
- Quick replies & suggested actions
- Analytics & reporting

### 5. 📊 Advanced Analytics
- Dashboard metrics (revenue, bookings, customers)
- Revenue charts (day/week/month)
- Popular tours analysis
- Customer segmentation (VIP, Regular, New)
- Booking trends
- Conversion funnel
- Export to Excel/PDF

### 6. 🔐 Social Login
- **Google Sign-In** - OAuth 2.0
- **Facebook Login** - Graph API
- **Apple Sign-In** - Sign in with Apple
- Account linking
- Profile sync
- Auto user creation

### 7. 🎫 E-Ticket Generation
- Unique ticket code
- QR code generation
- PDF ticket generation
- Email delivery
- Ticket validation
- Status management (Active, Used, Expired, Cancelled, Refunded)
- Usage tracking

### 8. 📧 Email Marketing
- Campaign management
- Email templates
- Subscriber management
- Email tracking
- Analytics

### 9. 🎁 Loyalty Program
- Points system
- Tier management (Bronze, Silver, Gold, Platinum)
- Rewards & redemption
- Transaction history

### 10. 📍 Map & Location Services
- Location search
- Route planning
- Nearby attractions
- Distance calculation

### 11. 📝 Blog & Content
- Blog posts
- Categories
- Tags
- Comments
- SEO optimization

### 12. 🔔 Notifications
- Real-time notifications (SignalR)
- Email notifications
- Push notifications (ready)
- Notification history

### 13. 💬 Chat Support
- Real-time chat (SignalR)
- Chat history
- File sharing
- Typing indicators

### 14. 🎟️ Coupon & Discounts
- Coupon codes
- Discount management
- Usage tracking
- Expiry management

### 15. 📈 Reporting
- Revenue reports
- Booking reports
- Customer reports
- Custom reports

---

## 📁 Cấu Trúc Dự Án

```
WEBDULICH/
├── Controllers/              # API & MVC Controllers
│   ├── HomeController.cs
│   ├── TourController.cs
│   ├── PaymentController.cs
│   ├── ChatbotController.cs
│   ├── AnalyticsController.cs
│   ├── SocialAuthController.cs
│   ├── TicketController.cs
│   └── ...
│
├── Services/                 # Business Logic
│   ├── Payment/             # VNPay, MoMo services
│   ├── AI/                  # Chatbot service
│   ├── Analytics/           # Analytics service
│   ├── Auth/                # Social auth service
│   ├── Ticket/              # Ticket service
│   ├── Email/               # Email service
│   └── ...
│
├── Models/                   # Data Models
│   ├── User.cs
│   ├── Tour.cs
│   ├── Orders.cs
│   ├── OrderDetail.cs
│   ├── Ticket.cs
│   └── ...
│
├── Views/                    # Razor Views
│   ├── Home/
│   ├── Tour/
│   ├── Admin/
│   └── ...
│
├── Hubs/                     # SignalR Hubs
│   ├── NotificationHub.cs
│   └── ChatHub.cs
│
├── Commands/                 # CQRS Commands
│   └── CreateBookingCommand.cs
│
├── Queries/                  # CQRS Queries
│   └── GetTourByIdQuery.cs
│
├── Mappings/                 # AutoMapper Profiles
│   └── TourProfile.cs
│
├── Validators/               # FluentValidation
│   └── TourValidator.cs
│
├── Jobs/                     # Hangfire Jobs
│   ├── EmailJob.cs
│   └── DataCleanupJob.cs
│
├── wwwroot/                  # Static files
│   ├── css/
│   ├── js/
│   └── images/
│
├── appsettings.json          # Configuration
├── Program.cs                # Application entry point
├── docker-compose.yml        # Docker configuration
└── Dockerfile                # Docker image
```

---

## 🔧 Cài Đặt & Chạy Dự Án

### Yêu Cầu Hệ Thống
- **.NET 8.0 SDK** hoặc cao hơn
- **SQL Server** (LocalDB hoặc SQL Server Express)
- **Redis** (optional, for caching)
- **RabbitMQ** (optional, for messaging)
- **Visual Studio 2022** hoặc **VS Code**

### Bước 1: Clone Repository
```bash
git clone <repository-url>
cd WEBDULICH
```

### Bước 2: Cấu Hình Database
1. Mở `appsettings.json`
2. Cập nhật connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost;Initial Catalog=WEBDULICH;Integrated Security=True;Encrypt=False;Trust Server Certificate=True"
  }
}
```

### Bước 3: Chạy Migration
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Bước 4: Cấu Hình Services (Optional)

#### Redis
```bash
# Install Redis (Windows)
# Download from: https://github.com/microsoftarchive/redis/releases

# Start Redis
redis-server
```

#### RabbitMQ
```bash
# Install RabbitMQ
# Download from: https://www.rabbitmq.com/download.html

# Start RabbitMQ
rabbitmq-server
```

### Bước 5: Cập Nhật Credentials
Chỉnh sửa `appsettings.json`:

```json
{
  "VNPay": {
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET"
  },
  "MoMo": {
    "PartnerCode": "YOUR_PARTNER_CODE",
    "AccessKey": "YOUR_ACCESS_KEY",
    "SecretKey": "YOUR_SECRET_KEY"
  },
  "GoogleAuth": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
  },
  "Email": {
    "SmtpUser": "your-email@gmail.com",
    "SmtpPassword": "your-app-password"
  }
}
```

### Bước 6: Build & Run
```bash
# Build project
dotnet build

# Run project
dotnet run

# Or with hot reload
dotnet watch run
```

Application sẽ chạy tại:
- **HTTPS**: https://localhost:5001
- **HTTP**: http://localhost:5000

---

## 🌐 API Endpoints

### Swagger Documentation
Truy cập: `https://localhost:5001/api-docs`

### Hangfire Dashboard
Truy cập: `https://localhost:5001/hangfire`

### Health Checks
Truy cập: `https://localhost:5001/health`

### SignalR Hubs
- **Notification Hub**: `/hubs/notification`
- **Chat Hub**: `/hubs/chat`

### Main API Groups

#### Payment APIs
```
POST   /api/payment/vnpay/create
POST   /api/payment/momo/create
GET    /api/payment/vnpay/return
GET    /api/payment/momo/return
POST   /api/payment/vnpay/refund
POST   /api/payment/momo/refund
```

#### Chatbot APIs
```
POST   /api/chatbot/message
GET    /api/chatbot/history/{id}
DELETE /api/chatbot/history/{id}
GET    /api/chatbot/analytics
```

#### Analytics APIs
```
GET /api/analytics/dashboard
GET /api/analytics/revenue
GET /api/analytics/popular-tours
GET /api/analytics/customer-segments
```

#### Social Auth APIs
```
POST   /api/socialauth/google
POST   /api/socialauth/facebook
POST   /api/socialauth/apple
```

#### Ticket APIs
```
POST /api/ticket/generate/{bookingId}
GET  /api/ticket/pdf/{bookingId}
GET  /api/ticket/validate/{ticketCode}
```

---

## 🐳 Docker Deployment

### Build Docker Image
```bash
docker build -t webdulich:latest .
```

### Run with Docker Compose
```bash
docker-compose up -d
```

Services included:
- **webdulich** - Main application
- **sqlserver** - SQL Server database
- **redis** - Redis cache
- **rabbitmq** - Message queue

---

## 🧪 Testing

### Run Unit Tests
```bash
dotnet test
```

### Test với Postman
1. Import Swagger JSON vào Postman
2. Tạo environment với base URL
3. Test các endpoints

### Test Payment
- **VNPay Sandbox**: https://sandbox.vnpayment.vn
- **MoMo Test**: https://developers.momo.vn

---

## 📊 Database Schema

### Main Tables
- **Users** - User accounts
- **Tours** - Tour information
- **Destinations** - Travel destinations
- **Categories** - Tour categories
- **Orders** - Booking orders
- **OrderDetails** - Order line items
- **Tickets** - E-tickets
- **Reviews** - Tour reviews
- **Payments** - Payment transactions
- **Bookings** - Hotel bookings
- **Notifications** - User notifications
- **ChatMessages** - Chat history
- **BlogPosts** - Blog content
- **Coupons** - Discount coupons
- **LoyaltyAccounts** - Loyalty program
- **EmailCampaigns** - Email marketing

---

## 🔐 Security Features

- ✅ **JWT Authentication** - Secure API access
- ✅ **OAuth 2.0** - Social login
- ✅ **HTTPS** - Encrypted communication
- ✅ **Rate Limiting** - DDoS protection
- ✅ **Input Validation** - SQL injection prevention
- ✅ **HMAC Signature** - Payment security
- ✅ **CORS** - Cross-origin resource sharing
- ✅ **XSS Protection** - Cross-site scripting prevention

---

## 📈 Performance Optimization

- ✅ **Redis Caching** - Fast data access
- ✅ **Response Compression** - Reduced bandwidth
- ✅ **Async/Await** - Non-blocking operations
- ✅ **Database Indexing** - Fast queries
- ✅ **Connection Pooling** - Efficient DB connections
- ✅ **CDN Ready** - Static file delivery

---

## 🛠️ Development Tools

### Recommended Extensions (VS Code)
- C# Dev Kit
- C# Extensions
- REST Client
- Docker
- GitLens

### Recommended Tools
- **Postman** - API testing
- **SQL Server Management Studio** - Database management
- **Redis Desktop Manager** - Redis GUI
- **RabbitMQ Management** - Queue monitoring

---

## 📝 Configuration

### appsettings.json Structure
```json
{
  "ConnectionStrings": { ... },
  "JWT": { ... },
  "VNPay": { ... },
  "MoMo": { ... },
  "GoogleAuth": { ... },
  "FacebookAuth": { ... },
  "AppleAuth": { ... },
  "Chatbot": { ... },
  "Ticket": { ... },
  "Analytics": { ... },
  "Email": { ... },
  "Redis": { ... },
  "RabbitMQ": { ... },
  "Hangfire": { ... },
  "Serilog": { ... }
}
```

---

## 🤝 Contributing

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License.

---

## 👥 Team

- **Developer**: Your Team Name
- **Contact**: your-email@example.com
- **Website**: https://yourwebsite.com

---

## 📞 Support

- **Documentation**: See `/docs` folder
- **Issues**: GitHub Issues
- **Email**: support@webdulich.local

---

## 🎯 Roadmap

### Phase 1 (Completed) ✅
- ✅ Basic tour management
- ✅ Booking system
- ✅ Payment integration
- ✅ User authentication

### Phase 2 (Completed) ✅
- ✅ AI Chatbot
- ✅ Advanced Analytics
- ✅ Social Login
- ✅ E-Ticket Generation

### Phase 3 (Planned) 🚧
- 🚧 Mobile app (React Native)
- 🚧 AI recommendations
- 🚧 Multi-language support
- 🚧 Advanced reporting

### Phase 4 (Future) 📅
- 📅 Blockchain integration
- 📅 AR/VR tour preview
- 📅 Voice assistant
- 📅 IoT integration

---

## 📚 Documentation

- [API Documentation](./docs/API.md)
- [Database Schema](./docs/DATABASE.md)
- [Deployment Guide](./DEPLOYMENT_GUIDE.md)
- [Feature Implementation](./NEW_FEATURES_IMPLEMENTATION.md)
- [Build Success Report](./BUILD_SUCCESS_FINAL.md)

---

## 🌟 Features Highlight

### 💡 Modern Architecture
- Clean Architecture
- SOLID Principles
- CQRS Pattern
- Repository Pattern
- Dependency Injection

### 🚀 Scalability
- Microservices ready
- Horizontal scaling
- Load balancing ready
- Cloud deployment ready

### 📱 Responsive Design
- Mobile-first approach
- Bootstrap 5
- Modern UI/UX
- Progressive Web App ready

---

## 🎉 Acknowledgments

- ASP.NET Core Team
- Entity Framework Team
- SignalR Team
- Hangfire Team
- All open-source contributors

---

**Made with ❤️ by WEBDULICH Team**

*Last Updated: April 29, 2026*

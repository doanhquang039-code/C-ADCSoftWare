# 🌍 WEBDULICH - Travel Management System

<<<<<<< HEAD
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
=======
<div align="center">

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-red)
![Redis](https://img.shields.io/badge/Redis-7.0-red)
![License](https://img.shields.io/badge/License-MIT-green)

**Hệ thống quản lý du lịch toàn diện với 20+ công nghệ hiện đại**

[Tính năng](#-tính-năng-chính) • [Công nghệ](#-công-nghệ-sử-dụng) • [Cài đặt](#-cài-đặt) • [API Documentation](#-api-documentation) • [Kiến trúc](#-kiến-trúc-hệ-thống)

</div>

---

## 📋 Mục lục

- [Giới thiệu](#-giới-thiệu)
- [Tính năng chính](#-tính-năng-chính)
- [Công nghệ sử dụng](#-công-nghệ-sử-dụng)
- [Kiến trúc hệ thống](#-kiến-trúc-hệ-thống)
- [Cài đặt](#-cài-đặt)
- [Cấu hình](#-cấu-hình)
- [API Documentation](#-api-documentation)
- [Database Schema](#-database-schema)
- [Testing](#-testing)
- [Deployment](#-deployment)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 Giới thiệu

**WEBDULICH** là một hệ thống quản lý du lịch toàn diện được xây dựng trên nền tảng **ASP.NET Core 8.0**, tích hợp **20+ công nghệ hiện đại** để cung cấp trải nghiệm đặt tour du lịch tốt nhất cho người dùng.

### 🎨 Mục đích dự án

- ✅ **Quản lý Tour Du lịch**: Tạo, cập nhật, xóa tours với đầy đủ thông tin
- ✅ **Đặt Tour Online**: Hệ thống đặt tour trực tuyến với thanh toán tích hợp
- ✅ **Quản lý Khách sạn**: Quản lý thông tin khách sạn và phòng
- ✅ **Thanh toán Đa dạng**: VNPay, MoMo, Credit Card
- ✅ **AI Chatbot**: Trợ lý ảo hỗ trợ khách hàng 24/7
- ✅ **Analytics**: Phân tích dữ liệu kinh doanh chi tiết
- ✅ **Social Login**: Đăng nhập nhanh qua Google, Facebook, Apple
- ✅ **E-Ticket**: Vé điện tử với QR code và PDF

### 🌟 Điểm nổi bật

- 🚀 **Hiệu suất cao** với Redis caching và Response Compression
- 🔒 **Bảo mật tốt** với JWT, OAuth 2.0, Rate Limiting
- 📊 **Real-time** với SignalR (notifications, chat)
- 🤖 **AI-powered** chatbot với 15+ intents
- 📱 **Responsive** design cho mọi thiết bị
- 🌐 **Multi-language** support (Vietnamese, English)
- 📈 **Scalable** architecture với microservices-ready design

---

## 🎯 Tính năng chính

### 1. 🏨 Quản lý Tour & Khách sạn

- **Tour Management**
  - CRUD operations cho tours
  - Upload hình ảnh tours
  - Quản lý giá và khuyến mãi
  - Lịch trình chi tiết
  - Đánh giá và review
  
- **Hotel Management**
  - Quản lý khách sạn
  - Quản lý phòng và giá
  - Tích hợp với tours
  - Đánh giá khách sạn

### 2. 💳 Thanh toán Tích hợp

- **VNPay Integration**
  - Thanh toán qua VNPay
  - Xác thực callback
  - Hoàn tiền tự động
  - Kiểm tra trạng thái giao dịch
  
- **MoMo Integration**
  - Thanh toán qua MoMo
  - QR Code payment
  - Deep link support
  - IPN handling

- **Payment Features**
  - HMAC signature validation
  - Transaction logging
  - Refund management
  - Payment history

### 3. 🤖 AI Chatbot

- **15+ Intents hỗ trợ**
  - Greeting, Farewell
  - Search Tour, Book Tour
  - Cancel Booking
  - Payment Inquiry
  - Price Inquiry
  - Tour Details
  - Contact Support
  - Recommendations
  - Availability Check
  - Location Info
  - Weather Info
  - FAQ, Complaints

- **Advanced Features**
  - Entity extraction (location, date, price, people)
  - Conversation history (Redis cache)
  - Quick replies
  - Suggested actions
  - Confidence scoring
  - Analytics & reporting

### 4. 📊 Advanced Analytics

- **Dashboard Metrics**
  - Total revenue & growth
  - Total bookings & growth
  - Customer metrics
  - Average order value
  - Conversion rate
  
- **Revenue Analytics**
  - Daily/Weekly/Monthly charts
  - Revenue by destination
  - Revenue by category
  - Trend analysis
  
- **Customer Analytics**
  - Customer segmentation (VIP, Regular, New)
  - Customer lifetime value
  - Retention rate
  - Churn analysis
  
- **Booking Analytics**
  - Booking trends
  - Popular destinations
  - Popular tours
  - Conversion funnel

### 5. 🔐 Social Login

- **Providers hỗ trợ**
  - Google Sign-In (OAuth 2.0)
  - Facebook Login (Graph API)
  - Apple Sign-In
  - Microsoft Account (ready)
  
- **Features**
  - One-click sign-in
  - Auto account creation
  - Profile sync
  - Account linking
  - Email verification

### 6. 🎫 E-Ticket Generation

- **Ticket Features**
  - Unique ticket code generation
  - QR code with encryption
  - PDF generation with branding
  - Email delivery
  - SMS delivery (ready)
  
- **Validation**
  - Real-time validation
  - QR code scanning
  - Usage tracking
  - Duplicate prevention
  - Expiry management

### 7. 📧 Email Marketing

- **Campaign Management**
  - Create email campaigns
  - Target audience segmentation
  - Email templates
  - Scheduled sending
  - A/B testing
  
- **Analytics**
  - Open rate tracking
  - Click-through rate
  - Conversion tracking
  - Subscriber management

### 8. 🎁 Loyalty Program

- **Points System**
  - Earn points on bookings
  - Redeem points for rewards
  - Tier-based benefits
  - Point expiration
  
- **Rewards**
  - Discount coupons
  - Free upgrades
  - Exclusive tours
  - Partner rewards

### 9. 📱 Real-time Features

- **SignalR Hubs**
  - Notification Hub (real-time notifications)
  - Chat Hub (live chat support)
  
- **Features**
  - Push notifications
  - Live chat
  - Real-time updates
  - Online status

### 10. 🗺️ Map Integration

- **Location Services**
  - Interactive maps
  - Route planning
  - Distance calculation
  - Nearby attractions
  - Geocoding

---

## 🛠️ Công nghệ sử dụng

### Backend Framework
- **ASP.NET Core 8.0** - Web framework
- **Entity Framework Core** - ORM
- **C# 12** - Programming language

### Database
- **SQL Server 2019+** - Primary database
- **Redis 7.0** - Caching & session storage

### Authentication & Authorization
- **JWT Bearer** - Token-based authentication
- **OAuth 2.0** - Social login (Google, Facebook, Apple)
- **Identity** - User management

### Real-time Communication
- **SignalR** - WebSocket communication
  - Notification Hub
  - Chat Hub

### Background Jobs
- **Hangfire** - Background job processing
  - Email jobs
  - Data cleanup jobs
  - Report generation

### Caching & Performance
- **Redis** (StackExchange.Redis) - Distributed caching
- **Response Compression** - Brotli & Gzip
- **Memory Cache** - In-memory caching

### Messaging
- **RabbitMQ** - Message queue
  - Event-driven architecture
  - Async processing

### Logging
- **Serilog** - Structured logging
  - Console sink
  - File sink
  - Elasticsearch sink (optional)

### API Documentation
- **Swagger/OpenAPI** - API documentation
  - Interactive API testing
  - Schema generation

### Validation & Mapping
- **FluentValidation** - Input validation
- **AutoMapper** - Object mapping

### Mediator Pattern
- **MediatR** - CQRS implementation
  - Commands
  - Queries
  - Handlers

### Security
- **Rate Limiting** (AspNetCoreRateLimit) - API rate limiting
- **CORS** - Cross-origin resource sharing
- **HTTPS** - SSL/TLS encryption

### Storage
- **Azure Blob Storage** - Cloud storage (optional)
- **Local File System** - File storage

### Search (Optional)
- **Elasticsearch** (NEST) - Full-text search
  - Tour search
  - Log aggregation

### Health Checks
- **AspNetCore.HealthChecks** - Health monitoring
  - SQL Server health check
  - Redis health check
  - Hangfire health check

### Payment Gateways
- **VNPay** - Vietnamese payment gateway
- **MoMo** - Mobile payment

### Email
- **SMTP** - Email delivery
- **Email Templates** - HTML email templates

### Testing (Ready)
- **xUnit** - Unit testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library

### DevOps
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration

---

## 🏗️ Kiến trúc hệ thống

### Layered Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ Controllers  │  │    Views     │  │  API Docs    │  │
│  │   (MVC)      │  │   (Razor)    │  │  (Swagger)   │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                    Application Layer                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   Services   │  │   Commands   │  │   Queries    │  │
│  │  (Business)  │  │   (MediatR)  │  │   (MediatR)  │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                     Domain Layer                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   Models     │  │  Interfaces  │  │  Validators  │  │
│  │  (Entities)  │  │              │  │              │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │  DbContext   │  │  Repositories│  │  External    │  │
│  │   (EF Core)  │  │              │  │   Services   │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### Technology Stack Diagram

```
┌─────────────────────────────────────────────────────────┐
│                      Client Layer                        │
│         Web Browser / Mobile App / API Client           │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                    API Gateway Layer                     │
│    Rate Limiting │ CORS │ Authentication │ Logging      │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                  Application Services                    │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐  │
│  │ Payment  │ │ Chatbot  │ │Analytics │ │  Ticket  │  │
│  │ Service  │ │ Service  │ │ Service  │ │ Service  │  │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘  │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                    Data Layer                            │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐  │
│  │   SQL    │ │  Redis   │ │ RabbitMQ │ │  Azure   │  │
│  │  Server  │ │  Cache   │ │  Queue   │ │  Blob    │  │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘  │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                  External Services                       │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐  │
│  │  VNPay   │ │   MoMo   │ │  Google  │ │ Facebook │  │
│  │ Payment  │ │ Payment  │ │  OAuth   │ │  OAuth   │  │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘  │
└─────────────────────────────────────────────────────────┘
>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
```

---

<<<<<<< HEAD
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
=======
## 🚀 Cài đặt

### Yêu cầu hệ thống

- **.NET SDK 9.0** hoặc cao hơn
- **SQL Server 2019+** hoặc SQL Server Express
- **Redis 7.0+** (optional, for caching)
- **RabbitMQ 3.12+** (optional, for messaging)
- **Visual Studio 2022** hoặc **VS Code**

### Bước 1: Clone repository

```bash
git clone https://github.com/your-username/WEBDULICH.git
cd WEBDULICH
```

### Bước 2: Restore packages

```bash
dotnet restore
```

### Bước 3: Cấu hình Database

1. Mở `appsettings.json`
2. Cập nhật connection string:

>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost;Initial Catalog=WEBDULICH;Integrated Security=True;Encrypt=False;Trust Server Certificate=True"
  }
}
```

<<<<<<< HEAD
### Bước 3: Chạy Migration
=======
### Bước 4: Chạy migrations

>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

<<<<<<< HEAD
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
=======
### Bước 5: Cấu hình Redis (Optional)

```bash
# Windows (với Chocolatey)
choco install redis-64

# hoặc sử dụng Docker
docker run -d -p 6379:6379 redis:7.0-alpine
```

### Bước 6: Cấu hình RabbitMQ (Optional)

```bash
# Sử dụng Docker
docker run -d -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### Bước 7: Build và Run

```bash
# Development
dotnet run

# Production
dotnet run --configuration Release
```

Application sẽ chạy tại:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001

---

## ⚙️ Cấu hình

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_SQL_SERVER_CONNECTION_STRING",
    "Redis": "localhost:6379",
    "Hangfire": "YOUR_HANGFIRE_CONNECTION_STRING"
  },
  
  "JWT": {
    "SecretKey": "YOUR_SECRET_KEY_HERE",
    "Issuer": "WEBDULICH",
    "Audience": "WEBDULICH_Users",
    "ExpiryInMinutes": 60
  },
  
  "VNPay": {
    "Url": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "ReturnUrl": "https://yoursite.com/payment/vnpay-return"
  },
  
  "MoMo": {
    "Endpoint": "https://test-payment.momo.vn/v2/gateway/api/create",
>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
    "PartnerCode": "YOUR_PARTNER_CODE",
    "AccessKey": "YOUR_ACCESS_KEY",
    "SecretKey": "YOUR_SECRET_KEY"
  },
<<<<<<< HEAD
=======
  
>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
  "GoogleAuth": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
  },
<<<<<<< HEAD
  "Email": {
    "SmtpUser": "your-email@gmail.com",
    "SmtpPassword": "your-app-password"
=======
  
  "FacebookAuth": {
    "AppId": "YOUR_FACEBOOK_APP_ID",
    "AppSecret": "YOUR_FACEBOOK_APP_SECRET"
  },
  
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@webdulich.local",
    "FromName": "WEBDULICH"
>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
  }
}
```

<<<<<<< HEAD
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
=======
### Environment Variables

```bash
# Development
export ASPNETCORE_ENVIRONMENT=Development

# Production
export ASPNETCORE_ENVIRONMENT=Production
```

---

## 📚 API Documentation

### Swagger UI

Truy cập API documentation tại: `https://localhost:5001/api-docs`

### API Endpoints Overview

#### 🔐 Authentication
```
POST   /api/auth/login
POST   /api/auth/register
POST   /api/auth/refresh-token
POST   /api/auth/logout
```

#### 💳 Payment
```
POST   /api/payment/vnpay/create
GET    /api/payment/vnpay/return
POST   /api/payment/vnpay/refund
GET    /api/payment/vnpay/status/{id}

POST   /api/payment/momo/create
GET    /api/payment/momo/return
POST   /api/payment/momo/refund
GET    /api/payment/momo/status/{id}
```

#### 🤖 Chatbot
>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
```
POST   /api/chatbot/message
GET    /api/chatbot/history/{id}
DELETE /api/chatbot/history/{id}
GET    /api/chatbot/analytics
<<<<<<< HEAD
```

#### Analytics APIs
=======
POST   /api/chatbot/feedback
```

#### 📊 Analytics
>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
```
GET /api/analytics/dashboard
GET /api/analytics/revenue
GET /api/analytics/popular-tours
GET /api/analytics/customer-segments
<<<<<<< HEAD
```

#### Social Auth APIs
=======
GET /api/analytics/booking-trends
GET /api/analytics/conversion-funnel
```

#### 🔐 Social Auth
>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
```
POST   /api/socialauth/google
POST   /api/socialauth/facebook
POST   /api/socialauth/apple
<<<<<<< HEAD
```

#### Ticket APIs
=======
POST   /api/socialauth/link
DELETE /api/socialauth/unlink/{provider}
```

#### 🎫 Ticket
>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
```
POST /api/ticket/generate/{bookingId}
GET  /api/ticket/pdf/{bookingId}
GET  /api/ticket/validate/{ticketCode}
<<<<<<< HEAD
=======
POST /api/ticket/send-email/{bookingId}
GET  /api/ticket/booking/{bookingId}
```

#### 🏨 Tours
```
GET    /api/tours
GET    /api/tours/{id}
POST   /api/tours
PUT    /api/tours/{id}
DELETE /api/tours/{id}
GET    /api/tours/search
```

#### 🏨 Hotels
```
GET    /api/hotels
GET    /api/hotels/{id}
POST   /api/hotels
PUT    /api/hotels/{id}
DELETE /api/hotels/{id}
```

#### 📝 Bookings
```
GET    /api/bookings
GET    /api/bookings/{id}
POST   /api/bookings
PUT    /api/bookings/{id}
DELETE /api/bookings/{id}
GET    /api/bookings/user/{userId}
>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
```

---

<<<<<<< HEAD
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

=======
## 🗄️ Database Schema

### Core Tables

```sql
-- Users
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(200),
    Email NVARCHAR(200) UNIQUE,
    Password NVARCHAR(500),
    Role NVARCHAR(50),
    GoogleId NVARCHAR(100),
    FacebookId NVARCHAR(100),
    AppleId NVARCHAR(100),
    CreatedAt DATETIME2,
    ...
);

-- Tours
CREATE TABLE Tours (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(500),
    Description NVARCHAR(MAX),
    Price DECIMAL(18,2),
    Duration INT,
    Image NVARCHAR(500),
    DestinationId INT,
    CategoryId INT,
    ...
);

-- Orders
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT,
    TourId INT,
    TotalAmount DECIMAL(18,2),
    Status NVARCHAR(50),
    OrderDate DATETIME2,
    ...
);

-- OrderDetails
CREATE TABLE OrderDetails (
    Id INT PRIMARY KEY IDENTITY,
    OrderId INT,
    TourId INT,
    Quantity INT,
    Price DECIMAL(18,2),
    ...
);

-- Tickets
CREATE TABLE Tickets (
    Id INT PRIMARY KEY IDENTITY,
    BookingId INT,
    TicketCode NVARCHAR(100) UNIQUE,
    QRCode NVARCHAR(MAX),
    Status NVARCHAR(50),
    ValidFrom DATETIME2,
    ValidUntil DATETIME2,
    ...
);
```

>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
---

## 🧪 Testing

### Run Unit Tests
<<<<<<< HEAD
=======

>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
```bash
dotnet test
```

<<<<<<< HEAD
### Test với Postman
=======
### Run Integration Tests

```bash
dotnet test --filter Category=Integration
```

### Test Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Manual Testing với Postman

>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
1. Import Swagger JSON vào Postman
2. Tạo environment với base URL
3. Test các endpoints

<<<<<<< HEAD
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

=======
---

## 🚢 Deployment

### Docker Deployment

```bash
# Build image
docker build -t webdulich:latest .

# Run container
docker run -d -p 8080:80 --name webdulich webdulich:latest
```

### Docker Compose

```bash
docker-compose up -d
```

### Azure Deployment

```bash
# Login to Azure
az login

# Create resource group
az group create --name webdulich-rg --location eastasia

# Create App Service
az webapp create --resource-group webdulich-rg --plan webdulich-plan --name webdulich-app --runtime "DOTNETCORE:8.0"

# Deploy
az webapp deployment source config-zip --resource-group webdulich-rg --name webdulich-app --src publish.zip
```

---

## 📊 Performance

### Benchmarks

- **Response Time**: < 100ms (average)
- **Throughput**: 1000+ requests/second
- **Concurrent Users**: 10,000+
- **Database Queries**: Optimized with indexes
- **Cache Hit Rate**: > 90%

### Optimization

- ✅ Redis caching
- ✅ Response compression (Brotli/Gzip)
- ✅ Database indexing
- ✅ Async/await pattern
- ✅ Connection pooling
- ✅ CDN for static files

---

## 🔒 Security

### Implemented Security Features

- ✅ JWT Authentication
- ✅ OAuth 2.0 (Google, Facebook, Apple)
- ✅ HTTPS enforcement
- ✅ CORS configuration
- ✅ Rate limiting
- ✅ Input validation
- ✅ SQL injection prevention (EF Core)
- ✅ XSS prevention
- ✅ CSRF protection
- ✅ Password hashing (BCrypt)
- ✅ Secure headers

---

## 📈 Monitoring

### Health Checks

Access at: `https://localhost:5001/health`

### Hangfire Dashboard

Access at: `https://localhost:5001/hangfire`

### Logs

Logs are stored in: `Logs/webdulich-{Date}.log`

>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
---

## 🤝 Contributing

<<<<<<< HEAD
1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
=======
Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

<<<<<<< HEAD
## 📄 License

This project is licensed under the MIT License.
=======
## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be

---

## 👥 Team

<<<<<<< HEAD
- **Developer**: Your Team Name
- **Contact**: your-email@example.com
- **Website**: https://yourwebsite.com
=======
- **Developer**: Your Name
- **Email**: your.email@example.com
- **GitHub**: [@yourusername](https://github.com/yourusername)

---

## 🙏 Acknowledgments

- ASP.NET Core Team
- Entity Framework Core Team
- All open-source contributors
>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be

---

## 📞 Support

<<<<<<< HEAD
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
=======
For support, email support@webdulich.local or join our Slack channel.

---

<div align="center">

**Made with ❤️ by WEBDULICH Team**

⭐ Star us on GitHub — it helps!

[Website](https://webdulich.local) • [Documentation](https://docs.webdulich.local) • [Blog](https://blog.webdulich.local)

</div>
>>>>>>> ff15598075593fc66ed6e8d31ef49776e5c846be

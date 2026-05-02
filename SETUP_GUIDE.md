# 🚀 HƯỚNG DẪN CÀI ĐẶT VÀ SỬ DỤNG WEBDULICH

**Ngày cập nhật**: 2 Tháng 5, 2026  
**Phiên bản**: 2.0  
**Framework**: ASP.NET Core 8.0

---

## 📋 MỤC LỤC

1. [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
2. [Cài đặt cơ bản](#cài-đặt-cơ-bản)
3. [Cấu hình Database](#cấu-hình-database)
4. [Cấu hình API Keys](#cấu-hình-api-keys)
5. [Chạy ứng dụng](#chạy-ứng-dụng)
6. [Tính năng mới](#tính-năng-mới)
7. [API Documentation](#api-documentation)
8. [Troubleshooting](#troubleshooting)

---

## 🖥️ YÊU CẦU HỆ THỐNG

### Phần mềm cần thiết:
- ✅ **.NET 8.0 SDK** hoặc cao hơn
- ✅ **SQL Server** (LocalDB hoặc SQL Server Express)
- ✅ **Redis** (tùy chọn - cho caching)
- ✅ **Visual Studio 2022** hoặc **VS Code**

### Kiểm tra phiên bản:
```bash
# Kiểm tra .NET version
dotnet --version

# Kiểm tra SQL Server
sqlcmd -S localhost -Q "SELECT @@VERSION"
```

---

## 📦 CÀI ĐẶT CƠ BẢN

### 1. Clone hoặc tải project:
```bash
cd D:\Users\admoi\source\repos\DoanhNoVip\WEBDULICH
```

### 2. Restore dependencies:
```bash
dotnet restore
```

### 3. Build project:
```bash
dotnet build
```

**Kết quả mong đợi:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## 🗄️ CẤU HÌNH DATABASE

### 1. Cập nhật Connection String

Mở file `appsettings.json` và cập nhật connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost;Initial Catalog=WEBDULICH;Integrated Security=True;Encrypt=False;Trust Server Certificate=True",
    "Redis": "localhost:6379,abortConnect=false",
    "Hangfire": "Data Source=localhost;Initial Catalog=WEBDULICH_Hangfire;Integrated Security=True;Encrypt=False;Trust Server Certificate=True"
  }
}
```

### 2. Tạo Database

```bash
# Tạo migration (nếu chưa có)
dotnet ef migrations add InitialCreate

# Cập nhật database
dotnet ef database update
```

### 3. Seed dữ liệu mẫu

Ứng dụng sẽ tự động seed dữ liệu khi chạy lần đầu tiên.

---

## 🔑 CẤU HÌNH API KEYS

### 1. Weather Service (OpenWeatherMap)

**Đăng ký API Key:**
1. Truy cập: https://openweathermap.org/api
2. Đăng ký tài khoản miễn phí
3. Lấy API Key từ dashboard

**Cập nhật trong `appsettings.json`:**
```json
{
  "Weather": {
    "ApiKey": "YOUR_OPENWEATHERMAP_API_KEY",
    "ApiBaseUrl": "https://api.openweathermap.org/data/2.5",
    "CacheDurationMinutes": 30,
    "DefaultLanguage": "vi"
  }
}
```

### 2. Currency Service (Exchange Rate API)

**Đăng ký API Key:**
1. Truy cập: https://www.exchangerate-api.com/
2. Đăng ký tài khoản miễn phí
3. Lấy API Key

**Cập nhật trong `appsettings.json`:**
```json
{
  "Currency": {
    "ApiKey": "YOUR_EXCHANGE_RATE_API_KEY",
    "ApiBaseUrl": "https://api.exchangerate-api.com/v4/latest",
    "CacheDurationMinutes": 60,
    "BaseCurrency": "VND"
  }
}
```

### 3. Payment Gateways

**VNPay:**
```json
{
  "VNPay": {
    "Url": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "ReturnUrl": "https://localhost:5000/payment/vnpay-return",
    "IpnUrl": "https://localhost:5000/payment/vnpay-ipn"
  }
}
```

**MoMo:**
```json
{
  "MoMo": {
    "Endpoint": "https://test-payment.momo.vn/v2/gateway/api/create",
    "PartnerCode": "YOUR_PARTNER_CODE",
    "AccessKey": "YOUR_ACCESS_KEY",
    "SecretKey": "YOUR_SECRET_KEY",
    "ReturnUrl": "https://localhost:5000/payment/momo-return",
    "IpnUrl": "https://localhost:5000/payment/momo-ipn"
  }
}
```

### 4. Social Authentication

**Google:**
```json
{
  "GoogleAuth": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
  }
}
```

**Facebook:**
```json
{
  "FacebookAuth": {
    "AppId": "YOUR_FACEBOOK_APP_ID",
    "AppSecret": "YOUR_FACEBOOK_APP_SECRET"
  }
}
```

---

## ▶️ CHẠY ỨNG DỤNG

### Cách 1: Sử dụng dotnet CLI

```bash
# Chạy ở chế độ development
dotnet run

# Hoặc chạy với watch (tự động reload khi có thay đổi)
dotnet watch run
```

### Cách 2: Sử dụng Visual Studio

1. Mở file `WEBDULICH.sln`
2. Nhấn `F5` hoặc click nút **Run**

### Cách 3: Sử dụng VS Code

1. Mở folder WEBDULICH trong VS Code
2. Nhấn `F5` hoặc chọn **Run > Start Debugging**

### Truy cập ứng dụng:

- **Website**: https://localhost:5001 hoặc http://localhost:5000
- **API Docs**: https://localhost:5001/api-docs
- **Hangfire Dashboard**: https://localhost:5001/hangfire
- **Health Check**: https://localhost:5001/health

---

## 🎯 TÍNH NĂNG MỚI

### 1. 🌤️ Weather Service

**Mô tả**: Dự báo thời tiết cho điểm đến du lịch

**API Endpoints:**
```
GET /api/weather/current/{location}
GET /api/weather/forecast/{location}?days=7
GET /api/weather/coordinates?latitude=10.8231&longitude=106.6297
GET /api/weather/suitable/{location}
GET /api/weather/best-months/{location}
```

**Ví dụ sử dụng:**
```javascript
// Lấy thời tiết hiện tại
const response = await fetch('/api/weather/current/Hà Nội');
const data = await response.json();
console.log(data.data.temperature); // 25°C
```

### 2. 💱 Currency Converter

**Mô tả**: Chuyển đổi tiền tệ real-time với 12 loại tiền

**API Endpoints:**
```
GET /api/currency/convert?amount=1000000&from=VND&to=USD
GET /api/currency/rate?from=VND&to=USD
GET /api/currency/currencies
GET /api/currency/rates/VND
GET /api/currency/format?amount=1000000&currency=VND
```

**Tiền tệ hỗ trợ:**
- VND, USD, EUR, GBP, JPY, CNY, KRW, THB, SGD, AUD, CAD, HKD

**Ví dụ sử dụng:**
```javascript
// Chuyển đổi VND sang USD
const response = await fetch('/api/currency/convert?amount=5000000&from=VND&to=USD');
const data = await response.json();
console.log(data.data.convertedAmount); // 200
```

### 3. 🎯 Tour Recommendation Engine

**Mô tả**: Gợi ý tour thông minh dựa trên AI và machine learning

**API Endpoints:**
```
GET /api/recommendation/personalized?count=10
GET /api/recommendation/similar/{tourId}?count=5
GET /api/recommendation/trending?count=10
POST /api/recommendation/by-preferences
GET /api/recommendation/collaborative/{tourId}?count=5
GET /api/recommendation/seasonal?month=12&count=10
POST /api/recommendation/track
```

**Ví dụ sử dụng:**
```javascript
// Lấy gợi ý cá nhân hóa
const response = await fetch('/api/recommendation/personalized?count=10');
const data = await response.json();
console.log(data.data); // Array of recommended tours

// Gợi ý theo sở thích
const preferences = {
  preferredDestinations: ['Phú Quốc', 'Nha Trang'],
  minBudget: 5000000,
  maxBudget: 10000000,
  preferredDuration: 3
};

const response = await fetch('/api/recommendation/by-preferences', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(preferences)
});
```

### 4. 💳 Payment Gateways

**Mô tả**: Tích hợp VNPay và MoMo

**API Endpoints:**
```
POST /api/payment/vnpay/create
GET /api/payment/vnpay/return
POST /api/payment/vnpay/ipn

POST /api/payment/momo/create
GET /api/payment/momo/return
POST /api/payment/momo/ipn
```

### 5. 🤖 AI Chatbot

**Mô tả**: Chatbot hỗ trợ khách hàng 24/7

**API Endpoints:**
```
POST /api/chatbot/message
GET /api/chatbot/history
DELETE /api/chatbot/clear-history
GET /api/chatbot/suggestions
```

### 6. 📊 Analytics Dashboard

**Mô tả**: Thống kê và phân tích dữ liệu

**API Endpoints:**
```
GET /api/analytics/overview
GET /api/analytics/revenue?startDate=2026-01-01&endDate=2026-12-31
GET /api/analytics/popular-tours?limit=10
GET /api/analytics/user-growth
GET /api/analytics/booking-trends
```

### 7. 🎫 Ticket Management

**Mô tả**: Quản lý vé điện tử với QR code

**API Endpoints:**
```
POST /api/ticket/generate
GET /api/ticket/{ticketId}
GET /api/ticket/verify/{ticketCode}
POST /api/ticket/send-email
GET /api/ticket/user/{userId}
```

### 8. 🔐 Social Authentication

**Mô tả**: Đăng nhập bằng Google, Facebook, Apple

**API Endpoints:**
```
POST /api/auth/google
POST /api/auth/facebook
POST /api/auth/apple
GET /api/auth/profile
```

---

## 📚 API DOCUMENTATION

### Swagger UI

Truy cập: **https://localhost:5001/api-docs**

Swagger UI cung cấp:
- ✅ Danh sách tất cả API endpoints
- ✅ Mô tả chi tiết từng endpoint
- ✅ Request/Response schemas
- ✅ Try it out - Test API trực tiếp
- ✅ Authentication với JWT Bearer token

### Postman Collection

Import Postman collection từ file `WEBDULICH.postman_collection.json` (nếu có)

---

## 🔧 TROUBLESHOOTING

### Lỗi: "Build failed - File is locked"

**Nguyên nhân**: Ứng dụng đang chạy

**Giải pháp**:
```bash
# Tìm process đang chạy
tasklist | findstr WEBDULICH

# Kill process (thay PID bằng số thực tế)
taskkill /F /PID <PID>

# Build lại
dotnet build
```

### Lỗi: "Cannot connect to SQL Server"

**Giải pháp**:
1. Kiểm tra SQL Server đang chạy
2. Kiểm tra connection string trong `appsettings.json`
3. Thử connection string khác:
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WEBDULICH;Trusted_Connection=True;"
```

### Lỗi: "Redis connection failed"

**Giải pháp**:
1. Cài đặt Redis (tùy chọn)
2. Hoặc comment Redis configuration trong `Program.cs`
3. Ứng dụng sẽ dùng in-memory cache thay thế

### Lỗi: "Weather API returns 401"

**Giải pháp**:
1. Kiểm tra API key trong `appsettings.json`
2. Đảm bảo API key còn hiệu lực
3. Kiểm tra quota của free plan

### Lỗi: "Port 5000 already in use"

**Giải pháp**:
```bash
# Tìm process đang dùng port 5000
netstat -ano | findstr :5000

# Kill process
taskkill /F /PID <PID>
```

---

## 🎨 TÍCH HỢP VÀO UI

### Weather Widget

```html
<div class="weather-widget" data-location="Phú Quốc">
  <div class="weather-icon"></div>
  <div class="weather-temp"></div>
  <div class="weather-condition"></div>
</div>

<script>
async function loadWeather(location) {
  const res = await fetch(`/api/weather/current/${location}`);
  const data = await res.json();
  
  document.querySelector('.weather-temp').textContent = 
    `${data.data.temperature}°C`;
  document.querySelector('.weather-condition').textContent = 
    data.data.condition;
}
</script>
```

### Currency Converter

```html
<div class="price-converter">
  <span class="price-vnd">5.000.000 ₫</span>
  <select class="currency-selector" onchange="convertPrice()">
    <option value="USD">USD</option>
    <option value="EUR">EUR</option>
    <option value="GBP">GBP</option>
  </select>
  <span class="price-converted"></span>
</div>

<script>
async function convertPrice() {
  const amount = 5000000;
  const currency = document.querySelector('.currency-selector').value;
  
  const res = await fetch(
    `/api/currency/convert?amount=${amount}&from=VND&to=${currency}`
  );
  const data = await res.json();
  
  document.querySelector('.price-converted').textContent = 
    data.data.formattedConverted;
}
</script>
```

### Recommendation Section

```html
<section class="recommendations">
  <h2>Tour dành riêng cho bạn</h2>
  <div class="tour-grid" id="recommended-tours"></div>
</section>

<script>
async function loadRecommendations() {
  const res = await fetch('/api/recommendation/personalized?count=8');
  const data = await res.json();
  
  const grid = document.getElementById('recommended-tours');
  data.data.forEach(tour => {
    grid.innerHTML += `
      <div class="tour-card">
        <img src="${tour.image}" alt="${tour.name}">
        <h3>${tour.name}</h3>
        <p>${tour.price.toLocaleString('vi-VN')} ₫</p>
      </div>
    `;
  });
}
</script>
```

---

## 📊 PERFORMANCE TIPS

### 1. Enable Redis Caching

Redis giúp tăng tốc độ ứng dụng đáng kể:
- Weather data: cache 30 phút
- Currency rates: cache 1 giờ
- Recommendations: cache 1 giờ

### 2. Enable Response Compression

Đã được cấu hình sẵn trong `Program.cs`:
- Brotli compression
- Gzip compression

### 3. Database Indexing

Đảm bảo các bảng quan trọng có index:
```sql
CREATE INDEX IX_Tours_DestinationId ON Tours(DestinationId);
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_TourId ON Orders(TourId);
```

---

## 🔒 SECURITY CHECKLIST

- ✅ JWT Authentication đã được cấu hình
- ✅ HTTPS redirect enabled
- ✅ Rate limiting enabled
- ✅ CORS policy configured
- ✅ SQL injection protection (Entity Framework)
- ✅ XSS protection (Razor encoding)
- ⚠️ **Cần làm**: Thay đổi JWT SecretKey trong production
- ⚠️ **Cần làm**: Cấu hình CORS cho production domain
- ⚠️ **Cần làm**: Enable HSTS trong production

---

## 📞 HỖ TRỢ

### Liên hệ:
- **Email**: support@webdulich.local
- **GitHub Issues**: [Link to repository]
- **Documentation**: https://localhost:5001/api-docs

### Tài liệu tham khảo:
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Hangfire Documentation](https://docs.hangfire.io)
- [SignalR Documentation](https://docs.microsoft.com/aspnet/core/signalr)

---

## 🎉 HOÀN TẤT!

**WEBDULICH đã sẵn sàng để sử dụng!**

Chúc bạn phát triển thành công! 🚀

---

*Được xây dựng với ❤️ bởi Kiro AI*  
*Ngày cập nhật: 2 Tháng 5, 2026*  
*Version: 2.0*

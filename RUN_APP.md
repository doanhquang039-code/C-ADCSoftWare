# 🚀 CHẠY ỨNG DỤNG WEBDULICH

## ✅ ĐÃ HOÀN THÀNH

1. ✅ Build thành công (0 errors)
2. ✅ Database schema đã cập nhật
3. ✅ Hangfire database đã tạo
4. ✅ Tất cả services đã đăng ký

---

## 🎯 CHẠY NGAY BÂY GIỜ

### Cách 1: Chạy với port mặc định (5000/5001)

```bash
cd D:\Users\admoi\source\repos\DoanhNoVip\WEBDULICH
dotnet run
```

### Cách 2: Chạy với port tùy chỉnh (khuyến nghị)

```bash
cd D:\Users\admoi\source\repos\DoanhNoVip\WEBDULICH
dotnet run --urls "http://localhost:5555;https://localhost:5556"
```

### Cách 3: Chạy với watch mode (auto-reload)

```bash
cd D:\Users\admoi\source\repos\DoanhNoVip\WEBDULICH
dotnet watch run
```

---

## 🌐 TRUY CẬP ỨNG DỤNG

Sau khi chạy thành công, truy cập:

### Website
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Custom**: http://localhost:5555 (nếu dùng cách 2)

### API Documentation
- **Swagger UI**: http://localhost:5000/api-docs
- Xem tất cả 100+ API endpoints
- Test API trực tiếp từ browser

### Hangfire Dashboard
- **URL**: http://localhost:5000/hangfire
- Quản lý background jobs
- Xem job history và statistics

### Health Check
- **URL**: http://localhost:5000/health
- Kiểm tra trạng thái hệ thống
- Database, Redis, Hangfire status

---

## 🎨 TÍNH NĂNG MỚI

### 1. Weather Service
```
GET http://localhost:5000/api/weather/current/Hà Nội
GET http://localhost:5000/api/weather/forecast/Phú Quốc?days=7
```

### 2. Currency Converter
```
GET http://localhost:5000/api/currency/convert?amount=5000000&from=VND&to=USD
GET http://localhost:5000/api/currency/rates/VND
```

### 3. Tour Recommendations
```
GET http://localhost:5000/api/recommendation/personalized?count=10
GET http://localhost:5000/api/recommendation/trending
```

### 4. AI Chatbot
```
POST http://localhost:5000/api/chatbot/message
GET http://localhost:5000/api/chatbot/history
```

### 5. Analytics
```
GET http://localhost:5000/api/analytics/overview
GET http://localhost:5000/api/analytics/revenue
```

---

## 📝 ĐĂNG NHẬP

### Admin Account
- **Email**: admin@webdulich.local
- **Password**: Admin@123

### Test User
- **Email**: user@webdulich.local
- **Password**: User@123

---

## 🔧 NẾU GẶP LỖI

### Lỗi: Port đang được sử dụng

```bash
# Tìm process
netstat -ano | findstr :5000

# Kill process
taskkill /F /PID <PID>

# Hoặc dùng port khác
dotnet run --urls "http://localhost:5555"
```

### Lỗi: Cannot connect to database

```bash
# Kiểm tra SQL Server
sc query MSSQLSERVER

# Start SQL Server
net start MSSQLSERVER

# Kiểm tra connection string trong appsettings.json
```

### Lỗi: Hangfire migration failed

Không sao! Ứng dụng vẫn chạy được. Hangfire sẽ tự động tạo tables khi chạy lần đầu.

---

## 📊 KIỂM TRA TRẠNG THÁI

### Sau khi chạy, bạn sẽ thấy:

```
[09:30:10 INF] Starting WEBDULICH application
[09:30:10 INF] Redis caching configured
[09:30:10 INF] Hangfire configured
[09:30:10 INF] JWT Authentication configured
[09:30:10 INF] Rate limiting configured
[09:30:10 INF] Response compression configured
[09:30:10 INF] SignalR configured
[09:30:10 INF] AutoMapper configured
[09:30:10 INF] FluentValidation configured
[09:30:10 INF] MediatR configured
[09:30:10 INF] Swagger configured
[09:30:10 INF] RabbitMQ configured
[09:30:10 INF] Health checks configured
[09:30:10 INF] New feature services registered (Payment, Chatbot, Analytics, Social Auth, Ticket, Weather, Currency, Recommendation)
[09:30:10 INF] All services registered
[09:30:10 INF] Data seeding completed
[09:30:10 INF] Application started successfully
[09:30:10 INF] Now listening on: http://localhost:5000
[09:30:10 INF] Now listening on: https://localhost:5001
```

---

## 🎉 THÀNH CÔNG!

Khi thấy dòng:
```
Now listening on: http://localhost:5000
```

Ứng dụng đã chạy thành công! 🚀

Mở browser và truy cập:
- **Website**: http://localhost:5000
- **API Docs**: http://localhost:5000/api-docs

---

## 📚 TÀI LIỆU THAM KHẢO

- [README.md](README.md) - Tổng quan project
- [SETUP_GUIDE.md](SETUP_GUIDE.md) - Hướng dẫn cài đặt chi tiết
- [NEW_FEATURES_ADDED.md](NEW_FEATURES_ADDED.md) - Tính năng mới
- [QUICK_FIX_GUIDE.md](QUICK_FIX_GUIDE.md) - Sửa lỗi nhanh

---

**Chúc bạn phát triển thành công! 🎊**

*Kiro AI - 2 Tháng 5, 2026*

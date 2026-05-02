# ⚡ CHẠY NGAY - WEBDULICH v2.0

## 🚀 3 BƯỚC ĐỂ CHẠY

### Bước 1: Kill process cũ (nếu có)
```bash
taskkill /F /IM WEBDULICH.exe
```

### Bước 2: Chạy ứng dụng
```bash
cd D:\Users\admoi\source\repos\DoanhNoVip\WEBDULICH
dotnet run
```

### Bước 3: Mở browser
```
http://localhost:5000
```

---

## 🎯 HOẶC CHẠY VỚI PORT KHÁC

```bash
dotnet run --urls "http://localhost:5555"
```

Sau đó truy cập: http://localhost:5555

---

## 📚 TRUY CẬP

- **Website**: http://localhost:5000
- **API Docs**: http://localhost:5000/api-docs
- **Hangfire**: http://localhost:5000/hangfire
- **Health**: http://localhost:5000/health

---

## 🔑 ĐĂNG NHẬP

**Admin**:
- Email: `admin@webdulich.local`
- Password: `Admin@123`

**User**:
- Email: `user@webdulich.local`
- Password: `User@123`

---

## 🌤️ TEST TÍNH NĂNG MỚI

### Weather API
```
http://localhost:5000/api/weather/current/Hà Nội
```

### Currency API
```
http://localhost:5000/api/currency/convert?amount=5000000&from=VND&to=USD
```

### Recommendation API
```
http://localhost:5000/api/recommendation/trending
```

---

## ❌ NẾU GẶP LỖI

### Lỗi: Port đang được sử dụng
```bash
netstat -ano | findstr :5000
taskkill /F /PID <PID>
```

### Lỗi: Cannot connect to database
```bash
net start MSSQLSERVER
```

### Lỗi: Build failed
```bash
taskkill /F /IM WEBDULICH.exe
dotnet clean
dotnet build
```

---

## 📖 TÀI LIỆU CHI TIẾT

- [README.md](README.md) - Tổng quan
- [SETUP_GUIDE.md](SETUP_GUIDE.md) - Hướng dẫn đầy đủ
- [NEW_FEATURES_ADDED.md](NEW_FEATURES_ADDED.md) - Tính năng mới
- [QUICK_FIX_GUIDE.md](QUICK_FIX_GUIDE.md) - Sửa lỗi
- [BUILD_COMPLETE_SUMMARY.md](BUILD_COMPLETE_SUMMARY.md) - Tổng kết

---

## ✅ ĐÃ HOÀN THÀNH

```
✅ Build thành công (0 errors)
✅ 3 tính năng mới (Weather, Currency, Recommendation)
✅ Database đã cập nhật
✅ 100+ API endpoints
✅ Tài liệu đầy đủ
✅ Sẵn sàng sử dụng
```

---

## 🎉 CHÚC BẠN THÀNH CÔNG!

**Kiro AI - May 2, 2026**

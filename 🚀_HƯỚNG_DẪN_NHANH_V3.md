# 🚀 HƯỚNG DẪN NHANH - WEBDULICH V3.0

## ⚡ KHỞI ĐỘNG NHANH (5 PHÚT)

### Bước 1: Apply Migration
```bash
cd WEBDULICH
dotnet ef database update
```

### Bước 2: Chạy ứng dụng
```bash
dotnet run
```

### Bước 3: Truy cập
- 🌐 Website: http://localhost:5134
- 📚 API Docs: http://localhost:5134/api-docs
- 📊 Hangfire: http://localhost:5134/hangfire

---

## 🎯 TEST 6 TÍNH NĂNG MỚI

### 1. Customer Segmentation (Phân khúc khách hàng)

#### Tạo segments tự động:
```bash
POST http://localhost:5134/api/CustomerSegment/analyze
```

#### Xem tất cả segments:
```bash
GET http://localhost:5134/api/CustomerSegment
```

#### Xem khách hàng giá trị cao:
```bash
GET http://localhost:5134/api/CustomerSegment/customers/high-value?count=20
```

#### Xem khách hàng có nguy cơ rời bỏ:
```bash
GET http://localhost:5134/api/CustomerSegment/customers/churn-risk
```

---

### 2. Availability Management (Quản lý còn chỗ)

#### Tạo availability cho tour:
```bash
POST http://localhost:5134/api/Availability
Content-Type: application/json

{
  "entityType": "Tour",
  "entityId": 1,
  "date": "2026-06-01",
  "totalSlots": 20,
  "availableSlots": 20,
  "status": "Available"
}
```

#### Xem calendar:
```bash
GET http://localhost:5134/api/Availability/Tour/1/calendar?startDate=2026-06-01&endDate=2026-06-30
```

#### Check occupancy rate:
```bash
GET http://localhost:5134/api/Availability/Tour/1/occupancy?startDate=2026-06-01&endDate=2026-06-30
```

---

### 3. Tour Package Builder (Xây dựng gói tour)

#### Build custom package:
```bash
POST http://localhost:5134/api/TourPackage/build
Content-Type: application/json

{
  "userId": 1,
  "name": "Gói Tour Miền Bắc 7 Ngày",
  "description": "Hà Nội - Sapa - Hạ Long",
  "items": [
    {
      "itemType": "Tour",
      "tourId": 1,
      "dayNumber": 1,
      "orderInDay": 1,
      "title": "Tham quan Hà Nội",
      "price": 500000
    },
    {
      "itemType": "Hotel",
      "hotelId": 1,
      "dayNumber": 1,
      "orderInDay": 2,
      "title": "Khách sạn 4 sao",
      "price": 800000
    }
  ]
}
```

#### Xem packages phổ biến:
```bash
GET http://localhost:5134/api/TourPackage/popular?count=10
```

#### Xem packages được recommend:
```bash
GET http://localhost:5134/api/TourPackage/recommended/1?count=10
```

---

### 4. Price Optimization (Tối ưu giá)

#### Tính giá tối ưu:
```bash
GET http://localhost:5134/api/PriceOptimization/optimal/Tour/1
```

#### Xem gợi ý giá:
```bash
GET http://localhost:5134/api/PriceOptimization/suggestions/Tour/1
```

#### Tạo dynamic pricing rule:
```bash
POST http://localhost:5134/api/PriceOptimization/rules
Content-Type: application/json

{
  "name": "High Season Pricing",
  "description": "Tăng giá 20% vào mùa cao điểm",
  "appliesTo": "Tour",
  "ruleType": "Season",
  "actionType": "Increase",
  "actionValue": 20,
  "isActive": true,
  "priority": 10
}
```

#### Xem demand forecast:
```bash
GET http://localhost:5134/api/PriceOptimization/forecast/Tour/1?days=30
```

---

### 5. Review Analytics (Phân tích đánh giá)

#### Phân tích review:
```bash
POST http://localhost:5134/api/ReviewAnalytics/analyze/1
```

#### Xem sentiment summary:
```bash
GET http://localhost:5134/api/ReviewAnalytics/sentiment/Tour/1
```

#### Xem top keywords:
```bash
GET http://localhost:5134/api/ReviewAnalytics/keywords/Tour/1?count=20
```

#### Xem aspect scores:
```bash
GET http://localhost:5134/api/ReviewAnalytics/aspects/Tour/1
```

#### Xem improvement suggestions:
```bash
GET http://localhost:5134/api/ReviewAnalytics/suggestions/Tour/1
```

#### Detect spam reviews:
```bash
GET http://localhost:5134/api/ReviewAnalytics/spam/Tour/1
```

---

### 6. Advanced Search (Tìm kiếm nâng cao)

#### Smart search:
```bash
GET http://localhost:5134/api/AdvancedSearch/smart?query=beach&userId=1
```

#### Autocomplete:
```bash
GET http://localhost:5134/api/AdvancedSearch/autocomplete?query=ha
```

#### Search tours với filters:
```bash
POST http://localhost:5134/api/AdvancedSearch/tours/filter
Content-Type: application/json

{
  "minPrice": 1000000,
  "maxPrice": 5000000,
  "minRating": 4.0,
  "categoryId": 1
}
```

#### Geo search:
```bash
GET http://localhost:5134/api/AdvancedSearch/tours/location?latitude=21.0285&longitude=105.8542&radiusKm=50
```

#### Similar tours:
```bash
GET http://localhost:5134/api/AdvancedSearch/tours/1/similar?count=10
```

#### Trending searches:
```bash
GET http://localhost:5134/api/AdvancedSearch/trending?count=10
```

---

## 📊 DASHBOARD EXAMPLES

### Customer Insights Dashboard
```bash
# Overall segmentation insights
GET http://localhost:5134/api/CustomerSegment/insights/overall

# Segment insights
GET http://localhost:5134/api/CustomerSegment/1/insights

# Marketing recommendations
GET http://localhost:5134/api/CustomerSegment/1/recommendations
```

### Revenue Optimization Dashboard
```bash
# Overall pricing statistics
GET http://localhost:5134/api/PriceOptimization/statistics/overall

# Price performance
GET http://localhost:5134/api/PriceOptimization/performance?days=30

# Optimization report
GET http://localhost:5134/api/PriceOptimization/report/Tour/1
```

### Review Analytics Dashboard
```bash
# Overall review statistics
GET http://localhost:5134/api/ReviewAnalytics/statistics/overall

# Analytics report
GET http://localhost:5134/api/ReviewAnalytics/report/Tour/1

# Competitor comparison
GET http://localhost:5134/api/ReviewAnalytics/competitor/Tour/1
```

### Package Performance Dashboard
```bash
# Overall package statistics
GET http://localhost:5134/api/TourPackage/statistics/overall

# Package statistics
GET http://localhost:5134/api/TourPackage/1/statistics
```

---

## 🔥 USE CASES THỰC TẾ

### Use Case 1: Tăng doanh thu với Dynamic Pricing
```bash
# 1. Phân tích demand
GET /api/PriceOptimization/demand/Tour/1

# 2. Xem giá tối ưu
GET /api/PriceOptimization/optimal/Tour/1

# 3. Apply dynamic pricing
POST /api/PriceOptimization/dynamic/Tour/1
{
  "basePrice": 2000000
}

# 4. Record price change
POST /api/PriceOptimization/history
{
  "entityType": "Tour",
  "entityId": 1,
  "oldPrice": 2000000,
  "newPrice": 2400000,
  "reason": "High demand - automatic adjustment"
}
```

### Use Case 2: Giảm Churn Rate
```bash
# 1. Identify at-risk customers
GET /api/CustomerSegment/customers/churn-risk

# 2. Get segment insights
GET /api/CustomerSegment/2/insights

# 3. Get marketing recommendations
GET /api/CustomerSegment/2/recommendations

# 4. Send personalized offers (integrate with email service)
```

### Use Case 3: Cải thiện Review Score
```bash
# 1. Get negative reviews
GET /api/ReviewAnalytics/negative/Tour/1?count=20

# 2. Analyze aspects
GET /api/ReviewAnalytics/aspects/Tour/1

# 3. Get improvement suggestions
GET /api/ReviewAnalytics/suggestions/Tour/1

# 4. Track trends
GET /api/ReviewAnalytics/trends/Tour/1?months=6
```

### Use Case 4: Tăng Conversion với Packages
```bash
# 1. Build attractive package
POST /api/TourPackage/build

# 2. Calculate optimal price
POST /api/TourPackage/1/calculate-price

# 3. Optimize package
POST /api/TourPackage/1/optimize

# 4. Track performance
GET /api/TourPackage/1/statistics
```

---

## 🎯 TIPS & TRICKS

### 1. Tối ưu Performance
- Sử dụng pagination cho large datasets
- Cache frequently accessed data
- Use indexes hiệu quả

### 2. Best Practices
- Chạy segment analysis hàng tuần
- Update review statistics daily
- Monitor price performance
- Track search analytics

### 3. Monitoring
- Check Hangfire dashboard cho background jobs
- Monitor API response times
- Track error logs trong Serilog
- Review health check endpoint

---

## 🐛 TROUBLESHOOTING

### Migration Issues
```bash
# Drop database và recreate
dotnet ef database drop
dotnet ef database update
```

### Service Registration Issues
- Check Program.cs có đăng ký đủ services không
- Verify dependency injection configuration

### API Errors
- Check Swagger UI cho error details
- Review logs trong console
- Verify request body format

---

## 📚 RESOURCES

### API Documentation
- Swagger UI: http://localhost:5134/api-docs
- Postman Collection: Import từ Swagger

### Database
- SQL Server Management Studio
- Azure Data Studio
- DBeaver

### Monitoring
- Hangfire Dashboard: http://localhost:5134/hangfire
- Health Check: http://localhost:5134/health

---

## 🎊 NEXT STEPS

1. ✅ Test tất cả 98 API endpoints
2. ✅ Tạo sample data
3. ✅ Run segment analysis
4. ✅ Configure dynamic pricing rules
5. ✅ Monitor performance
6. ✅ Deploy to production

---

**🚀 CHÚC BẠN THÀNH CÔNG VỚI WEBDULICH V3.0!**

*Cần hỗ trợ? Xem file ✅_WEBDULICH_V3_COMPLETE_FINAL_MAY_2026.md*

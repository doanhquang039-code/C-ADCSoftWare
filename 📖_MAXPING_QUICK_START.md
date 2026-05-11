# 📖 MAXPING QUICK START GUIDE

## 🚀 Hướng dẫn nhanh sử dụng MAXPING Features

---

## 1️⃣ AI RECOMMENDATION ENGINE

### 🎯 Mục đích
Gợi ý tours và hotels cá nhân hóa cho từng user dựa trên lịch sử và preferences.

### 📡 API Endpoints

#### Get Personalized Tours
```http
GET /api/recommendation/tours/personalized/{userId}?count=10
```

**Example**:
```bash
curl -X GET "https://localhost:7011/api/recommendation/tours/personalized/1?count=10"
```

**Response**:
```json
{
  "success": true,
  "userId": 1,
  "count": 10,
  "recommendations": [
    {
      "id": 5,
      "name": "Ha Long Bay 3 Days",
      "price": 5000000,
      "rating": 4.8,
      "category": "Adventure",
      "destination": "Ha Long"
    }
  ]
}
```

---

#### Get Similar Tours
```http
GET /api/recommendation/tours/similar/{tourId}?count=10
```

**Example**:
```bash
curl -X GET "https://localhost:7011/api/recommendation/tours/similar/5?count=10"
```

---

#### Get Trending Tours
```http
GET /api/recommendation/trending/Tour?days=7
```

**Example**:
```bash
curl -X GET "https://localhost:7011/api/recommendation/trending/Tour?days=7"
```

**Response**:
```json
{
  "success": true,
  "itemType": "Tour",
  "days": 7,
  "trending": [
    {
      "tourId": 5,
      "bookingCount": 25,
      "totalRevenue": 125000000
    }
  ]
}
```

---

### 💻 Code Usage

#### In Controller
```csharp
private readonly IRecommendationEngine _recommendationEngine;

public async Task<IActionResult> GetRecommendations(int userId)
{
    // Get personalized tours
    var tours = await _recommendationEngine.GetPersonalizedToursAsync(userId, 10);
    
    // Get similar tours
    var similar = await _recommendationEngine.GetSimilarToursAsync(tourId, 10);
    
    // Get user preferences
    var preferences = await _recommendationEngine.GetUserPreferencesAsync(userId);
    
    return Ok(new { tours, similar, preferences });
}
```

#### In Service
```csharp
public class BookingService
{
    private readonly IRecommendationEngine _recommendationEngine;
    
    public async Task<List<Tour>> GetRecommendationsForUser(int userId)
    {
        return await _recommendationEngine.GetPersonalizedToursAsync(userId, 10);
    }
}
```

---

### 🎨 Frontend Integration

#### JavaScript/TypeScript
```typescript
// Get personalized tours
async function getPersonalizedTours(userId: number) {
    const response = await fetch(
        `/api/recommendation/tours/personalized/${userId}?count=10`
    );
    const data = await response.json();
    return data.recommendations;
}

// Display recommendations
const tours = await getPersonalizedTours(currentUserId);
tours.forEach(tour => {
    console.log(`Recommended: ${tour.name} - ${tour.price} VND`);
});
```

#### React Component
```tsx
function RecommendedTours({ userId }) {
    const [tours, setTours] = useState([]);
    
    useEffect(() => {
        fetch(`/api/recommendation/tours/personalized/${userId}?count=10`)
            .then(res => res.json())
            .then(data => setTours(data.recommendations));
    }, [userId]);
    
    return (
        <div className="recommendations">
            <h2>Recommended for You</h2>
            {tours.map(tour => (
                <TourCard key={tour.id} tour={tour} />
            ))}
        </div>
    );
}
```

---

## 2️⃣ BLOCKCHAIN SERVICE

### 🎯 Mục đích
Bảo mật bookings và payments bằng blockchain technology với Proof of Work.

### 📡 API Endpoints

#### Create Booking Block
```http
POST /api/blockchain/blocks/booking
Authorization: Bearer {token}
Content-Type: application/json

{
  "bookingId": 123,
  "amount": 5000000,
  "details": "Tour booking for Da Nang"
}
```

**Example**:
```bash
curl -X POST "https://localhost:7011/api/blockchain/blocks/booking" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "bookingId": 123,
    "amount": 5000000,
    "details": "Tour booking for Da Nang"
  }'
```

**Response**:
```json
{
  "success": true,
  "block": {
    "index": 5,
    "timestamp": "2026-05-11T10:30:00",
    "hash": "0000a1b2c3d4e5f6789...",
    "previousHash": "0000f6e5d4c3b2a1098...",
    "nonce": 12345,
    "blockType": "BOOKING",
    "bookingId": 123,
    "amount": 5000000,
    "data": "{\"bookingId\":123,\"amount\":5000000,...}"
  },
  "message": "Booking block created successfully"
}
```

---

#### Verify Blockchain
```http
GET /api/blockchain/verify
```

**Example**:
```bash
curl -X GET "https://localhost:7011/api/blockchain/verify"
```

**Response**:
```json
{
  "success": true,
  "isValid": true,
  "message": "Blockchain integrity verified successfully",
  "timestamp": "2026-05-11T10:30:00"
}
```

---

#### Get Blockchain Stats
```http
GET /api/blockchain/stats
```

**Response**:
```json
{
  "success": true,
  "stats": {
    "totalBlocks": 25,
    "bookingBlocks": 20,
    "totalAmount": 100000000,
    "lastBlockHash": "0000a1b2c3d4e5f6...",
    "isValid": true,
    "difficulty": 4
  }
}
```

---

#### Get Blocks by Booking
```http
GET /api/blockchain/blocks/booking/{bookingId}
```

**Example**:
```bash
curl -X GET "https://localhost:7011/api/blockchain/blocks/booking/123"
```

---

### 💻 Code Usage

#### In Booking Service
```csharp
public class BookingService
{
    private readonly IBlockchainService _blockchainService;
    
    public async Task<Booking> CreateSecureBooking(BookingDto dto)
    {
        // Create booking in database
        var booking = await _dbContext.Bookings.AddAsync(new Booking
        {
            UserId = dto.UserId,
            TourId = dto.TourId,
            TotalPrice = dto.TotalPrice,
            Status = "Confirmed"
        });
        
        await _dbContext.SaveChangesAsync();
        
        // Create blockchain block for security
        var block = await _blockchainService.CreateBookingBlockAsync(
            booking.Entity.Id,
            booking.Entity.TotalPrice,
            $"Booking #{booking.Entity.Id} - User {dto.UserId}"
        );
        
        _logger.LogInformation($"Booking {booking.Entity.Id} secured in blockchain: {block.Hash}");
        
        return booking.Entity;
    }
    
    public async Task<bool> VerifyBooking(int bookingId)
    {
        // Get blockchain blocks for this booking
        var blockchain = await _blockchainService.GetBlockchainAsync();
        var bookingBlocks = blockchain.Where(b => b.BookingId == bookingId).ToList();
        
        if (!bookingBlocks.Any())
        {
            return false; // No blockchain record
        }
        
        // Verify blockchain integrity
        return await _blockchainService.VerifyBlockchainIntegrityAsync();
    }
}
```

---

### 🎨 Frontend Integration

#### JavaScript/TypeScript
```typescript
// Create secure booking
async function createSecureBooking(bookingData: any) {
    const response = await fetch('/api/blockchain/blocks/booking', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(bookingData)
    });
    
    const result = await response.json();
    
    if (result.success) {
        console.log('Booking secured in blockchain:', result.block.hash);
        return result.block;
    }
}

// Verify blockchain
async function verifyBlockchain() {
    const response = await fetch('/api/blockchain/verify');
    const result = await response.json();
    
    return result.isValid;
}

// Get blockchain stats
async function getBlockchainStats() {
    const response = await fetch('/api/blockchain/stats');
    const result = await response.json();
    
    return result.stats;
}
```

#### React Component
```tsx
function BlockchainStatus() {
    const [stats, setStats] = useState(null);
    const [isValid, setIsValid] = useState(true);
    
    useEffect(() => {
        // Get stats
        fetch('/api/blockchain/stats')
            .then(res => res.json())
            .then(data => setStats(data.stats));
        
        // Verify integrity
        fetch('/api/blockchain/verify')
            .then(res => res.json())
            .then(data => setIsValid(data.isValid));
    }, []);
    
    return (
        <div className="blockchain-status">
            <h3>Blockchain Status</h3>
            <div className={isValid ? 'valid' : 'invalid'}>
                {isValid ? '✅ Verified' : '❌ Invalid'}
            </div>
            {stats && (
                <div>
                    <p>Total Blocks: {stats.totalBlocks}</p>
                    <p>Total Amount: {stats.totalAmount.toLocaleString()} VND</p>
                    <p>Difficulty: {stats.difficulty}</p>
                </div>
            )}
        </div>
    );
}
```

---

## 🔧 CONFIGURATION

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "WEBDULICH.Services.AI": "Information",
      "WEBDULICH.Services.Blockchain": "Information"
    }
  }
}
```

---

## 🎯 USE CASES

### Use Case 1: Homepage Recommendations
```csharp
// Show personalized tours on homepage
public async Task<IActionResult> Index()
{
    var userId = GetCurrentUserId();
    
    var recommendedTours = await _recommendationEngine
        .GetPersonalizedToursAsync(userId, 6);
    
    var trendingTours = await _recommendationEngine
        .GetTrendingItemsAsync("Tour", 7);
    
    return View(new HomeViewModel
    {
        RecommendedTours = recommendedTours,
        TrendingTours = trendingTours
    });
}
```

### Use Case 2: Tour Detail Page
```csharp
// Show similar tours on tour detail page
public async Task<IActionResult> TourDetail(int id)
{
    var tour = await _tourService.GetByIdAsync(id);
    
    var similarTours = await _recommendationEngine
        .GetSimilarToursAsync(id, 4);
    
    return View(new TourDetailViewModel
    {
        Tour = tour,
        SimilarTours = similarTours
    });
}
```

### Use Case 3: Secure Booking
```csharp
// Create booking with blockchain security
public async Task<IActionResult> CreateBooking(BookingDto dto)
{
    // Create booking
    var booking = await _bookingService.CreateAsync(dto);
    
    // Secure in blockchain
    var block = await _blockchainService.CreateBookingBlockAsync(
        booking.Id,
        booking.TotalPrice,
        $"Booking #{booking.Id}"
    );
    
    return Ok(new
    {
        booking = booking,
        blockchainHash = block.Hash,
        message = "Booking created and secured in blockchain"
    });
}
```

### Use Case 4: Admin Dashboard
```csharp
// Show blockchain stats in admin dashboard
public async Task<IActionResult> Dashboard()
{
    var stats = await _blockchainService.GetBlockchainStatsAsync();
    var isValid = await _blockchainService.VerifyBlockchainIntegrityAsync();
    
    return View(new DashboardViewModel
    {
        BlockchainStats = stats,
        IsBlockchainValid = isValid
    });
}
```

---

## 🧪 TESTING

### Test Recommendation Engine
```bash
# Get personalized tours
curl -X GET "https://localhost:7011/api/recommendation/tours/personalized/1?count=10"

# Get similar tours
curl -X GET "https://localhost:7011/api/recommendation/tours/similar/5?count=10"

# Get trending
curl -X GET "https://localhost:7011/api/recommendation/trending/Tour?days=7"

# Get user preferences
curl -X GET "https://localhost:7011/api/recommendation/preferences/1"
```

### Test Blockchain
```bash
# Create booking block
curl -X POST "https://localhost:7011/api/blockchain/blocks/booking" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"bookingId":123,"amount":5000000,"details":"Test booking"}'

# Verify blockchain
curl -X GET "https://localhost:7011/api/blockchain/verify"

# Get stats
curl -X GET "https://localhost:7011/api/blockchain/stats"

# Get blockchain
curl -X GET "https://localhost:7011/api/blockchain/blocks"
```

---

## 📊 PERFORMANCE TIPS

### Recommendation Engine
- ✅ Cache recommendations for 5-10 minutes
- ✅ Use background jobs for model training
- ✅ Limit recommendation count to 10-20
- ✅ Index database tables properly

### Blockchain
- ✅ Use singleton service (already configured)
- ✅ Adjust difficulty based on load
- ✅ Consider persisting to database for production
- ✅ Implement async mining for better performance

---

## 🔒 SECURITY BEST PRACTICES

### Recommendation Engine
- ✅ Validate user IDs
- ✅ Implement rate limiting
- ✅ Protect user preferences
- ✅ Sanitize inputs

### Blockchain
- ✅ Require authentication for creating blocks
- ✅ Validate booking IDs
- ✅ Verify amounts
- ✅ Regular integrity checks
- ✅ Monitor for tampering

---

## 📚 ADDITIONAL RESOURCES

### Documentation
- Swagger UI: `https://localhost:7011/api-docs`
- Full Documentation: `🎊_MAXPING_INTEGRATION_COMPLETE_MAY_2026.md`

### Code Examples
- Controllers: `Controllers/RecommendationController.cs`, `Controllers/BlockchainController.cs`
- Services: `Services/AI/RecommendationEngine.cs`, `Services/Blockchain/BlockchainService.cs`

---

## 🎊 SUMMARY

### ✅ Quick Start Checklist

1. ✅ Services registered in `Program.cs`
2. ✅ Controllers created and working
3. ✅ API endpoints available
4. ✅ Swagger documentation ready
5. ✅ Code examples provided
6. ✅ Frontend integration examples ready

### 🚀 Ready to Use!

**Recommendation Engine**: 8 endpoints  
**Blockchain Service**: 8 endpoints  
**Total**: 16 production-ready APIs

---

**📖 QUICK START GUIDE COMPLETE!**

*Last Updated: 11/05/2026*  
*Version: 1.0*  
*Status: ✅ Production Ready*

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using WEBDULICH.Models;
using System.Text.Json;

namespace WEBDULICH.Services.Analytics
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        private readonly int _cacheDuration;

        public AnalyticsService(
            ApplicationDbContext context,
            IDistributedCache cache,
            IConfiguration configuration)
        {
            _context = context;
            _cache = cache;
            _configuration = configuration;
            _cacheDuration = configuration.GetValue<int>("Analytics:CacheDurationMinutes", 15);
        }

        public async Task<DashboardMetrics> GetDashboardMetricsAsync(DateTime from, DateTime to)
        {
            var cacheKey = $"analytics:dashboard:{from:yyyyMMdd}:{to:yyyyMMdd}";
            var cached = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<DashboardMetrics>(cached)!;
            }

            var previousFrom = from.AddDays(-(to - from).Days);
            var previousTo = from;

            // Current period bookings
            var currentBookings = await _context.Orders
                .Where(o => o.OrderDate >= from && o.OrderDate <= to)
                .ToListAsync();

            // Previous period bookings
            var previousBookings = await _context.Orders
                .Where(o => o.OrderDate >= previousFrom && o.OrderDate < previousTo)
                .ToListAsync();

            var totalRevenue = currentBookings.Sum(o => o.TotalAmount);
            var previousRevenue = previousBookings.Sum(o => o.TotalAmount);
            var revenueGrowth = previousRevenue > 0 
                ? ((totalRevenue - previousRevenue) / previousRevenue) * 100 
                : 0;

            var totalBookings = currentBookings.Count;
            var previousBookingCount = previousBookings.Count;
            var bookingGrowth = previousBookingCount > 0 
                ? ((double)(totalBookings - previousBookingCount) / previousBookingCount) * 100 
                : 0;

            var uniqueCustomers = currentBookings.Select(o => o.UserId).Distinct().Count();
            var previousCustomers = previousBookings.Select(o => o.UserId).Distinct().Count();
            var customerGrowth = previousCustomers > 0 
                ? ((double)(uniqueCustomers - previousCustomers) / previousCustomers) * 100 
                : 0;

            var averageOrderValue = totalBookings > 0 ? (double)(totalRevenue / totalBookings) : 0;

            // Conversion rate (simplified - bookings / total visitors)
            var totalVisitors = await _context.Users
                .Where(u => u.CreatedAt >= from && u.CreatedAt <= to)
                .CountAsync();
            var conversionRate = totalVisitors > 0 ? ((double)totalBookings / totalVisitors) * 100 : 0;

            var metrics = new DashboardMetrics
            {
                TotalRevenue = totalRevenue,
                RevenueGrowth = revenueGrowth,
                TotalBookings = totalBookings,
                BookingGrowth = bookingGrowth,
                TotalCustomers = uniqueCustomers,
                CustomerGrowth = customerGrowth,
                AverageOrderValue = averageOrderValue,
                ConversionRate = conversionRate,
                PendingBookings = currentBookings.Count(o => o.Status == "Pending"),
                CompletedBookings = currentBookings.Count(o => o.Status == "Completed"),
                CancelledBookings = currentBookings.Count(o => o.Status == "Cancelled")
            };

            // Cache for configured duration
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheDuration)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(metrics), cacheOptions);

            return metrics;
        }

        public async Task<List<RevenueData>> GetRevenueChartDataAsync(DateTime from, DateTime to, string groupBy = "day")
        {
            var orders = await _context.Orders
                .Where(o => o.OrderDate >= from && o.OrderDate <= to && o.Status != "Cancelled")
                .OrderBy(o => o.OrderDate)
                .ToListAsync();

            var revenueData = new List<RevenueData>();

            switch (groupBy.ToLower())
            {
                case "day":
                    revenueData = orders
                        .GroupBy(o => o.OrderDate.Date)
                        .Select(g => new RevenueData
                        {
                            Date = g.Key,
                            Revenue = g.Sum(o => o.TotalAmount),
                            Bookings = g.Count(),
                            Label = g.Key.ToString("dd/MM/yyyy")
                        })
                        .ToList();
                    break;

                case "week":
                    revenueData = orders
                        .GroupBy(o => new { Year = o.OrderDate.Year, Week = GetWeekOfYear(o.OrderDate) })
                        .Select(g => new RevenueData
                        {
                            Date = FirstDateOfWeek(g.Key.Year, g.Key.Week),
                            Revenue = g.Sum(o => o.TotalAmount),
                            Bookings = g.Count(),
                            Label = $"Week {g.Key.Week}, {g.Key.Year}"
                        })
                        .ToList();
                    break;

                case "month":
                    revenueData = orders
                        .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                        .Select(g => new RevenueData
                        {
                            Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                            Revenue = g.Sum(o => o.TotalAmount),
                            Bookings = g.Count(),
                            Label = $"{g.Key.Month:00}/{g.Key.Year}"
                        })
                        .ToList();
                    break;
            }

            return revenueData;
        }

        public async Task<List<PopularTour>> GetPopularToursAsync(int top = 10)
        {
            var popularTours = await _context.OrderDetails
                .Include(od => od.Tour)
                .GroupBy(od => new { od.TourId, od.Tour.Name, od.Tour.ImageUrl })
                .Select(g => new PopularTour
                {
                    TourId = g.Key.TourId,
                    TourName = g.Key.Name,
                    BookingCount = g.Count(),
                    TotalRevenue = g.Sum(od => od.Price * od.Quantity),
                    ImageUrl = g.Key.ImageUrl ?? ""
                })
                .OrderByDescending(t => t.BookingCount)
                .Take(top)
                .ToListAsync();

            // Get average ratings
            foreach (var tour in popularTours)
            {
                var reviews = await _context.Reviews
                    .Where(r => r.TourId == tour.TourId)
                    .ToListAsync();
                
                if (reviews.Any())
                {
                    // Parse Rating string to int and calculate average
                    var ratings = reviews
                        .Where(r => !string.IsNullOrEmpty(r.Rating) && int.TryParse(r.Rating, out _))
                        .Select(r => int.Parse(r.Rating))
                        .ToList();
                    
                    tour.AverageRating = ratings.Any() ? ratings.Average() : 0;
                }
                else
                {
                    tour.AverageRating = 0;
                }
            }

            return popularTours;
        }

        public async Task<List<CustomerSegment>> GetCustomerSegmentsAsync()
        {
            var customers = await _context.Orders
                .GroupBy(o => o.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalSpent = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .ToListAsync();

            var segments = new List<CustomerSegment>();

            // VIP Customers (>= 10 orders or >= 50M VND)
            var vipCustomers = customers.Where(c => c.OrderCount >= 10 || c.TotalSpent >= 50000000).ToList();
            segments.Add(new CustomerSegment
            {
                SegmentName = "VIP",
                CustomerCount = vipCustomers.Count,
                TotalSpent = vipCustomers.Sum(c => c.TotalSpent),
                AverageOrderValue = vipCustomers.Any() ? (double)(vipCustomers.Sum(c => c.TotalSpent) / vipCustomers.Sum(c => c.OrderCount)) : 0,
                Description = "Khách hàng thân thiết với >= 10 đơn hoặc >= 50M VND"
            });

            // Regular Customers (3-9 orders)
            var regularCustomers = customers.Where(c => c.OrderCount >= 3 && c.OrderCount < 10 && c.TotalSpent < 50000000).ToList();
            segments.Add(new CustomerSegment
            {
                SegmentName = "Regular",
                CustomerCount = regularCustomers.Count,
                TotalSpent = regularCustomers.Sum(c => c.TotalSpent),
                AverageOrderValue = regularCustomers.Any() ? (double)(regularCustomers.Sum(c => c.TotalSpent) / regularCustomers.Sum(c => c.OrderCount)) : 0,
                Description = "Khách hàng thường xuyên với 3-9 đơn"
            });

            // New Customers (1-2 orders)
            var newCustomers = customers.Where(c => c.OrderCount >= 1 && c.OrderCount < 3).ToList();
            segments.Add(new CustomerSegment
            {
                SegmentName = "New",
                CustomerCount = newCustomers.Count,
                TotalSpent = newCustomers.Sum(c => c.TotalSpent),
                AverageOrderValue = newCustomers.Any() ? (double)(newCustomers.Sum(c => c.TotalSpent) / newCustomers.Sum(c => c.OrderCount)) : 0,
                Description = "Khách hàng mới với 1-2 đơn"
            });

            return segments;
        }

        public async Task<BookingTrends> GetBookingTrendsAsync(DateTime from, DateTime to)
        {
            var bookings = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Tour)
                        .ThenInclude(t => t.Destination)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Tour)
                        .ThenInclude(t => t.Category)
                .Where(o => o.OrderDate >= from && o.OrderDate <= to)
                .ToListAsync();

            var trends = new BookingTrends
            {
                Daily = bookings
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new TrendData
                    {
                        Period = g.Key,
                        Count = g.Count(),
                        Value = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(t => t.Period)
                    .ToList(),

                Weekly = bookings
                    .GroupBy(o => new { Year = o.OrderDate.Year, Week = GetWeekOfYear(o.OrderDate) })
                    .Select(g => new TrendData
                    {
                        Period = FirstDateOfWeek(g.Key.Year, g.Key.Week),
                        Count = g.Count(),
                        Value = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(t => t.Period)
                    .ToList(),

                Monthly = bookings
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .Select(g => new TrendData
                    {
                        Period = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Count = g.Count(),
                        Value = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(t => t.Period)
                    .ToList(),

                ByDestination = bookings
                    .SelectMany(o => o.OrderDetails)
                    .Where(od => od.Tour?.Destination != null)
                    .GroupBy(od => od.Tour!.Destination!.Name)
                    .ToDictionary(g => g.Key, g => g.Count()),

                ByCategory = bookings
                    .SelectMany(o => o.OrderDetails)
                    .Where(od => od.Tour?.Category != null)
                    .GroupBy(od => od.Tour!.Category!.Name)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return trends;
        }

        public async Task<List<ConversionFunnel>> GetConversionFunnelAsync(DateTime from, DateTime to)
        {
            // Simplified funnel - in real app, track user journey
            var totalVisitors = await _context.Users
                .Where(u => u.CreatedAt >= from && u.CreatedAt <= to)
                .CountAsync();

            var tourViews = totalVisitors * 0.7; // Assume 70% view tours
            var addedToCart = totalVisitors * 0.3; // Assume 30% add to cart
            var initiatedCheckout = totalVisitors * 0.2; // Assume 20% checkout
            
            var completedBookings = await _context.Orders
                .Where(o => o.OrderDate >= from && o.OrderDate <= to && o.Status == "Completed")
                .CountAsync();

            var funnel = new List<ConversionFunnel>
            {
                new ConversionFunnel
                {
                    Stage = "Visitors",
                    Count = totalVisitors,
                    ConversionRate = 100,
                    DropOffRate = 0
                },
                new ConversionFunnel
                {
                    Stage = "Tour Views",
                    Count = (int)tourViews,
                    ConversionRate = totalVisitors > 0 ? (tourViews / totalVisitors) * 100 : 0,
                    DropOffRate = totalVisitors > 0 ? ((totalVisitors - tourViews) / totalVisitors) * 100 : 0
                },
                new ConversionFunnel
                {
                    Stage = "Added to Cart",
                    Count = (int)addedToCart,
                    ConversionRate = totalVisitors > 0 ? (addedToCart / totalVisitors) * 100 : 0,
                    DropOffRate = tourViews > 0 ? ((tourViews - addedToCart) / tourViews) * 100 : 0
                },
                new ConversionFunnel
                {
                    Stage = "Initiated Checkout",
                    Count = (int)initiatedCheckout,
                    ConversionRate = totalVisitors > 0 ? (initiatedCheckout / totalVisitors) * 100 : 0,
                    DropOffRate = addedToCart > 0 ? ((addedToCart - initiatedCheckout) / addedToCart) * 100 : 0
                },
                new ConversionFunnel
                {
                    Stage = "Completed Booking",
                    Count = completedBookings,
                    ConversionRate = totalVisitors > 0 ? ((double)completedBookings / totalVisitors) * 100 : 0,
                    DropOffRate = initiatedCheckout > 0 ? ((initiatedCheckout - completedBookings) / initiatedCheckout) * 100 : 0
                }
            };

            return funnel;
        }

        private static int GetWeekOfYear(DateTime date)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(date, 
                System.Globalization.CalendarWeekRule.FirstDay, 
                DayOfWeek.Monday);
        }

        private static DateTime FirstDateOfWeek(int year, int weekOfYear)
        {
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
            var firstMonday = jan1.AddDays(daysOffset);
            var firstWeek = GetWeekOfYear(firstMonday);
            var weekNum = weekOfYear;
            if (firstWeek == 1)
            {
                weekNum -= 1;
            }
            return firstMonday.AddDays(weekNum * 7);
        }
    }
}

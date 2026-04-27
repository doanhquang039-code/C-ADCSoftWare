using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IEmailMarketingService _emailMarketingService;
        private readonly ILoyaltyService _loyaltyService;
        private readonly ApplicationDbContext _context;

        public AnalyticsController(
            IReportService reportService,
            IEmailMarketingService emailMarketingService,
            ILoyaltyService loyaltyService,
            ApplicationDbContext context)
        {
            _reportService = reportService;
            _emailMarketingService = emailMarketingService;
            _loyaltyService = loyaltyService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var fromDate = DateTime.Now.AddDays(-30);
            var toDate = DateTime.Now;

            // Get comprehensive analytics data
            var revenueReport = await _reportService.GenerateRevenueReportAsync(fromDate, toDate);
            var bookingReport = await _reportService.GenerateBookingReportAsync(fromDate, toDate);
            var emailStats = await _emailMarketingService.GetEmailStatsAsync();
            var loyaltyStats = await _loyaltyService.GetLoyaltyStatsAsync();

            // Additional metrics
            var totalUsers = _context.Users.Count();
            var totalTours = _context.Tours.Count();
            var totalHotels = _context.Hotels.Count();
            var totalDestinations = _context.Destinations.Count();

            ViewBag.RevenueReport = revenueReport;
            ViewBag.BookingReport = bookingReport;
            ViewBag.EmailStats = emailStats;
            ViewBag.LoyaltyStats = loyaltyStats;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalTours = totalTours;
            ViewBag.TotalHotels = totalHotels;
            ViewBag.TotalDestinations = totalDestinations;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenueData(int days = 30)
        {
            var fromDate = DateTime.Now.AddDays(-days);
            var toDate = DateTime.Now;
            
            var revenueReport = await _reportService.GenerateRevenueReportAsync(fromDate, toDate);
            
            return Json(new
            {
                labels = revenueReport.DailyRevenues.Select(d => d.Date.ToString("dd/MM")).ToArray(),
                revenue = revenueReport.DailyRevenues.Select(d => d.Revenue).ToArray(),
                bookings = revenueReport.DailyRevenues.Select(d => d.BookingCount).ToArray()
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingTrends(int days = 30)
        {
            var fromDate = DateTime.Now.AddDays(-days);
            var toDate = DateTime.Now;
            
            var bookingReport = await _reportService.GenerateBookingReportAsync(fromDate, toDate);
            
            var trendData = bookingReport.BookingTrends
                .GroupBy(t => t.Date)
                .Select(g => new
                {
                    date = g.Key.ToString("dd/MM"),
                    confirmed = g.Where(t => t.Status == "Confirmed").Sum(t => t.BookingCount),
                    pending = g.Where(t => t.Status == "Pending").Sum(t => t.BookingCount),
                    cancelled = g.Where(t => t.Status == "Cancelled").Sum(t => t.BookingCount)
                })
                .OrderBy(t => t.date)
                .ToList();

            return Json(trendData);
        }

        [HttpGet]
        public async Task<IActionResult> GetTopDestinations()
        {
            var fromDate = DateTime.Now.AddDays(-30);
            var toDate = DateTime.Now;
            
            var bookingReport = await _reportService.GenerateBookingReportAsync(fromDate, toDate);
            
            return Json(bookingReport.PopularDestinations.Take(10));
        }

        [HttpGet]
        public IActionResult GetUserGrowth(int months = 12)
        {
            var startDate = DateTime.Now.AddMonths(-months);
            
            var userGrowth = _context.Users
                .Where(u => u.CreatedAt >= startDate)
                .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                .Select(g => new
                {
                    month = $"{g.Key.Month:00}/{g.Key.Year}",
                    count = g.Count()
                })
                .OrderBy(g => g.month)
                .ToList();

            return Json(userGrowth);
        }

        [HttpGet]
        public async Task<IActionResult> GetLoyaltyMetrics()
        {
            var loyaltyStats = await _loyaltyService.GetLoyaltyStatsAsync();
            
            return Json(new
            {
                tierDistribution = loyaltyStats.TierDistribution,
                popularRewards = loyaltyStats.PopularRewards.Take(5),
                totalPoints = loyaltyStats.TotalPointsIssued,
                redemptions = loyaltyStats.TotalRedemptions
            });
        }
    }
}
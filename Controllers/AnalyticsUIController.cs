using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WEBDULICH.Services;
using WEBDULICH.Models;
using WEBDULICH.Services.Analytics;

namespace WEBDULICH.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    [Route("admin/analytics")]
    public class AnalyticsUIController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IEmailMarketingService _emailMarketingService;
        private readonly ILoyaltyService _loyaltyService;
        private readonly IAnalyticsService _analyticsService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnalyticsUIController> _logger;

        public AnalyticsUIController(
            IReportService reportService,
            IEmailMarketingService emailMarketingService,
            ILoyaltyService loyaltyService,
            IAnalyticsService analyticsService,
            ApplicationDbContext context,
            ILogger<AnalyticsUIController> logger)
        {
            _reportService = reportService;
            _emailMarketingService = emailMarketingService;
            _loyaltyService = loyaltyService;
            _analyticsService = analyticsService;
            _context = context;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-30);
                var toDate = DateTime.UtcNow;

                var revenueReport = await _reportService.GenerateRevenueReportAsync(fromDate, toDate);
                var bookingReport = await _reportService.GenerateBookingReportAsync(fromDate, toDate);
                var emailStats = await _emailMarketingService.GetEmailStatsAsync();
                var loyaltyStats = await _loyaltyService.GetLoyaltyStatsAsync();
                
                // Get general metrics for counts
                var metrics = await _analyticsService.GetDashboardMetricsAsync(fromDate, toDate);

                ViewBag.RevenueReport = revenueReport;
                ViewBag.BookingReport = bookingReport;
                ViewBag.EmailStats = emailStats;
                ViewBag.LoyaltyStats = loyaltyStats;

                ViewBag.TotalUsers = metrics.TotalCustomers;
                ViewBag.TotalTours = _context.Tours.Count();
                ViewBag.TotalHotels = _context.Hotels.Count();
                ViewBag.TotalDestinations = _context.Destinations.Count();

                return View("~/Views/Analytics/Index.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Analytics Dashboard UI");
                return View("Error");
            }
        }
    }
}

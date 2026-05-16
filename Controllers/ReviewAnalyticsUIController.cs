using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WEBDULICH.Services.ReviewAnalytics;

namespace WEBDULICH.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/review-analytics")]
    public class ReviewAnalyticsUIController : Controller
    {
        private readonly IReviewAnalyticsService _analyticsService;
        private readonly ILogger<ReviewAnalyticsUIController> _logger;

        public ReviewAnalyticsUIController(
            IReviewAnalyticsService analyticsService,
            ILogger<ReviewAnalyticsUIController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var overallStats = await _analyticsService.GetOverallReviewStatisticsAsync();
                return View("~/Views/ReviewAnalytics/Index.cshtml", overallStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading review analytics index");
                return View("Error");
            }
        }
    }
}

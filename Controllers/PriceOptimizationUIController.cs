using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WEBDULICH.Services.PriceOptimization;

namespace WEBDULICH.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/price-optimization")]
    public class PriceOptimizationUIController : Controller
    {
        private readonly IPriceOptimizationService _priceService;
        private readonly ILogger<PriceOptimizationUIController> _logger;

        public PriceOptimizationUIController(
            IPriceOptimizationService priceService,
            ILogger<PriceOptimizationUIController> logger)
        {
            _priceService = priceService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var rules = await _priceService.GetAllRulesAsync(false);
                var recentChanges = await _priceService.GetRecentPriceChangesAsync(10);
                var stats = await _priceService.GetOverallPricingStatisticsAsync();

                ViewBag.RecentChanges = recentChanges;
                ViewBag.Stats = stats;

                return View("~/Views/PriceOptimization/Index.cshtml", rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading price optimization index");
                return View("Error");
            }
        }
    }
}

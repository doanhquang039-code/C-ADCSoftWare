using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WEBDULICH.Services.CustomerSegmentation;

namespace WEBDULICH.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/customer-segments")]
    public class CustomerSegmentUIController : Controller
    {
        private readonly ICustomerSegmentationService _segmentationService;
        private readonly ILogger<CustomerSegmentUIController> _logger;

        public CustomerSegmentUIController(
            ICustomerSegmentationService segmentationService,
            ILogger<CustomerSegmentUIController> logger)
        {
            _segmentationService = segmentationService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var segments = await _segmentationService.GetAllSegmentsAsync();
                return View("~/Views/CustomerSegment/Index.cshtml", segments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading segments view");
                return View("Error");
            }
        }

        [HttpGet("{id}/insights")]
        public async Task<IActionResult> Insights(int id)
        {
            try
            {
                var segment = await _segmentationService.GetSegmentByIdAsync(id);
                if (segment == null)
                {
                    return NotFound();
                }
                
                var insights = await _segmentationService.GetSegmentInsightsAsync(id);
                var recommendations = await _segmentationService.GetMarketingRecommendationsAsync(id);
                
                ViewBag.Segment = segment;
                ViewBag.Recommendations = recommendations;
                
                return View("~/Views/CustomerSegment/Insights.cshtml", insights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading segment insights view");
                return View("Error");
            }
        }
    }
}

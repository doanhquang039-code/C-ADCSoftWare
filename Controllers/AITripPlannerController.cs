using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Services.AI;

namespace WEBDULICH.Controllers
{
    public class AITripPlannerController : Controller
    {
        private readonly ITripPlannerService _tripPlannerService;
        private readonly ILogger<AITripPlannerController> _logger;

        public AITripPlannerController(ITripPlannerService tripPlannerService, ILogger<AITripPlannerController> logger)
        {
            _tripPlannerService = tripPlannerService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new TripPlanRequest
            {
                Days = 3,
                Adults = 2,
                TravelStyle = "balanced",
                StartDate = DateTime.Today.AddDays(14)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TripPlanRequest request)
        {
            var plan = await _tripPlannerService.GeneratePlanAsync(request);
            ViewBag.Plan = plan;
            return View(request);
        }

        [HttpPost]
        [Route("api/ai-trip-planner/generate")]
        public async Task<IActionResult> Generate([FromBody] TripPlanRequest request)
        {
            try
            {
                var plan = await _tripPlannerService.GeneratePlanAsync(request);
                return Ok(new { success = true, data = plan });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI trip plan");
                return StatusCode(500, new { success = false, message = "Khong the tao lich trinh AI luc nay." });
            }
        }
    }
}

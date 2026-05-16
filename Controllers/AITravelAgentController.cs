using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Services.AI;

namespace WEBDULICH.Controllers
{
    [ApiController]
    [Route("api/ai-travel-agent")]
    public class AITravelAgentController : ControllerBase
    {
        private readonly ITravelAgentService _travelAgentService;
        private readonly ILogger<AITravelAgentController> _logger;

        public AITravelAgentController(
            ITravelAgentService travelAgentService,
            ILogger<AITravelAgentController> logger)
        {
            _travelAgentService = travelAgentService;
            _logger = logger;
        }

        [HttpPost("run")]
        public async Task<IActionResult> Run([FromBody] TravelAgentRequest request)
        {
            try
            {
                var result = await _travelAgentService.RunAsync(request ?? new TravelAgentRequest());
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI Travel Agent failed");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Khong the chay AI Travel Agent luc nay."
                });
            }
        }
    }
}

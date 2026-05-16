using Microsoft.AspNetCore.Mvc;

namespace WEBDULICH.Controllers
{
    public class AITravelAgentUIController : Controller
    {
        [HttpGet("/ai-travel-agent")]
        public IActionResult Index()
        {
            return View("~/Views/AITravelAgent/Index.cshtml");
        }
    }
}

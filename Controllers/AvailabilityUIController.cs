using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WEBDULICH.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    [Route("admin/availability-calendar")]
    public class AvailabilityUIController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Views/Availability/Index.cshtml");
        }
    }
}

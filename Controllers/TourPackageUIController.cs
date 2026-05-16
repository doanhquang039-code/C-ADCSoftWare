using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WEBDULICH.Services.TourPackage;
using WEBDULICH.Models;

namespace WEBDULICH.Controllers
{
    [Authorize]
    [Route("tour-packages")]
    public class TourPackageUIController : Controller
    {
        private readonly ITourPackageService _packageService;
        private readonly ILogger<TourPackageUIController> _logger;

        public TourPackageUIController(
            ITourPackageService packageService,
            ILogger<TourPackageUIController> logger)
        {
            _packageService = packageService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var popularPackages = await _packageService.GetPopularPackagesAsync(10);
                return View("~/Views/TourPackage/Index.cshtml", popularPackages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tour packages index");
                return View("Error");
            }
        }

        [HttpGet("builder")]
        public IActionResult Builder()
        {
            return View("~/Views/TourPackage/Builder.cshtml");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var package = await _packageService.GetPackageByIdAsync(id);
                if (package == null)
                    return NotFound();

                return View("~/Views/TourPackage/Details.cshtml", package);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading package details");
                return View("Error");
            }
        }
    }
}

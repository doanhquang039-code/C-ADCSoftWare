using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Models;
using WEBDULICH.Services.TourPackage;

namespace WEBDULICH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TourPackageController : ControllerBase
    {
        private readonly ITourPackageService _packageService;
        private readonly ILogger<TourPackageController> _logger;

        public TourPackageController(
            ITourPackageService packageService,
            ILogger<TourPackageController> logger)
        {
            _packageService = packageService;
            _logger = logger;
        }

        /// <summary>
        /// Get all tour packages
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllPackages([FromQuery] string status = null, [FromQuery] bool? isPublic = null)
        {
            try
            {
                var packages = await _packageService.GetAllPackagesAsync(status, isPublic);
                return Ok(new { success = true, data = packages, count = packages.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting packages");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get package by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPackage(int id)
        {
            try
            {
                var package = await _packageService.GetPackageByIdAsync(id);
                if (package == null)
                    return NotFound(new { success = false, message = "Package not found" });

                return Ok(new { success = true, data = package });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting package");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create new package
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePackage([FromBody] Models.TourPackage package)
        {
            try
            {
                var created = await _packageService.CreatePackageAsync(package);
                return Ok(new { success = true, data = created });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating package");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update package
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePackage(int id, [FromBody] Models.TourPackage package)
        {
            try
            {
                package.Id = id;
                var updated = await _packageService.UpdatePackageAsync(package);
                return Ok(new { success = true, data = updated });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating package");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete package
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            try
            {
                var result = await _packageService.DeletePackageAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Package not found" });

                return Ok(new { success = true, message = "Package deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting package");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Add item to package
        /// </summary>
        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItem(int id, [FromBody] TourPackageItem item)
        {
            try
            {
                item.TourPackageId = id;
                var added = await _packageService.AddItemToPackageAsync(item);
                return Ok(new { success = true, data = added });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get package items
        /// </summary>
        [HttpGet("{id}/items")]
        public async Task<IActionResult> GetPackageItems(int id)
        {
            try
            {
                var items = await _packageService.GetPackageItemsAsync(id);
                return Ok(new { success = true, data = items, count = items.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting items");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Build custom package
        /// </summary>
        [HttpPost("build")]
        public async Task<IActionResult> BuildCustomPackage([FromBody] BuildPackageRequest request)
        {
            try
            {
                var package = await _packageService.BuildCustomPackageAsync(
                    request.UserId,
                    request.Items,
                    request.Name,
                    request.Description
                );
                return Ok(new { success = true, data = package });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building package");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Calculate package price
        /// </summary>
        [HttpPost("{id}/calculate-price")]
        public async Task<IActionResult> CalculatePrice(int id)
        {
            try
            {
                var price = await _packageService.CalculatePackagePriceAsync(id);
                return Ok(new { success = true, price });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating price");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Optimize package
        /// </summary>
        [HttpPost("{id}/optimize")]
        public async Task<IActionResult> OptimizePackage(int id)
        {
            try
            {
                var package = await _packageService.OptimizePackageAsync(id);
                return Ok(new { success = true, data = package });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing package");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create package booking
        /// </summary>
        [HttpPost("{id}/bookings")]
        public async Task<IActionResult> CreateBooking(int id, [FromBody] TourPackageBooking booking)
        {
            try
            {
                booking.TourPackageId = id;
                var created = await _packageService.CreateBookingAsync(booking);
                return Ok(new { success = true, data = created });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get user packages
        /// </summary>
        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUserPackages(int userId)
        {
            try
            {
                var packages = await _packageService.GetUserPackagesAsync(userId);
                return Ok(new { success = true, data = packages, count = packages.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user packages");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Clone package
        /// </summary>
        [HttpPost("{id}/clone")]
        public async Task<IActionResult> ClonePackage(int id, [FromBody] ClonePackageRequest request)
        {
            try
            {
                var cloned = await _packageService.ClonePackageAsync(id, request.UserId);
                return Ok(new { success = true, data = cloned });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning package");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get popular packages
        /// </summary>
        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularPackages([FromQuery] int count = 10)
        {
            try
            {
                var packages = await _packageService.GetPopularPackagesAsync(count);
                return Ok(new { success = true, data = packages });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular packages");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get recommended packages
        /// </summary>
        [HttpGet("recommended/{userId}")]
        public async Task<IActionResult> GetRecommendedPackages(int userId, [FromQuery] int count = 10)
        {
            try
            {
                var packages = await _packageService.GetRecommendedPackagesAsync(userId, count);
                return Ok(new { success = true, data = packages });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommended packages");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get package statistics
        /// </summary>
        [HttpGet("{id}/statistics")]
        public async Task<IActionResult> GetPackageStatistics(int id)
        {
            try
            {
                var stats = await _packageService.GetPackageStatisticsAsync(id);
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get overall statistics
        /// </summary>
        [HttpGet("statistics/overall")]
        public async Task<IActionResult> GetOverallStatistics()
        {
            try
            {
                var stats = await _packageService.GetOverallPackageStatisticsAsync();
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overall statistics");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class BuildPackageRequest
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<TourPackageItem> Items { get; set; }
    }

    public class ClonePackageRequest
    {
        public int UserId { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Models;
using WEBDULICH.Services.Availability;

namespace WEBDULICH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;
        private readonly ILogger<AvailabilityController> _logger;

        public AvailabilityController(
            IAvailabilityService availabilityService,
            ILogger<AvailabilityController> logger)
        {
            _availabilityService = availabilityService;
            _logger = logger;
        }

        /// <summary>
        /// Get availability for tour on specific date
        /// </summary>
        [HttpGet("tour/{tourId}")]
        public async Task<IActionResult> GetTourAvailability(int tourId, [FromQuery] DateTime date)
        {
            try
            {
                var availability = await _availabilityService.GetAvailabilityAsync("Tour", tourId, date);
                return Ok(new { success = true, data = availability });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour availability");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get availability for hotel on specific date
        /// </summary>
        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetHotelAvailability(int hotelId, [FromQuery] DateTime date)
        {
            try
            {
                var availability = await _availabilityService.GetAvailabilityAsync("Hotel", hotelId, date);
                return Ok(new { success = true, data = availability });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel availability");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Check availability for date range
        /// </summary>
        [HttpPost("check")]
        public async Task<IActionResult> CheckAvailability([FromBody] AvailabilityCheckRequest request)
        {
            try
            {
                var availabilities = await _availabilityService.GetAvailabilityRangeAsync(
                    request.EntityType,
                    request.EntityId,
                    request.StartDate,
                    request.EndDate
                );

                var isAvailable = availabilities.All(a => a.AvailableCapacity >= request.Quantity);

                return Ok(new
                {
                    success = true,
                    isAvailable,
                    availabilities,
                    totalCapacity = availabilities.Sum(a => a.AvailableCapacity)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create temporary block
        /// </summary>
        [HttpPost("block")]
        public async Task<IActionResult> CreateBlock([FromBody] AvailabilityBlock block)
        {
            try
            {
                var created = await _availabilityService.CreateBlockAsync(block);
                return Ok(new { success = true, data = created });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating block");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Release block
        /// </summary>
        [HttpDelete("block/{blockId}")]
        public async Task<IActionResult> ReleaseBlock(int blockId)
        {
            try
            {
                var result = await _availabilityService.ReleaseBlockAsync(blockId);
                if (!result)
                    return NotFound(new { success = false, message = "Block not found or already released" });

                return Ok(new { success = true, message = "Block released" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing block");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get calendar view
        /// </summary>
        [HttpGet("calendar")]
        public async Task<IActionResult> GetCalendar(
            [FromQuery] string entityType,
            [FromQuery] int entityId,
            [FromQuery] int year,
            [FromQuery] int month)
        {
            try
            {
                var calendar = await _availabilityService.GetCalendarAsync(entityType, entityId, year, month);
                return Ok(new { success = true, data = calendar });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting calendar");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get available dates
        /// </summary>
        [HttpGet("dates/available")]
        public async Task<IActionResult> GetAvailableDates(
            [FromQuery] string entityType,
            [FromQuery] int entityId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var dates = await _availabilityService.GetAvailableDatesAsync(entityType, entityId, startDate, endDate);
                return Ok(new { success = true, data = dates, count = dates.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available dates");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get occupancy statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetOccupancyStats(
            [FromQuery] string entityType,
            [FromQuery] int entityId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var stats = await _availabilityService.GetOccupancyStatsAsync(entityType, entityId, startDate, endDate);
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting occupancy stats");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Forecast demand
        /// </summary>
        [HttpGet("forecast")]
        public async Task<IActionResult> ForecastDemand(
            [FromQuery] string entityType,
            [FromQuery] int entityId,
            [FromQuery] int days = 30)
        {
            try
            {
                var forecast = await _availabilityService.ForecastDemandAsync(entityType, entityId, days);
                return Ok(new { success = true, data = forecast });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forecasting demand");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get high demand dates
        /// </summary>
        [HttpGet("dates/high-demand")]
        public async Task<IActionResult> GetHighDemandDates(
            [FromQuery] string entityType,
            [FromQuery] int entityId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var dates = await _availabilityService.GetHighDemandDatesAsync(entityType, entityId, startDate, endDate);
                return Ok(new { success = true, data = dates, count = dates.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting high demand dates");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class AvailabilityCheckRequest
    {
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Quantity { get; set; }
    }
}

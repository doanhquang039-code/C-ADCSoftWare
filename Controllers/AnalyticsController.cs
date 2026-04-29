using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WEBDULICH.Services.Analytics;

namespace WEBDULICH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardMetrics([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
                var toDate = to ?? DateTime.UtcNow;

                var metrics = await _analyticsService.GetDashboardMetricsAsync(fromDate, toDate);
                return Ok(new { success = true, data = metrics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard metrics");
                return StatusCode(500, new { success = false, message = "Failed to retrieve metrics" });
            }
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueChart(
            [FromQuery] DateTime? from, 
            [FromQuery] DateTime? to,
            [FromQuery] string groupBy = "day")
        {
            try
            {
                var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
                var toDate = to ?? DateTime.UtcNow;

                if (!new[] { "day", "week", "month" }.Contains(groupBy.ToLower()))
                {
                    return BadRequest(new { success = false, message = "Invalid groupBy parameter. Use: day, week, or month" });
                }

                var data = await _analyticsService.GetRevenueChartDataAsync(fromDate, toDate, groupBy);
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving revenue chart data");
                return StatusCode(500, new { success = false, message = "Failed to retrieve revenue data" });
            }
        }

        [HttpGet("popular-tours")]
        public async Task<IActionResult> GetPopularTours([FromQuery] int top = 10)
        {
            try
            {
                if (top < 1 || top > 100)
                {
                    return BadRequest(new { success = false, message = "Top parameter must be between 1 and 100" });
                }

                var tours = await _analyticsService.GetPopularToursAsync(top);
                return Ok(new { success = true, data = tours });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving popular tours");
                return StatusCode(500, new { success = false, message = "Failed to retrieve popular tours" });
            }
        }

        [HttpGet("customer-segments")]
        public async Task<IActionResult> GetCustomerSegments()
        {
            try
            {
                var segments = await _analyticsService.GetCustomerSegmentsAsync();
                return Ok(new { success = true, data = segments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer segments");
                return StatusCode(500, new { success = false, message = "Failed to retrieve customer segments" });
            }
        }

        [HttpGet("booking-trends")]
        public async Task<IActionResult> GetBookingTrends([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
                var toDate = to ?? DateTime.UtcNow;

                var trends = await _analyticsService.GetBookingTrendsAsync(fromDate, toDate);
                return Ok(new { success = true, data = trends });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving booking trends");
                return StatusCode(500, new { success = false, message = "Failed to retrieve booking trends" });
            }
        }

        [HttpGet("conversion-funnel")]
        public async Task<IActionResult> GetConversionFunnel([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
                var toDate = to ?? DateTime.UtcNow;

                var funnel = await _analyticsService.GetConversionFunnelAsync(fromDate, toDate);
                return Ok(new { success = true, data = funnel });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversion funnel");
                return StatusCode(500, new { success = false, message = "Failed to retrieve conversion funnel" });
            }
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                // TODO: Implement Excel export
                return Ok(new { success = false, message = "Excel export not implemented yet" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to Excel");
                return StatusCode(500, new { success = false, message = "Failed to export data" });
            }
        }

        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportToPdf([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                // TODO: Implement PDF export
                return Ok(new { success = false, message = "PDF export not implemented yet" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to PDF");
                return StatusCode(500, new { success = false, message = "Failed to export data" });
            }
        }
    }
}

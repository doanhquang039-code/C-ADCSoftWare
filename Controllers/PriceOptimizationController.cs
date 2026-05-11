using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Models;
using WEBDULICH.Services.PriceOptimization;

namespace WEBDULICH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PriceOptimizationController : ControllerBase
    {
        private readonly IPriceOptimizationService _priceService;
        private readonly ILogger<PriceOptimizationController> _logger;

        public PriceOptimizationController(
            IPriceOptimizationService priceService,
            ILogger<PriceOptimizationController> logger)
        {
            _priceService = priceService;
            _logger = logger;
        }

        /// <summary>
        /// Get price history
        /// </summary>
        [HttpGet("history/{entityType}/{entityId}")]
        public async Task<IActionResult> GetPriceHistory(string entityType, int entityId)
        {
            try
            {
                var history = await _priceService.GetPriceHistoryAsync(entityType, entityId);
                return Ok(new { success = true, data = history, count = history.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price history");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get recent price changes
        /// </summary>
        [HttpGet("history/recent")]
        public async Task<IActionResult> GetRecentPriceChanges([FromQuery] int days = 30)
        {
            try
            {
                var changes = await _priceService.GetRecentPriceChangesAsync(days);
                return Ok(new { success = true, data = changes, count = changes.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent changes");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Record price change
        /// </summary>
        [HttpPost("history")]
        public async Task<IActionResult> RecordPriceChange([FromBody] RecordPriceChangeRequest request)
        {
            try
            {
                var history = await _priceService.RecordPriceChangeAsync(
                    request.EntityType,
                    request.EntityId,
                    request.OldPrice,
                    request.NewPrice,
                    request.Reason
                );
                return Ok(new { success = true, data = history });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording price change");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get all pricing rules
        /// </summary>
        [HttpGet("rules")]
        public async Task<IActionResult> GetAllRules([FromQuery] bool activeOnly = true)
        {
            try
            {
                var rules = await _priceService.GetAllRulesAsync(activeOnly);
                return Ok(new { success = true, data = rules, count = rules.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rules");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get rule by ID
        /// </summary>
        [HttpGet("rules/{id}")]
        public async Task<IActionResult> GetRule(int id)
        {
            try
            {
                var rule = await _priceService.GetRuleByIdAsync(id);
                if (rule == null)
                    return NotFound(new { success = false, message = "Rule not found" });

                return Ok(new { success = true, data = rule });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rule");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create pricing rule
        /// </summary>
        [HttpPost("rules")]
        public async Task<IActionResult> CreateRule([FromBody] DynamicPricingRule rule)
        {
            try
            {
                var created = await _priceService.CreateRuleAsync(rule);
                return Ok(new { success = true, data = created });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rule");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update pricing rule
        /// </summary>
        [HttpPut("rules/{id}")]
        public async Task<IActionResult> UpdateRule(int id, [FromBody] DynamicPricingRule rule)
        {
            try
            {
                rule.Id = id;
                var updated = await _priceService.UpdateRuleAsync(rule);
                return Ok(new { success = true, data = updated });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rule");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete pricing rule
        /// </summary>
        [HttpDelete("rules/{id}")]
        public async Task<IActionResult> DeleteRule(int id)
        {
            try
            {
                var result = await _priceService.DeleteRuleAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Rule not found" });

                return Ok(new { success = true, message = "Rule deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rule");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Calculate optimal price
        /// </summary>
        [HttpGet("optimal/{entityType}/{entityId}")]
        public async Task<IActionResult> CalculateOptimalPrice(string entityType, int entityId)
        {
            try
            {
                var price = await _priceService.CalculateOptimalPriceAsync(entityType, entityId);
                return Ok(new { success = true, optimalPrice = price });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating optimal price");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Apply dynamic pricing
        /// </summary>
        [HttpPost("dynamic/{entityType}/{entityId}")]
        public async Task<IActionResult> ApplyDynamicPricing(string entityType, int entityId, [FromBody] ApplyPricingRequest request)
        {
            try
            {
                var price = await _priceService.ApplyDynamicPricingAsync(entityType, entityId, request.BasePrice);
                return Ok(new { success = true, dynamicPrice = price });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying dynamic pricing");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get price suggestions
        /// </summary>
        [HttpGet("suggestions/{entityType}/{entityId}")]
        public async Task<IActionResult> GetPriceSuggestions(string entityType, int entityId)
        {
            try
            {
                var suggestions = await _priceService.GetPriceSuggestionsAsync(entityType, entityId);
                return Ok(new { success = true, data = suggestions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suggestions");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Analyze demand level
        /// </summary>
        [HttpGet("demand/{entityType}/{entityId}")]
        public async Task<IActionResult> AnalyzeDemand(string entityType, int entityId)
        {
            try
            {
                var demandLevel = await _priceService.AnalyzeDemandLevelAsync(entityType, entityId);
                return Ok(new { success = true, demandLevel });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing demand");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get demand forecast
        /// </summary>
        [HttpGet("forecast/{entityType}/{entityId}")]
        public async Task<IActionResult> GetDemandForecast(string entityType, int entityId, [FromQuery] int days = 30)
        {
            try
            {
                var forecast = await _priceService.GetDemandForecastAsync(entityType, entityId, days);
                return Ok(new { success = true, data = forecast });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting forecast");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get seasonal trends
        /// </summary>
        [HttpGet("trends/seasonal/{entityType}/{entityId}")]
        public async Task<IActionResult> GetSeasonalTrends(string entityType, int entityId)
        {
            try
            {
                var trends = await _priceService.GetSeasonalTrendsAsync(entityType, entityId);
                return Ok(new { success = true, data = trends });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seasonal trends");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get pricing trends
        /// </summary>
        [HttpGet("trends/pricing/{entityType}/{entityId}")]
        public async Task<IActionResult> GetPricingTrends(string entityType, int entityId, [FromQuery] int months = 6)
        {
            try
            {
                var trends = await _priceService.GetPricingTrendsAsync(entityType, entityId, months);
                return Ok(new { success = true, data = trends });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pricing trends");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get competitor pricing
        /// </summary>
        [HttpGet("competitor/{entityType}/{entityId}")]
        public async Task<IActionResult> GetCompetitorPricing(string entityType, int entityId)
        {
            try
            {
                var comparison = await _priceService.GetCompetitorPricingAsync(entityType, entityId);
                return Ok(new { success = true, data = comparison });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting competitor pricing");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get optimization report
        /// </summary>
        [HttpGet("report/{entityType}/{entityId}")]
        public async Task<IActionResult> GetOptimizationReport(string entityType, int entityId)
        {
            try
            {
                var report = await _priceService.GetPriceOptimizationReportAsync(entityType, entityId);
                return Ok(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report");
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
                var stats = await _priceService.GetOverallPricingStatisticsAsync();
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get price performance
        /// </summary>
        [HttpGet("performance")]
        public async Task<IActionResult> GetPricePerformance([FromQuery] int days = 30)
        {
            try
            {
                var performance = await _priceService.GetPricePerformanceAsync(days);
                return Ok(new { success = true, data = performance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class RecordPriceChangeRequest
    {
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string Reason { get; set; }
    }

    public class ApplyPricingRequest
    {
        public decimal BasePrice { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Models;
using WEBDULICH.Services.CustomerSegmentation;

namespace WEBDULICH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerSegmentController : ControllerBase
    {
        private readonly ICustomerSegmentationService _segmentationService;
        private readonly ILogger<CustomerSegmentController> _logger;

        public CustomerSegmentController(
            ICustomerSegmentationService segmentationService,
            ILogger<CustomerSegmentController> logger)
        {
            _segmentationService = segmentationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all customer segments
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllSegments()
        {
            try
            {
                var segments = await _segmentationService.GetAllSegmentsAsync();
                return Ok(new { success = true, data = segments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting segments");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get segment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSegment(int id)
        {
            try
            {
                var segment = await _segmentationService.GetSegmentByIdAsync(id);
                if (segment == null)
                    return NotFound(new { success = false, message = "Segment not found" });

                return Ok(new { success = true, data = segment });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting segment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create new segment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSegment([FromBody] CustomerSegment segment)
        {
            try
            {
                var created = await _segmentationService.CreateSegmentAsync(segment);
                return Ok(new { success = true, data = created });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating segment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update segment
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSegment(int id, [FromBody] CustomerSegment segment)
        {
            try
            {
                segment.Id = id;
                var updated = await _segmentationService.UpdateSegmentAsync(segment);
                return Ok(new { success = true, data = updated });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating segment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete segment
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSegment(int id)
        {
            try
            {
                var result = await _segmentationService.DeleteSegmentAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Segment not found" });

                return Ok(new { success = true, message = "Segment deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting segment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Analyze and create segments automatically
        /// </summary>
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeSegments()
        {
            try
            {
                var segments = await _segmentationService.AnalyzeAndCreateSegmentsAsync();
                return Ok(new { success = true, data = segments, count = segments.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing segments");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get segment members
        /// </summary>
        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetSegmentMembers(int id)
        {
            try
            {
                var members = await _segmentationService.GetSegmentMembersAsync(id);
                return Ok(new { success = true, data = members, count = members.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting segment members");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get segment insights
        /// </summary>
        [HttpGet("{id}/insights")]
        public async Task<IActionResult> GetSegmentInsights(int id)
        {
            try
            {
                var insights = await _segmentationService.GetSegmentInsightsAsync(id);
                if (insights == null)
                    return NotFound(new { success = false, message = "Segment not found" });

                return Ok(new { success = true, data = insights });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting segment insights");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get overall segmentation insights
        /// </summary>
        [HttpGet("insights/overall")]
        public async Task<IActionResult> GetOverallInsights()
        {
            try
            {
                var insights = await _segmentationService.GetOverallSegmentationInsightsAsync();
                return Ok(new { success = true, data = insights });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overall insights");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get customer behavior
        /// </summary>
        [HttpGet("customers/{userId}/behavior")]
        public async Task<IActionResult> GetCustomerBehavior(int userId)
        {
            try
            {
                var behavior = await _segmentationService.GetCustomerBehaviorAsync(userId);
                if (behavior == null)
                    return NotFound(new { success = false, message = "Customer behavior not found" });

                return Ok(new { success = true, data = behavior });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer behavior");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get high value customers
        /// </summary>
        [HttpGet("customers/high-value")]
        public async Task<IActionResult> GetHighValueCustomers([FromQuery] int count = 100)
        {
            try
            {
                var customers = await _segmentationService.GetHighValueCustomersAsync(count);
                return Ok(new { success = true, data = customers, count = customers.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting high value customers");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get churn risk customers
        /// </summary>
        [HttpGet("customers/churn-risk")]
        public async Task<IActionResult> GetChurnRiskCustomers([FromQuery] decimal minRiskScore = 0.7m)
        {
            try
            {
                var customers = await _segmentationService.GetChurnRiskCustomersAsync(minRiskScore);
                return Ok(new { success = true, data = customers, count = customers.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting churn risk customers");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get marketing recommendations for segment
        /// </summary>
        [HttpGet("{id}/recommendations")]
        public async Task<IActionResult> GetMarketingRecommendations(int id)
        {
            try
            {
                var recommendations = await _segmentationService.GetMarketingRecommendationsAsync(id);
                return Ok(new { success = true, data = recommendations });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting marketing recommendations");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}

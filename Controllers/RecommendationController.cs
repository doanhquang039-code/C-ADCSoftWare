using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Services.Recommendation;
using WEBDULICH.Helpers;

namespace WEBDULICH.Controllers
{
    /// <summary>
    /// Recommendation Controller
    /// Provides personalized tour recommendations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;
        private readonly ILogger<RecommendationController> _logger;

        public RecommendationController(
            IRecommendationService recommendationService,
            ILogger<RecommendationController> logger)
        {
            _recommendationService = recommendationService;
            _logger = logger;
        }

        /// <summary>
        /// Get personalized recommendations for current user
        /// </summary>
        [HttpGet("personalized")]
        [AuthenticatedOnly]
        public async Task<IActionResult> GetPersonalizedRecommendations([FromQuery] int count = 10)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var recommendations = await _recommendationService
                    .GetPersonalizedRecommendationsAsync(userId.Value, count);

                return Ok(new
                {
                    success = true,
                    data = recommendations,
                    message = "Gợi ý tour dành riêng cho bạn"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting personalized recommendations");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy gợi ý" });
            }
        }

        /// <summary>
        /// Get similar tours
        /// </summary>
        [HttpGet("similar/{tourId}")]
        public async Task<IActionResult> GetSimilarTours(int tourId, [FromQuery] int count = 5)
        {
            try
            {
                var similarTours = await _recommendationService.GetSimilarToursAsync(tourId, count);

                return Ok(new
                {
                    success = true,
                    data = similarTours,
                    message = "Tour tương tự"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting similar tours");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy tour tương tự" });
            }
        }

        /// <summary>
        /// Get trending tours
        /// </summary>
        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingTours([FromQuery] int count = 10)
        {
            try
            {
                var trendingTours = await _recommendationService.GetTrendingToursAsync(count);

                return Ok(new
                {
                    success = true,
                    data = trendingTours,
                    message = "Tour đang thịnh hành"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending tours");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy tour thịnh hành" });
            }
        }

        /// <summary>
        /// Get recommendations by preferences
        /// </summary>
        [HttpPost("by-preferences")]
        public async Task<IActionResult> GetRecommendationsByPreferences(
            [FromBody] UserPreferences preferences,
            [FromQuery] int count = 10)
        {
            try
            {
                var recommendations = await _recommendationService
                    .GetRecommendationsByPreferencesAsync(preferences, count);

                return Ok(new
                {
                    success = true,
                    data = recommendations,
                    message = "Tour phù hợp với sở thích của bạn"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations by preferences");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy gợi ý" });
            }
        }

        /// <summary>
        /// Get collaborative recommendations (customers who viewed this also viewed)
        /// </summary>
        [HttpGet("collaborative/{tourId}")]
        public async Task<IActionResult> GetCollaborativeRecommendations(
            int tourId, 
            [FromQuery] int count = 5)
        {
            try
            {
                var recommendations = await _recommendationService
                    .GetCollaborativeRecommendationsAsync(tourId, count);

                return Ok(new
                {
                    success = true,
                    data = recommendations,
                    message = "Khách hàng cũng xem các tour này"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collaborative recommendations");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy gợi ý" });
            }
        }

        /// <summary>
        /// Get seasonal recommendations
        /// </summary>
        [HttpGet("seasonal")]
        public async Task<IActionResult> GetSeasonalRecommendations(
            [FromQuery] int? month = null,
            [FromQuery] int count = 10)
        {
            try
            {
                var targetMonth = month ?? DateTime.Now.Month;
                var recommendations = await _recommendationService
                    .GetSeasonalRecommendationsAsync(targetMonth, count);

                return Ok(new
                {
                    success = true,
                    data = recommendations,
                    message = $"Tour phù hợp cho tháng {targetMonth}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seasonal recommendations");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy gợi ý theo mùa" });
            }
        }

        /// <summary>
        /// Track user behavior for recommendations
        /// </summary>
        [HttpPost("track")]
        [AuthenticatedOnly]
        public async Task<IActionResult> TrackUserBehavior(
            [FromBody] TrackingRequest request)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
                }

                await _recommendationService.UpdateUserPreferencesAsync(
                    userId.Value, 
                    request.TourId, 
                    request.Action);

                return Ok(new
                {
                    success = true,
                    message = "Đã cập nhật sở thích"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking user behavior");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật" });
            }
        }
    }

    /// <summary>
    /// Tracking Request Model
    /// </summary>
    public class TrackingRequest
    {
        public int TourId { get; set; }
        public string Action { get; set; } = string.Empty; // view, wishlist, order, review
    }
}

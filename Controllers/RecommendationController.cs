using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WEBDULICH.Services.AI;
using WEBDULICH.Models;

namespace WEBDULICH.Controllers
{
    /// <summary>
    /// AI-Powered Recommendation Controller
    /// API endpoints cho hệ thống gợi ý thông minh
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationEngine _recommendationEngine;
        private readonly ILogger<RecommendationController> _logger;

        public RecommendationController(
            IRecommendationEngine recommendationEngine,
            ILogger<RecommendationController> logger)
        {
            _recommendationEngine = recommendationEngine;
            _logger = logger;
        }

        /// <summary>
        /// Get personalized tour recommendations for user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="count">Number of recommendations (default: 10)</param>
        /// <returns>List of recommended tours</returns>
        [HttpGet("tours/personalized/{userId}")]
        [ProducesResponseType(typeof(List<Tour>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetPersonalizedTours(int userId, [FromQuery] int count = 10)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "Invalid user ID" });
                }

                if (count <= 0 || count > 50)
                {
                    return BadRequest(new { message = "Count must be between 1 and 50" });
                }

                _logger.LogInformation($"Getting personalized tours for user {userId}");

                var tours = await _recommendationEngine.GetPersonalizedToursAsync(userId, count);

                return Ok(new
                {
                    success = true,
                    userId = userId,
                    count = tours.Count,
                    recommendations = tours,
                    message = $"Found {tours.Count} personalized tour recommendations"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting personalized tours for user {userId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get personalized hotel recommendations for user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="count">Number of recommendations (default: 10)</param>
        /// <returns>List of recommended hotels</returns>
        [HttpGet("hotels/personalized/{userId}")]
        [ProducesResponseType(typeof(List<Hotel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetPersonalizedHotels(int userId, [FromQuery] int count = 10)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "Invalid user ID" });
                }

                if (count <= 0 || count > 50)
                {
                    return BadRequest(new { message = "Count must be between 1 and 50" });
                }

                _logger.LogInformation($"Getting personalized hotels for user {userId}");

                var hotels = await _recommendationEngine.GetPersonalizedHotelsAsync(userId, count);

                return Ok(new
                {
                    success = true,
                    userId = userId,
                    count = hotels.Count,
                    recommendations = hotels,
                    message = $"Found {hotels.Count} personalized hotel recommendations"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting personalized hotels for user {userId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get similar tours based on a specific tour
        /// </summary>
        /// <param name="tourId">Tour ID</param>
        /// <param name="count">Number of similar tours (default: 10)</param>
        /// <returns>List of similar tours</returns>
        [HttpGet("tours/similar/{tourId}")]
        [ProducesResponseType(typeof(List<Tour>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetSimilarTours(int tourId, [FromQuery] int count = 10)
        {
            try
            {
                if (tourId <= 0)
                {
                    return BadRequest(new { message = "Invalid tour ID" });
                }

                if (count <= 0 || count > 50)
                {
                    return BadRequest(new { message = "Count must be between 1 and 50" });
                }

                _logger.LogInformation($"Getting similar tours for tour {tourId}");

                var tours = await _recommendationEngine.GetSimilarToursAsync(tourId, count);

                return Ok(new
                {
                    success = true,
                    tourId = tourId,
                    count = tours.Count,
                    similarTours = tours,
                    message = $"Found {tours.Count} similar tours"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting similar tours for tour {tourId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user preferences and behavior analysis
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User preferences dictionary</returns>
        [HttpGet("preferences/{userId}")]
        [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUserPreferences(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "Invalid user ID" });
                }

                _logger.LogInformation($"Getting preferences for user {userId}");

                var preferences = await _recommendationEngine.GetUserPreferencesAsync(userId);

                return Ok(new
                {
                    success = true,
                    userId = userId,
                    preferences = preferences,
                    message = "User preferences retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting preferences for user {userId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Update user preferences manually
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="preferences">Preferences dictionary</param>
        /// <returns>Success response</returns>
        [HttpPut("preferences/{userId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateUserPreferences(int userId, [FromBody] Dictionary<string, object> preferences)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "Invalid user ID" });
                }

                if (preferences == null || preferences.Count == 0)
                {
                    return BadRequest(new { message = "Preferences cannot be empty" });
                }

                _logger.LogInformation($"Updating preferences for user {userId}");

                await _recommendationEngine.UpdateUserPreferencesAsync(userId, preferences);

                return Ok(new
                {
                    success = true,
                    userId = userId,
                    message = "User preferences updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating preferences for user {userId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get trending items (tours, hotels, etc.)
        /// </summary>
        /// <param name="itemType">Type of item (Tour, Hotel)</param>
        /// <param name="days">Number of days to analyze (default: 7)</param>
        /// <returns>List of trending items</returns>
        [HttpGet("trending/{itemType}")]
        [ProducesResponseType(typeof(List<Dictionary<string, object>>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTrendingItems(string itemType, [FromQuery] int days = 7)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(itemType))
                {
                    return BadRequest(new { message = "Item type is required" });
                }

                if (days <= 0 || days > 90)
                {
                    return BadRequest(new { message = "Days must be between 1 and 90" });
                }

                _logger.LogInformation($"Getting trending {itemType} for last {days} days");

                var trending = await _recommendationEngine.GetTrendingItemsAsync(itemType, days);

                return Ok(new
                {
                    success = true,
                    itemType = itemType,
                    days = days,
                    count = trending.Count,
                    trending = trending,
                    message = $"Found {trending.Count} trending items"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting trending {itemType}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Train recommendation model (Admin only)
        /// </summary>
        /// <returns>Success response</returns>
        [HttpPost("train")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> TrainModel()
        {
            try
            {
                _logger.LogInformation("Starting recommendation model training");

                await _recommendationEngine.TrainRecommendationModelAsync();

                return Ok(new
                {
                    success = true,
                    message = "Recommendation model trained successfully",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error training recommendation model");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get recommendation engine statistics
        /// </summary>
        /// <returns>Statistics object</returns>
        [HttpGet("stats")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                _logger.LogInformation("Getting recommendation engine statistics");

                // In production, implement actual statistics gathering
                var stats = new
                {
                    success = true,
                    totalRecommendations = 0,
                    activeUsers = 0,
                    averageAccuracy = 0.0,
                    lastTrainingDate = DateTime.Now.AddDays(-7),
                    algorithms = new[] { "Collaborative Filtering", "Content-Based Filtering", "Hybrid" },
                    message = "Recommendation engine statistics"
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendation statistics");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}

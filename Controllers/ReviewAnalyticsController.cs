using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Services.ReviewAnalytics;

namespace WEBDULICH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewAnalyticsController : ControllerBase
    {
        private readonly IReviewAnalyticsService _analyticsService;
        private readonly ILogger<ReviewAnalyticsController> _logger;

        public ReviewAnalyticsController(
            IReviewAnalyticsService analyticsService,
            ILogger<ReviewAnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Analyze a review
        /// </summary>
        [HttpPost("analyze/{reviewId}")]
        public async Task<IActionResult> AnalyzeReview(int reviewId)
        {
            try
            {
                var analytics = await _analyticsService.AnalyzeReviewAsync(reviewId);
                if (analytics == null)
                    return NotFound(new { success = false, message = "Review not found" });

                return Ok(new { success = true, data = analytics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing review");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get review analytics
        /// </summary>
        [HttpGet("{reviewId}")]
        public async Task<IActionResult> GetReviewAnalytics(int reviewId)
        {
            try
            {
                var analytics = await _analyticsService.GetReviewAnalyticsAsync(reviewId);
                if (analytics == null)
                    return NotFound(new { success = false, message = "Analytics not found" });

                return Ok(new { success = true, data = analytics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get all review analytics for entity
        /// </summary>
        [HttpGet("{entityType}/{entityId}")]
        public async Task<IActionResult> GetAllReviewAnalytics(string entityType, int entityId)
        {
            try
            {
                var analytics = await _analyticsService.GetAllReviewAnalyticsAsync(entityType, entityId);
                return Ok(new { success = true, data = analytics, count = analytics.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all analytics");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get sentiment summary
        /// </summary>
        [HttpGet("sentiment/{entityType}/{entityId}")]
        public async Task<IActionResult> GetSentimentSummary(string entityType, int entityId)
        {
            try
            {
                var summary = await _analyticsService.GetSentimentSummaryAsync(entityType, entityId);
                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sentiment summary");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get positive reviews
        /// </summary>
        [HttpGet("positive/{entityType}/{entityId}")]
        public async Task<IActionResult> GetPositiveReviews(string entityType, int entityId, [FromQuery] int count = 10)
        {
            try
            {
                var reviews = await _analyticsService.GetPositiveReviewsAsync(entityType, entityId, count);
                return Ok(new { success = true, data = reviews, count = reviews.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting positive reviews");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get negative reviews
        /// </summary>
        [HttpGet("negative/{entityType}/{entityId}")]
        public async Task<IActionResult> GetNegativeReviews(string entityType, int entityId, [FromQuery] int count = 10)
        {
            try
            {
                var reviews = await _analyticsService.GetNegativeReviewsAsync(entityType, entityId, count);
                return Ok(new { success = true, data = reviews, count = reviews.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting negative reviews");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get review statistics
        /// </summary>
        [HttpGet("statistics/{entityType}/{entityId}")]
        public async Task<IActionResult> GetReviewStatistics(string entityType, int entityId)
        {
            try
            {
                var stats = await _analyticsService.GetReviewStatisticsAsync(entityType, entityId);
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update review statistics
        /// </summary>
        [HttpPost("statistics/{entityType}/{entityId}/update")]
        public async Task<IActionResult> UpdateReviewStatistics(string entityType, int entityId)
        {
            try
            {
                await _analyticsService.UpdateReviewStatisticsAsync(entityType, entityId);
                return Ok(new { success = true, message = "Statistics updated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating statistics");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get rating distribution
        /// </summary>
        [HttpGet("distribution/{entityType}/{entityId}")]
        public async Task<IActionResult> GetRatingDistribution(string entityType, int entityId)
        {
            try
            {
                var distribution = await _analyticsService.GetRatingDistributionAsync(entityType, entityId);
                return Ok(new { success = true, data = distribution });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting distribution");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get top keywords
        /// </summary>
        [HttpGet("keywords/{entityType}/{entityId}")]
        public async Task<IActionResult> GetTopKeywords(string entityType, int entityId, [FromQuery] int count = 20)
        {
            try
            {
                var keywords = await _analyticsService.GetTopKeywordsAsync(entityType, entityId, count);
                return Ok(new { success = true, data = keywords });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting keywords");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get top topics
        /// </summary>
        [HttpGet("topics/{entityType}/{entityId}")]
        public async Task<IActionResult> GetTopTopics(string entityType, int entityId, [FromQuery] int count = 10)
        {
            try
            {
                var topics = await _analyticsService.GetTopTopicsAsync(entityType, entityId, count);
                return Ok(new { success = true, data = topics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting topics");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get aspect scores
        /// </summary>
        [HttpGet("aspects/{entityType}/{entityId}")]
        public async Task<IActionResult> GetAspectScores(string entityType, int entityId)
        {
            try
            {
                var aspects = await _analyticsService.GetAspectScoresAsync(entityType, entityId);
                return Ok(new { success = true, data = aspects });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting aspects");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Detect spam reviews
        /// </summary>
        [HttpGet("spam/{entityType}/{entityId}")]
        public async Task<IActionResult> DetectSpamReviews(string entityType, int entityId)
        {
            try
            {
                var spam = await _analyticsService.DetectSpamReviewsAsync(entityType, entityId);
                return Ok(new { success = true, data = spam, count = spam.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting spam");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Detect fake reviews
        /// </summary>
        [HttpGet("fake/{entityType}/{entityId}")]
        public async Task<IActionResult> DetectFakeReviews(string entityType, int entityId)
        {
            try
            {
                var fake = await _analyticsService.DetectFakeReviewsAsync(entityType, entityId);
                return Ok(new { success = true, data = fake, count = fake.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting fake reviews");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Mark review as spam
        /// </summary>
        [HttpPost("{reviewId}/spam")]
        public async Task<IActionResult> MarkAsSpam(int reviewId)
        {
            try
            {
                var result = await _analyticsService.MarkReviewAsSpamAsync(reviewId);
                if (!result)
                    return NotFound(new { success = false, message = "Review not found" });

                return Ok(new { success = true, message = "Marked as spam" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking as spam");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Mark review as fake
        /// </summary>
        [HttpPost("{reviewId}/fake")]
        public async Task<IActionResult> MarkAsFake(int reviewId)
        {
            try
            {
                var result = await _analyticsService.MarkReviewAsFakeAsync(reviewId);
                if (!result)
                    return NotFound(new { success = false, message = "Review not found" });

                return Ok(new { success = true, message = "Marked as fake" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking as fake");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get review trends
        /// </summary>
        [HttpGet("trends/{entityType}/{entityId}")]
        public async Task<IActionResult> GetReviewTrends(string entityType, int entityId, [FromQuery] int months = 6)
        {
            try
            {
                var trends = await _analyticsService.GetReviewTrendsAsync(entityType, entityId, months);
                return Ok(new { success = true, data = trends });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trends");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get competitor comparison
        /// </summary>
        [HttpGet("competitor/{entityType}/{entityId}")]
        public async Task<IActionResult> GetCompetitorComparison(string entityType, int entityId)
        {
            try
            {
                var comparison = await _analyticsService.GetCompetitorComparisonAsync(entityType, entityId);
                return Ok(new { success = true, data = comparison });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comparison");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get improvement suggestions
        /// </summary>
        [HttpGet("suggestions/{entityType}/{entityId}")]
        public async Task<IActionResult> GetImprovementSuggestions(string entityType, int entityId)
        {
            try
            {
                var suggestions = await _analyticsService.GetImprovementSuggestionsAsync(entityType, entityId);
                return Ok(new { success = true, data = suggestions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suggestions");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get most helpful reviews
        /// </summary>
        [HttpGet("helpful/{entityType}/{entityId}")]
        public async Task<IActionResult> GetMostHelpfulReviews(string entityType, int entityId, [FromQuery] int count = 10)
        {
            try
            {
                var reviews = await _analyticsService.GetMostHelpfulReviewsAsync(entityType, entityId, count);
                return Ok(new { success = true, data = reviews });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting helpful reviews");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get analytics report
        /// </summary>
        [HttpGet("report/{entityType}/{entityId}")]
        public async Task<IActionResult> GetAnalyticsReport(string entityType, int entityId)
        {
            try
            {
                var report = await _analyticsService.GetReviewAnalyticsReportAsync(entityType, entityId);
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
                var stats = await _analyticsService.GetOverallReviewStatisticsAsync();
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overall statistics");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}

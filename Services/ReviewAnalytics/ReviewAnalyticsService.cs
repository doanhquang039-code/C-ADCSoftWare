using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WEBDULICH.Services.ReviewAnalytics
{
    public class ReviewAnalyticsService : IReviewAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewAnalyticsService> _logger;

        // Simple sentiment keywords (in real app, use ML model)
        private readonly List<string> _positiveWords = new() { "tuyệt", "tốt", "đẹp", "great", "excellent", "amazing", "wonderful", "perfect", "love", "best" };
        private readonly List<string> _negativeWords = new() { "tệ", "xấu", "kém", "bad", "terrible", "awful", "worst", "hate", "poor", "disappointing" };

        public ReviewAnalyticsService(
            ApplicationDbContext context,
            ILogger<ReviewAnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ReviewAnalytics> AnalyzeReviewAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null) return null;

            var existing = await _context.ReviewAnalytics
                .FirstOrDefaultAsync(a => a.ReviewId == reviewId);

            if (existing != null)
            {
                _context.ReviewAnalytics.Remove(existing);
                await _context.SaveChangesAsync();
            }

            var analytics = new ReviewAnalytics
            {
                ReviewId = reviewId,
                AnalyzedAt = DateTime.Now
            };

            // Sentiment analysis
            var sentiment = AnalyzeSentiment(review.Comment);
            analytics.SentimentScore = sentiment.Score;
            analytics.SentimentLabel = sentiment.Label;
            analytics.Confidence = sentiment.Confidence;

            // Extract keywords
            var keywords = ExtractKeywords(review.Comment);
            analytics.Keywords = JsonSerializer.Serialize(keywords);

            // Detect topics
            var topics = DetectTopics(review.Comment);
            analytics.Topics = JsonSerializer.Serialize(topics);

            // Analyze aspects
            var aspects = AnalyzeAspects(review.Comment);
            analytics.Aspects = JsonSerializer.Serialize(aspects);

            // Detect emotions
            var emotions = DetectEmotions(review.Comment);
            analytics.Emotions = JsonSerializer.Serialize(emotions);

            // Language detection
            analytics.Language = DetectLanguage(review.Comment);

            // Spam detection
            var spamResult = DetectSpam(review.Comment);
            analytics.IsSpam = spamResult.IsSpam;
            analytics.SpamConfidence = spamResult.Confidence;

            // Fake detection
            var fakeResult = DetectFake(review);
            analytics.IsFake = fakeResult.IsFake;
            analytics.FakeConfidence = fakeResult.Confidence;

            // Helpfulness score
            analytics.HelpfulnessScore = CalculateHelpfulness(review);

            _context.ReviewAnalytics.Add(analytics);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Analyzed review {reviewId}");
            return analytics;
        }

        public async Task<ReviewAnalytics> GetReviewAnalyticsAsync(int reviewId)
        {
            return await _context.ReviewAnalytics
                .Include(a => a.Review)
                .FirstOrDefaultAsync(a => a.ReviewId == reviewId);
        }

        public async Task<List<ReviewAnalytics>> GetAllReviewAnalyticsAsync(string entityType, int entityId)
        {
            var reviews = await GetEntityReviewsAsync(entityType, entityId);
            var reviewIds = reviews.Select(r => r.Id).ToList();

            return await _context.ReviewAnalytics
                .Where(a => reviewIds.Contains(a.ReviewId))
                .Include(a => a.Review)
                .ToListAsync();
        }

        public async Task<Dictionary<string, object>> GetSentimentSummaryAsync(string entityType, int entityId)
        {
            var analytics = await GetAllReviewAnalyticsAsync(entityType, entityId);

            if (!analytics.Any())
                return new Dictionary<string, object>
                {
                    ["totalReviews"] = 0,
                    ["averageSentiment"] = 0,
                    ["positive"] = 0,
                    ["neutral"] = 0,
                    ["negative"] = 0
                };

            return new Dictionary<string, object>
            {
                ["totalReviews"] = analytics.Count,
                ["averageSentiment"] = Math.Round(analytics.Average(a => a.SentimentScore), 2),
                ["positive"] = analytics.Count(a => a.SentimentLabel == "Positive"),
                ["neutral"] = analytics.Count(a => a.SentimentLabel == "Neutral"),
                ["negative"] = analytics.Count(a => a.SentimentLabel == "Negative"),
                ["positivePercentage"] = Math.Round((decimal)analytics.Count(a => a.SentimentLabel == "Positive") / analytics.Count * 100, 2),
                ["negativePercentage"] = Math.Round((decimal)analytics.Count(a => a.SentimentLabel == "Negative") / analytics.Count * 100, 2)
            };
        }

        public async Task<List<Review>> GetPositiveReviewsAsync(string entityType, int entityId, int count = 10)
        {
            var analytics = await GetAllReviewAnalyticsAsync(entityType, entityId);
            var positiveIds = analytics
                .Where(a => a.SentimentLabel == "Positive")
                .OrderByDescending(a => a.SentimentScore)
                .Take(count)
                .Select(a => a.ReviewId)
                .ToList();

            return await _context.Reviews
                .Where(r => positiveIds.Contains(r.Id))
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<List<Review>> GetNegativeReviewsAsync(string entityType, int entityId, int count = 10)
        {
            var analytics = await GetAllReviewAnalyticsAsync(entityType, entityId);
            var negativeIds = analytics
                .Where(a => a.SentimentLabel == "Negative")
                .OrderBy(a => a.SentimentScore)
                .Take(count)
                .Select(a => a.ReviewId)
                .ToList();

            return await _context.Reviews
                .Where(r => negativeIds.Contains(r.Id))
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<ReviewStatistics> GetReviewStatisticsAsync(string entityType, int entityId)
        {
            var stats = await _context.ReviewStatistics
                .FirstOrDefaultAsync(s => s.EntityType == entityType && 
                    (entityType == "Tour" ? s.TourId == entityId : s.HotelId == entityId));

            if (stats == null)
            {
                await UpdateReviewStatisticsAsync(entityType, entityId);
                stats = await _context.ReviewStatistics
                    .FirstOrDefaultAsync(s => s.EntityType == entityType && 
                        (entityType == "Tour" ? s.TourId == entityId : s.HotelId == entityId));
            }

            return stats;
        }

        public async Task UpdateReviewStatisticsAsync(string entityType, int entityId)
        {
            var reviews = await GetEntityReviewsAsync(entityType, entityId);
            var analytics = await GetAllReviewAnalyticsAsync(entityType, entityId);

            var stats = await _context.ReviewStatistics
                .FirstOrDefaultAsync(s => s.EntityType == entityType && 
                    (entityType == "Tour" ? s.TourId == entityId : s.HotelId == entityId));

            if (stats == null)
            {
                stats = new ReviewStatistics { EntityType = entityType };
                if (entityType == "Tour") stats.TourId = entityId;
                else if (entityType == "Hotel") stats.HotelId = entityId;
                _context.ReviewStatistics.Add(stats);
            }

            stats.TotalReviews = reviews.Count;
            stats.AverageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0;

            stats.FiveStarCount = reviews.Count(r => r.Rating == 5);
            stats.FourStarCount = reviews.Count(r => r.Rating == 4);
            stats.ThreeStarCount = reviews.Count(r => r.Rating == 3);
            stats.TwoStarCount = reviews.Count(r => r.Rating == 2);
            stats.OneStarCount = reviews.Count(r => r.Rating == 1);

            if (analytics.Any())
            {
                stats.AverageSentiment = analytics.Average(a => a.SentimentScore);
                stats.PositivePercentage = (decimal)analytics.Count(a => a.SentimentLabel == "Positive") / analytics.Count * 100;
                stats.NeutralPercentage = (decimal)analytics.Count(a => a.SentimentLabel == "Neutral") / analytics.Count * 100;
                stats.NegativePercentage = (decimal)analytics.Count(a => a.SentimentLabel == "Negative") / analytics.Count * 100;

                // Top keywords
                var allKeywords = new List<string>();
                foreach (var a in analytics)
                {
                    try
                    {
                        var keywords = JsonSerializer.Deserialize<List<string>>(a.Keywords ?? "[]");
                        allKeywords.AddRange(keywords);
                    }
                    catch { }
                }
                var topKeywords = allKeywords.GroupBy(k => k).OrderByDescending(g => g.Count()).Take(20).Select(g => g.Key).ToList();
                stats.TopKeywords = JsonSerializer.Serialize(topKeywords);

                // Aspect scores
                var aspectScores = await GetAspectScoresAsync(entityType, entityId);
                stats.AspectScores = JsonSerializer.Serialize(aspectScores);
            }

            stats.RecommendationRate = reviews.Any() ? (decimal)reviews.Count(r => r.Rating >= 4) / reviews.Count * 100 : 0;
            stats.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<string, object>> GetRatingDistributionAsync(string entityType, int entityId)
        {
            var stats = await GetReviewStatisticsAsync(entityType, entityId);
            if (stats == null) return new Dictionary<string, object>();

            return new Dictionary<string, object>
            {
                ["5star"] = stats.FiveStarCount,
                ["4star"] = stats.FourStarCount,
                ["3star"] = stats.ThreeStarCount,
                ["2star"] = stats.TwoStarCount,
                ["1star"] = stats.OneStarCount,
                ["total"] = stats.TotalReviews,
                ["average"] = stats.AverageRating
            };
        }

        public async Task<List<string>> GetTopKeywordsAsync(string entityType, int entityId, int count = 20)
        {
            var analytics = await GetAllReviewAnalyticsAsync(entityType, entityId);
            var allKeywords = new List<string>();

            foreach (var a in analytics)
            {
                try
                {
                    var keywords = JsonSerializer.Deserialize<List<string>>(a.Keywords ?? "[]");
                    allKeywords.AddRange(keywords);
                }
                catch { }
            }

            return allKeywords
                .GroupBy(k => k)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToList();
        }

        public async Task<List<string>> GetTopTopicsAsync(string entityType, int entityId, int count = 10)
        {
            var analytics = await GetAllReviewAnalyticsAsync(entityType, entityId);
            var allTopics = new List<string>();

            foreach (var a in analytics)
            {
                try
                {
                    var topics = JsonSerializer.Deserialize<List<string>>(a.Topics ?? "[]");
                    allTopics.AddRange(topics);
                }
                catch { }
            }

            return allTopics
                .GroupBy(t => t)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToList();
        }

        public async Task<Dictionary<string, decimal>> GetAspectScoresAsync(string entityType, int entityId)
        {
            var analytics = await GetAllReviewAnalyticsAsync(entityType, entityId);
            var aspectScores = new Dictionary<string, List<decimal>>();

            foreach (var a in analytics)
            {
                try
                {
                    var aspects = JsonSerializer.Deserialize<Dictionary<string, decimal>>(a.Aspects ?? "{}");
                    foreach (var aspect in aspects)
                    {
                        if (!aspectScores.ContainsKey(aspect.Key))
                            aspectScores[aspect.Key] = new List<decimal>();
                        aspectScores[aspect.Key].Add(aspect.Value);
                    }
                }
                catch { }
            }

            return aspectScores.ToDictionary(
                kvp => kvp.Key,
                kvp => Math.Round(kvp.Value.Average(), 2)
            );
        }

        public async Task<List<Review>> DetectSpamReviewsAsync(string entityType, int entityId)
        {
            var analytics = await GetAllReviewAnalyticsAsync(entityType, entityId);
            var spamIds = analytics
                .Where(a => a.IsSpam)
                .Select(a => a.ReviewId)
                .ToList();

            return await _context.Reviews
                .Where(r => spamIds.Contains(r.Id))
                .ToListAsync();
        }

        public async Task<List<Review>> DetectFakeReviewsAsync(string entityType, int entityId)
        {
            var analytics = await GetAllReviewAnalyticsAsync(entityType, entityId);
            var fakeIds = analytics
                .Where(a => a.IsFake)
                .Select(a => a.ReviewId)
                .ToList();

            return await _context.Reviews
                .Where(r => fakeIds.Contains(r.Id))
                .ToListAsync();
        }

        public async Task<bool> MarkReviewAsSpamAsync(int reviewId)
        {
            var analytics = await GetReviewAnalyticsAsync(reviewId);
            if (analytics == null) return false;

            analytics.IsSpam = true;
            analytics.SpamConfidence = 1.0m;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkReviewAsFakeAsync(int reviewId)
        {
            var analytics = await GetReviewAnalyticsAsync(reviewId);
            if (analytics == null) return false;

            analytics.IsFake = true;
            analytics.FakeConfidence = 1.0m;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Dictionary<string, object>> GetReviewTrendsAsync(string entityType, int entityId, int months = 6)
        {
            var cutoffDate = DateTime.Now.AddMonths(-months);
            var reviews = await GetEntityReviewsAsync(entityType, entityId);
            reviews = reviews.Where(r => r.CreatedAt >= cutoffDate).ToList();

            var monthlyTrends = reviews
                .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count(),
                    AverageRating = g.Average(r => r.Rating)
                })
                .OrderBy(x => x.Month)
                .ToList();

            return new Dictionary<string, object>
            {
                ["trends"] = monthlyTrends,
                ["totalReviews"] = reviews.Count,
                ["averageRating"] = reviews.Any() ? reviews.Average(r => r.Rating) : 0
            };
        }

        public async Task<Dictionary<string, object>> GetCompetitorComparisonAsync(string entityType, int entityId)
        {
            var stats = await GetReviewStatisticsAsync(entityType, entityId);
            var allStats = await _context.ReviewStatistics
                .Where(s => s.EntityType == entityType)
                .ToListAsync();

            var avgRating = allStats.Any() ? allStats.Average(s => s.AverageRating) : 0;
            var avgSentiment = allStats.Any() ? allStats.Average(s => s.AverageSentiment) : 0;

            return new Dictionary<string, object>
            {
                ["yourRating"] = stats?.AverageRating ?? 0,
                ["marketAverage"] = Math.Round(avgRating, 2),
                ["yourSentiment"] = stats?.AverageSentiment ?? 0,
                ["marketSentiment"] = Math.Round(avgSentiment, 2),
                ["position"] = stats != null && stats.AverageRating > avgRating ? "Above Average" : "Below Average"
            };
        }

        public async Task<List<Dictionary<string, object>>> GetImprovementSuggestionsAsync(string entityType, int entityId)
        {
            var negativeReviews = await GetNegativeReviewsAsync(entityType, entityId, 50);
            var aspectScores = await GetAspectScoresAsync(entityType, entityId);

            var suggestions = new List<Dictionary<string, object>>();

            // Analyze low-scoring aspects
            foreach (var aspect in aspectScores.Where(a => a.Value < 3.0m).OrderBy(a => a.Value))
            {
                suggestions.Add(new Dictionary<string, object>
                {
                    ["area"] = aspect.Key,
                    ["currentScore"] = aspect.Value,
                    ["priority"] = "High",
                    ["suggestion"] = $"Improve {aspect.Key} - current score is {aspect.Value:F1}/5.0"
                });
            }

            return suggestions;
        }

        public async Task<List<Review>> GetMostHelpfulReviewsAsync(string entityType, int entityId, int count = 10)
        {
            var analytics = await GetAllReviewAnalyticsAsync(entityType, entityId);
            var helpfulIds = analytics
                .OrderByDescending(a => a.HelpfulnessScore)
                .Take(count)
                .Select(a => a.ReviewId)
                .ToList();

            return await _context.Reviews
                .Where(r => helpfulIds.Contains(r.Id))
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task UpdateHelpfulnessScoreAsync(int reviewId)
        {
            var analytics = await GetReviewAnalyticsAsync(reviewId);
            if (analytics == null) return;

            var review = await _context.Reviews.FindAsync(reviewId);
            analytics.HelpfulnessScore = CalculateHelpfulness(review);
            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<string, object>> GetReviewAnalyticsReportAsync(string entityType, int entityId)
        {
            var stats = await GetReviewStatisticsAsync(entityType, entityId);
            var sentiment = await GetSentimentSummaryAsync(entityType, entityId);
            var keywords = await GetTopKeywordsAsync(entityType, entityId, 10);
            var aspects = await GetAspectScoresAsync(entityType, entityId);
            var suggestions = await GetImprovementSuggestionsAsync(entityType, entityId);

            return new Dictionary<string, object>
            {
                ["statistics"] = stats,
                ["sentiment"] = sentiment,
                ["topKeywords"] = keywords,
                ["aspectScores"] = aspects,
                ["improvementSuggestions"] = suggestions
            };
        }

        public async Task<Dictionary<string, object>> GetOverallReviewStatisticsAsync()
        {
            var allStats = await _context.ReviewStatistics.ToListAsync();
            var allAnalytics = await _context.ReviewAnalytics.ToListAsync();

            return new Dictionary<string, object>
            {
                ["totalReviews"] = allAnalytics.Count,
                ["averageRating"] = allStats.Any() ? Math.Round(allStats.Average(s => s.AverageRating), 2) : 0,
                ["averageSentiment"] = allAnalytics.Any() ? Math.Round(allAnalytics.Average(a => a.SentimentScore), 2) : 0,
                ["spamDetected"] = allAnalytics.Count(a => a.IsSpam),
                ["fakeDetected"] = allAnalytics.Count(a => a.IsFake)
            };
        }

        // Helper methods
        private async Task<List<Review>> GetEntityReviewsAsync(string entityType, int entityId)
        {
            if (entityType == "Tour")
                return await _context.Reviews.Where(r => r.TourId == entityId).ToListAsync();
            else if (entityType == "Hotel")
                return await _context.Reviews.Where(r => r.HotelId == entityId).ToListAsync();
            return new List<Review>();
        }

        private (decimal Score, string Label, decimal Confidence) AnalyzeSentiment(string text)
        {
            if (string.IsNullOrEmpty(text))
                return (0, "Neutral", 0.5m);

            text = text.ToLower();
            int positiveCount = _positiveWords.Count(w => text.Contains(w));
            int negativeCount = _negativeWords.Count(w => text.Contains(w));

            decimal score = (positiveCount - negativeCount) / 10.0m;
            score = Math.Max(-1, Math.Min(1, score));

            string label = score > 0.2m ? "Positive" : score < -0.2m ? "Negative" : "Neutral";
            decimal confidence = Math.Abs(score);

            return (score, label, confidence);
        }

        private List<string> ExtractKeywords(string text)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();

            var words = Regex.Split(text.ToLower(), @"\W+")
                .Where(w => w.Length > 3)
                .GroupBy(w => w)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();

            return words;
        }

        private List<string> DetectTopics(string text)
        {
            var topics = new List<string>();
            text = text.ToLower();

            if (text.Contains("service") || text.Contains("dịch vụ")) topics.Add("Service");
            if (text.Contains("food") || text.Contains("đồ ăn")) topics.Add("Food");
            if (text.Contains("location") || text.Contains("vị trí")) topics.Add("Location");
            if (text.Contains("price") || text.Contains("giá")) topics.Add("Price");
            if (text.Contains("clean") || text.Contains("sạch sẽ")) topics.Add("Cleanliness");
            if (text.Contains("staff") || text.Contains("nhân viên")) topics.Add("Staff");

            return topics;
        }

        private Dictionary<string, decimal> AnalyzeAspects(string text)
        {
            var aspects = new Dictionary<string, decimal>();
            text = text.ToLower();

            // Simple aspect scoring
            aspects["service"] = text.Contains("good service") || text.Contains("dịch vụ tốt") ? 4.5m : 3.0m;
            aspects["food"] = text.Contains("delicious") || text.Contains("ngon") ? 4.5m : 3.0m;
            aspects["location"] = text.Contains("convenient") || text.Contains("thuận tiện") ? 4.5m : 3.0m;
            aspects["price"] = text.Contains("reasonable") || text.Contains("hợp lý") ? 4.5m : 3.0m;

            return aspects;
        }

        private List<string> DetectEmotions(string text)
        {
            var emotions = new List<string>();
            text = text.ToLower();

            if (text.Contains("love") || text.Contains("yêu")) emotions.Add("Love");
            if (text.Contains("happy") || text.Contains("vui")) emotions.Add("Happy");
            if (text.Contains("angry") || text.Contains("tức")) emotions.Add("Angry");
            if (text.Contains("sad") || text.Contains("buồn")) emotions.Add("Sad");
            if (text.Contains("excited") || text.Contains("phấn khích")) emotions.Add("Excited");

            return emotions;
        }

        private string DetectLanguage(string text)
        {
            if (string.IsNullOrEmpty(text)) return "unknown";
            
            // Simple detection
            if (Regex.IsMatch(text, @"[\u0080-\u024F]")) return "vi"; // Vietnamese
            return "en"; // English
        }

        private (bool IsSpam, decimal Confidence) DetectSpam(string text)
        {
            if (string.IsNullOrEmpty(text)) return (false, 0);

            // Simple spam detection
            bool isSpam = text.Length < 10 || 
                         Regex.Matches(text, @"http").Count > 2 ||
                         text.ToUpper() == text;

            return (isSpam, isSpam ? 0.8m : 0.2m);
        }

        private (bool IsFake, decimal Confidence) DetectFake(Review review)
        {
            // Simple fake detection
            bool isFake = review.Comment.Length < 20 || 
                         (review.Rating == 5 && review.Comment.Length < 30);

            return (isFake, isFake ? 0.6m : 0.3m);
        }

        private decimal CalculateHelpfulness(Review review)
        {
            decimal score = 0;

            // Length bonus
            if (review.Comment.Length > 100) score += 30;
            else if (review.Comment.Length > 50) score += 20;
            else if (review.Comment.Length > 20) score += 10;

            // Rating consistency
            if (review.Rating >= 3 && review.Rating <= 4) score += 20;

            // Recency
            var daysSince = (DateTime.Now - review.CreatedAt).Days;
            if (daysSince < 30) score += 20;
            else if (daysSince < 90) score += 10;

            return Math.Min(score, 100);
        }
    }
}

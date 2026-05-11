using WEBDULICH.Models;

namespace WEBDULICH.Services.ReviewAnalytics
{
    public interface IReviewAnalyticsService
    {
        // Review Analysis
        Task<ReviewAnalytics> AnalyzeReviewAsync(int reviewId);
        Task<ReviewAnalytics> GetReviewAnalyticsAsync(int reviewId);
        Task<List<ReviewAnalytics>> GetAllReviewAnalyticsAsync(string entityType, int entityId);

        // Sentiment Analysis
        Task<Dictionary<string, object>> GetSentimentSummaryAsync(string entityType, int entityId);
        Task<List<Review>> GetPositiveReviewsAsync(string entityType, int entityId, int count = 10);
        Task<List<Review>> GetNegativeReviewsAsync(string entityType, int entityId, int count = 10);

        // Review Statistics
        Task<ReviewStatistics> GetReviewStatisticsAsync(string entityType, int entityId);
        Task UpdateReviewStatisticsAsync(string entityType, int entityId);
        Task<Dictionary<string, object>> GetRatingDistributionAsync(string entityType, int entityId);

        // Keywords & Topics
        Task<List<string>> GetTopKeywordsAsync(string entityType, int entityId, int count = 20);
        Task<List<string>> GetTopTopicsAsync(string entityType, int entityId, int count = 10);
        Task<Dictionary<string, decimal>> GetAspectScoresAsync(string entityType, int entityId);

        // Spam & Fake Detection
        Task<List<Review>> DetectSpamReviewsAsync(string entityType, int entityId);
        Task<List<Review>> DetectFakeReviewsAsync(string entityType, int entityId);
        Task<bool> MarkReviewAsSpamAsync(int reviewId);
        Task<bool> MarkReviewAsFakeAsync(int reviewId);

        // Trends & Insights
        Task<Dictionary<string, object>> GetReviewTrendsAsync(string entityType, int entityId, int months = 6);
        Task<Dictionary<string, object>> GetCompetitorComparisonAsync(string entityType, int entityId);
        Task<List<Dictionary<string, object>>> GetImprovementSuggestionsAsync(string entityType, int entityId);

        // Helpfulness
        Task<List<Review>> GetMostHelpfulReviewsAsync(string entityType, int entityId, int count = 10);
        Task UpdateHelpfulnessScoreAsync(int reviewId);

        // Reports
        Task<Dictionary<string, object>> GetReviewAnalyticsReportAsync(string entityType, int entityId);
        Task<Dictionary<string, object>> GetOverallReviewStatisticsAsync();
    }
}

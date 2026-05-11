using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    /// <summary>
    /// Review Analytics - Phân tích sentiment và insights từ reviews
    /// </summary>
    public class ReviewAnalytics
    {
        public int Id { get; set; }

        public int ReviewId { get; set; }
        public Review Review { get; set; }

        /// <summary>
        /// Sentiment score (-1 to 1): negative, neutral, positive
        /// </summary>
        public decimal SentimentScore { get; set; }

        /// <summary>
        /// "Positive", "Neutral", "Negative"
        /// </summary>
        public string SentimentLabel { get; set; }

        /// <summary>
        /// Confidence score (0-1)
        /// </summary>
        public decimal Confidence { get; set; }

        /// <summary>
        /// Keywords extracted (JSON array)
        /// </summary>
        public string Keywords { get; set; }

        /// <summary>
        /// Topics detected (JSON array)
        /// </summary>
        public string Topics { get; set; }

        /// <summary>
        /// Aspects mentioned (service, food, location, price, etc.) - JSON
        /// </summary>
        public string Aspects { get; set; }

        /// <summary>
        /// Emotions detected (happy, sad, angry, etc.) - JSON
        /// </summary>
        public string Emotions { get; set; }

        /// <summary>
        /// Language detected
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Có phải spam không
        /// </summary>
        public bool IsSpam { get; set; }

        /// <summary>
        /// Spam confidence (0-1)
        /// </summary>
        public decimal SpamConfidence { get; set; }

        /// <summary>
        /// Có phải fake review không
        /// </summary>
        public bool IsFake { get; set; }

        /// <summary>
        /// Fake confidence (0-1)
        /// </summary>
        public decimal FakeConfidence { get; set; }

        /// <summary>
        /// Helpfulness score (based on likes, reports)
        /// </summary>
        public decimal HelpfulnessScore { get; set; }

        public DateTime AnalyzedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Aggregated review statistics for tours/hotels
    /// </summary>
    public class ReviewStatistics
    {
        public int Id { get; set; }

        /// <summary>
        /// "Tour" or "Hotel"
        /// </summary>
        [Required]
        public string EntityType { get; set; }

        public int? TourId { get; set; }
        public Tour Tour { get; set; }

        public int? HotelId { get; set; }
        public Hotel Hotel { get; set; }

        /// <summary>
        /// Tổng số reviews
        /// </summary>
        public int TotalReviews { get; set; }

        /// <summary>
        /// Rating trung bình
        /// </summary>
        public decimal AverageRating { get; set; }

        /// <summary>
        /// Số reviews 5 sao
        /// </summary>
        public int FiveStarCount { get; set; }

        /// <summary>
        /// Số reviews 4 sao
        /// </summary>
        public int FourStarCount { get; set; }

        /// <summary>
        /// Số reviews 3 sao
        /// </summary>
        public int ThreeStarCount { get; set; }

        /// <summary>
        /// Số reviews 2 sao
        /// </summary>
        public int TwoStarCount { get; set; }

        /// <summary>
        /// Số reviews 1 sao
        /// </summary>
        public int OneStarCount { get; set; }

        /// <summary>
        /// Sentiment score trung bình
        /// </summary>
        public decimal AverageSentiment { get; set; }

        /// <summary>
        /// % positive reviews
        /// </summary>
        public decimal PositivePercentage { get; set; }

        /// <summary>
        /// % neutral reviews
        /// </summary>
        public decimal NeutralPercentage { get; set; }

        /// <summary>
        /// % negative reviews
        /// </summary>
        public decimal NegativePercentage { get; set; }

        /// <summary>
        /// Top keywords (JSON array)
        /// </summary>
        public string TopKeywords { get; set; }

        /// <summary>
        /// Top topics (JSON array)
        /// </summary>
        public string TopTopics { get; set; }

        /// <summary>
        /// Aspect scores (JSON object)
        /// </summary>
        public string AspectScores { get; set; }

        /// <summary>
        /// Recommendation rate (%)
        /// </summary>
        public decimal RecommendationRate { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}

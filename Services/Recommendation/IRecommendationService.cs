using WEBDULICH.Models;

namespace WEBDULICH.Services.Recommendation
{
    /// <summary>
    /// Interface for Tour Recommendation Service
    /// Provides personalized tour recommendations using ML algorithms
    /// </summary>
    public interface IRecommendationService
    {
        /// <summary>
        /// Get personalized tour recommendations for a user
        /// </summary>
        Task<List<Tour>> GetPersonalizedRecommendationsAsync(int userId, int count = 10);

        /// <summary>
        /// Get similar tours based on a tour
        /// </summary>
        Task<List<Tour>> GetSimilarToursAsync(int tourId, int count = 5);

        /// <summary>
        /// Get trending tours
        /// </summary>
        Task<List<Tour>> GetTrendingToursAsync(int count = 10);

        /// <summary>
        /// Get recommendations based on user preferences
        /// </summary>
        Task<List<Tour>> GetRecommendationsByPreferencesAsync(
            UserPreferences preferences, int count = 10);

        /// <summary>
        /// Get "Customers who viewed this also viewed" recommendations
        /// </summary>
        Task<List<Tour>> GetCollaborativeRecommendationsAsync(int tourId, int count = 5);

        /// <summary>
        /// Get seasonal recommendations
        /// </summary>
        Task<List<Tour>> GetSeasonalRecommendationsAsync(int month, int count = 10);

        /// <summary>
        /// Update user preferences based on behavior
        /// </summary>
        Task UpdateUserPreferencesAsync(int userId, int tourId, string action);
    }

    /// <summary>
    /// User Preferences Model
    /// </summary>
    public class UserPreferences
    {
        public int UserId { get; set; }
        public List<string> PreferredDestinations { get; set; } = new();
        public List<string> PreferredCategories { get; set; } = new();
        public decimal MinBudget { get; set; }
        public decimal MaxBudget { get; set; }
        public int PreferredDuration { get; set; }
        public List<string> Interests { get; set; } = new();
        public string TravelStyle { get; set; } = string.Empty; // Adventure, Relaxation, Cultural, etc.
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Recommendation Score Model
    /// </summary>
    public class RecommendationScore
    {
        public int TourId { get; set; }
        public Tour? Tour { get; set; }
        public double Score { get; set; }
        public Dictionary<string, double> FactorScores { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
    }
}

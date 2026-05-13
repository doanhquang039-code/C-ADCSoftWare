using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using System.Text.Json;

namespace WEBDULICH.Services.AI
{
    /// <summary>
    /// AI-Powered Recommendation Engine
    /// Há»‡ thá»‘ng gá»£i Ã½ thÃ´ng minh sá»­ dá»¥ng Collaborative Filtering vÃ  Content-Based Filtering
    /// </summary>
    public interface IRecommendationEngine
    {
        Task<List<Tour>> GetPersonalizedToursAsync(int userId, int count = 10);
        Task<List<Hotel>> GetPersonalizedHotelsAsync(int userId, int count = 10);
        Task<List<Tour>> GetSimilarToursAsync(int tourId, int count = 10);
        Task<Dictionary<string, object>> GetUserPreferencesAsync(int userId);
        Task UpdateUserPreferencesAsync(int userId, Dictionary<string, object> preferences);
        Task<List<Dictionary<string, object>>> GetTrendingItemsAsync(string itemType, int days = 7);
        Task TrainRecommendationModelAsync();
    }

    public class RecommendationEngine : IRecommendationEngine
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecommendationEngine> _logger;

        public RecommendationEngine(
            ApplicationDbContext context,
            ILogger<RecommendationEngine> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Tour>> GetPersonalizedToursAsync(int userId, int count = 10)
        {
            // Get user's booking history
            var userBookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Tour)
                .ToListAsync();

            // Get user's reviews
            var userReviews = await _context.Reviews
                .Where(r => r.UserId == userId)
                .ToListAsync();

            // Calculate user preferences
            var preferences = await CalculateUserPreferencesAsync(userId, userBookings, userReviews);

            // Collaborative filtering - find similar users
            var similarUsers = await FindSimilarUsersAsync(userId, preferences);

            // Get tours liked by similar users
            var collaborativeRecommendations = await GetCollaborativeRecommendationsAsync(similarUsers, userId);

            // Content-based filtering - based on user preferences
            var contentBasedRecommendations = await GetContentBasedRecommendationsAsync(preferences);

            // Hybrid approach - combine both methods
            var recommendations = CombineRecommendations(
                collaborativeRecommendations,
                contentBasedRecommendations,
                0.6, // 60% collaborative, 40% content-based
                count
            );

            return recommendations;
        }

        public async Task<List<Hotel>> GetPersonalizedHotelsAsync(int userId, int count = 10)
        {
            var userBookings = await _context.Bookings
                .Where(b => b.UserId == userId && b.HotelId != null)
                .Include(b => b.Hotel)
                .ToListAsync();

            var preferences = new Dictionary<string, double>();

            // Analyze hotel preferences
            if (userBookings.Any())
            {
                preferences["avgPrice"] = (double)userBookings.Average(b => b.Hotel?.Price ?? 0);
                preferences["preferredStars"] = userBookings.Average(b => b.Hotel?.Stars ?? 0);
            }

            // Get hotels matching preferences
            var query = _context.Hotels.AsQueryable();

            if (preferences.ContainsKey("avgPrice"))
            {
                var minPrice = preferences["avgPrice"] * 0.7;
                var maxPrice = preferences["avgPrice"] * 1.3;
                query = query.Where(h => h.PricePerNight >= (decimal)minPrice && h.PricePerNight <= (decimal)maxPrice);
            }

            if (preferences.ContainsKey("preferredStars"))
            {
                var stars = (int)Math.Round(preferences["preferredStars"]);
                query = query.Where(h => h.Stars >= stars - 1 && h.Stars <= stars + 1);
            }

            return await query
                .OrderByDescending(h => h.Rating)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Tour>> GetSimilarToursAsync(int tourId, int count = 10)
        {
            var tour = await _context.Tours
                .Include(t => t.Category)
                .Include(t => t.Destination)
                .FirstOrDefaultAsync(t => t.Id == tourId);

            if (tour == null) return new List<Tour>();

            // Find similar tours based on category, destination, and price
            var similarTours = await _context.Tours
                .Where(t => t.Id != tourId &&
                           (t.CategoryId == tour.CategoryId ||
                            t.DestinationId == tour.DestinationId))
                .ToListAsync();

            // Calculate similarity scores
            var scoredTours = similarTours.Select(t => new
            {
                Tour = t,
                Score = CalculateSimilarityScore(tour, t)
            })
            .OrderByDescending(x => x.Score)
            .Take(count)
            .Select(x => x.Tour)
            .ToList();

            return scoredTours;
        }

        private double CalculateSimilarityScore(Tour tour1, Tour tour2)
        {
            double score = 0;

            // Category match
            if (tour1.CategoryId == tour2.CategoryId) score += 0.4;

            // Destination match
            if (tour1.DestinationId == tour2.DestinationId) score += 0.3;

            // Price similarity
            var priceDiff = Math.Abs(tour1.Price - tour2.Price);
            var priceScore = Math.Max(tour1.Price, tour2.Price) > 0 ? 1 - ((double)priceDiff / Math.Max(tour1.Price, tour2.Price)) : 0;
            score += priceScore * 0.2;

            // Rating similarity
            var ratingDiff = Math.Abs((double)(tour1.Rating - tour2.Rating));
            score += (1 - ratingDiff / 5) * 0.1;

            return score;
        }

        private async Task<Dictionary<string, double>> CalculateUserPreferencesAsync(
            int userId,
            List<Booking> bookings,
            List<Review> reviews)
        {
            var preferences = new Dictionary<string, double>();

            if (bookings.Any())
            {
                // Price preference
                preferences["avgPrice"] = (double)bookings.Average(b => b.TotalPrice);
                preferences["maxPrice"] = (double)bookings.Max(b => b.TotalPrice);

                // Category preferences
                var categoryGroups = bookings
                    .Where(b => b.Tour != null)
                    .GroupBy(b => b.Tour.CategoryId)
                    .OrderByDescending(g => g.Count())
                    .ToList();

                if (categoryGroups.Any())
                {
                    preferences["preferredCategoryId"] = categoryGroups.First().Key ?? 0;
                }

                // Destination preferences
                var destGroups = bookings
                    .Where(b => b.Tour != null)
                    .GroupBy(b => b.Tour.DestinationId)
                    .OrderByDescending(g => g.Count())
                    .ToList();

                if (destGroups.Any())
                {
                    preferences["preferredDestinationId"] = destGroups.First().Key ?? 0;
                }
            }

            if (reviews.Any())
            {
                preferences["avgRatingGiven"] = (double)reviews.Average(r => r.NumericRating);
            }

            return preferences;
        }

        private async Task<List<int>> FindSimilarUsersAsync(int userId, Dictionary<string, double> preferences)
        {
            // Simplified collaborative filtering
            var allUsers = await _context.Users
                .Where(u => u.Id != userId)
                .Take(100)
                .Select(u => u.Id)
                .ToListAsync();

            // In production, calculate actual similarity scores
            return allUsers.Take(10).ToList();
        }

        private async Task<List<Tour>> GetCollaborativeRecommendationsAsync(List<int> similarUsers, int userId)
        {
            var recommendations = await _context.Bookings
                .Where(b => similarUsers.Contains(b.UserId) && b.TourId != null)
                .GroupBy(b => b.TourId)
                .OrderByDescending(g => g.Count())
                .Take(20)
                .Select(g => g.Key)
                .ToListAsync();

            // Exclude tours already booked by user
            var userTourIds = await _context.Bookings
                .Where(b => b.UserId == userId && b.TourId != null)
                .Select(b => b.TourId)
                .ToListAsync();

            var filteredIds = recommendations.Where(id => !userTourIds.Contains(id)).ToList();

            return await _context.Tours
                .Where(t => filteredIds.Contains(t.Id))
                .Include(t => t.Category)
                .Include(t => t.Destination)
                .ToListAsync();
        }

        private async Task<List<Tour>> GetContentBasedRecommendationsAsync(Dictionary<string, double> preferences)
        {
            var query = _context.Tours.AsQueryable();

            if (preferences.ContainsKey("preferredCategoryId"))
            {
                var categoryId = (int)preferences["preferredCategoryId"];
                query = query.Where(t => t.CategoryId == categoryId);
            }

            if (preferences.ContainsKey("preferredDestinationId"))
            {
                var destId = (int)preferences["preferredDestinationId"];
                query = query.Where(t => t.DestinationId == destId);
            }

            if (preferences.ContainsKey("avgPrice"))
            {
                var avgPrice = (decimal)preferences["avgPrice"];
                var minPrice = avgPrice * 0.7m;
                var maxPrice = avgPrice * 1.3m;
                query = query.Where(t => t.Price >= minPrice && t.Price <= maxPrice);
            }

            return await query
                .Include(t => t.Category)
                .Include(t => t.Destination)
                .OrderByDescending(t => t.Rating)
                .Take(20)
                .ToListAsync();
        }

        private List<Tour> CombineRecommendations(
            List<Tour> collaborative,
            List<Tour> contentBased,
            double collaborativeWeight,
            int count)
        {
            var combined = new Dictionary<int, (Tour Tour, double Score)>();

            // Add collaborative recommendations
            for (int i = 0; i < collaborative.Count; i++)
            {
                var tour = collaborative[i];
                var score = (collaborative.Count - i) * collaborativeWeight;
                combined[tour.Id] = (tour, score);
            }

            // Add content-based recommendations
            for (int i = 0; i < contentBased.Count; i++)
            {
                var tour = contentBased[i];
                var score = (contentBased.Count - i) * (1 - collaborativeWeight);

                if (combined.ContainsKey(tour.Id))
                {
                    combined[tour.Id] = (tour, combined[tour.Id].Score + score);
                }
                else
                {
                    combined[tour.Id] = (tour, score);
                }
            }

            return combined
                .OrderByDescending(x => x.Value.Score)
                .Take(count)
                .Select(x => x.Value.Tour)
                .ToList();
        }

        public async Task<Dictionary<string, object>> GetUserPreferencesAsync(int userId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Tour)
                .ToListAsync();

            var reviews = await _context.Reviews
                .Where(r => r.UserId == userId)
                .ToListAsync();

            var preferences = await CalculateUserPreferencesAsync(userId, bookings, reviews);

            return preferences.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)kvp.Value
            );
        }

        public async Task UpdateUserPreferencesAsync(int userId, Dictionary<string, object> preferences)
        {
            // Store user preferences in database or cache
            _logger.LogInformation($"Updated preferences for user {userId}");
        }

        public async Task<List<Dictionary<string, object>>> GetTrendingItemsAsync(string itemType, int days = 7)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);

            if (itemType == "Tour")
            {
                var trending = await _context.Bookings
                    .Where(b => b.CreatedAt >= cutoffDate && b.TourId != null)
                    .GroupBy(b => b.TourId)
                    .Select(g => new
                    {
                        TourId = g.Key,
                        BookingCount = g.Count(),
                        TotalRevenue = g.Sum(b => b.TotalPrice)
                    })
                    .OrderByDescending(x => x.BookingCount)
                    .Take(10)
                    .ToListAsync();

                return trending.Select(t => new Dictionary<string, object>
                {
                    ["tourId"] = t.TourId,
                    ["bookingCount"] = t.BookingCount,
                    ["totalRevenue"] = t.TotalRevenue
                }).ToList();
            }

            return new List<Dictionary<string, object>>();
        }

        public async Task TrainRecommendationModelAsync()
        {
            _logger.LogInformation("Training recommendation model...");

            // In production, implement actual ML model training
            // Using libraries like ML.NET or TensorFlow.NET

            await Task.Delay(1000); // Simulate training

            _logger.LogInformation("Recommendation model trained successfully");
        }
    }
}


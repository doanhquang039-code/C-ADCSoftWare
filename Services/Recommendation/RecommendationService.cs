using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using WEBDULICH.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace WEBDULICH.Services.Recommendation
{
    /// <summary>
    /// Tour Recommendation Service Implementation
    /// Uses collaborative filtering and content-based filtering
    /// </summary>
    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ILogger<RecommendationService> _logger;

        public RecommendationService(
            ApplicationDbContext context,
            IDistributedCache cache,
            ILogger<RecommendationService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<Tour>> GetPersonalizedRecommendationsAsync(int userId, int count = 10)
        {
            try
            {
                // Check cache
                var cacheKey = $"recommendations:user:{userId}:{count}";
                var cachedData = await _cache.GetStringAsync(cacheKey);
                
                if (!string.IsNullOrEmpty(cachedData))
                {
                    var tourIds = JsonSerializer.Deserialize<List<int>>(cachedData) ?? new List<int>();
                    return await _context.Tours
                        .Where(t => tourIds.Contains(t.Id))
                        .Include(t => t.Destination)
                        .ToListAsync();
                }

                // Get user's order history
                var userOrders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.Tour)
                    .ToListAsync();

                // Get user's wishlist
                var userWishlist = await _context.Wishlists
                    .Where(w => w.UserId == userId && w.ItemType == "Tour")
                    .ToListAsync();

                // Calculate scores for all tours
                var allTours = await _context.Tours
                    .Include(t => t.Destination)
                    .Include(t => t.Reviews)
                    .Where(t => t.Quantity > 0) // Active tours have quantity > 0
                    .ToListAsync();

                var scores = new List<RecommendationScore>();

                foreach (var tour in allTours)
                {
                    // Skip if user already ordered
                    if (userOrders.Any(o => o.TourId == tour.Id))
                        continue;

                    var score = CalculatePersonalizedScore(tour, userOrders, userWishlist);
                    scores.Add(new RecommendationScore
                    {
                        TourId = tour.Id,
                        Tour = tour,
                        Score = score
                    });
                }

                // Get top recommendations
                var recommendations = scores
                    .OrderByDescending(s => s.Score)
                    .Take(count)
                    .Select(s => s.Tour!)
                    .ToList();

                // Cache for 1 hour
                var tourIds2 = recommendations.Select(t => t.Id).ToList();
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(tourIds2), cacheOptions);

                _logger.LogInformation("Generated {Count} personalized recommendations for user {UserId}", 
                    recommendations.Count, userId);

                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating personalized recommendations for user {UserId}", userId);
                return await GetTrendingToursAsync(count);
            }
        }

        public async Task<List<Tour>> GetSimilarToursAsync(int tourId, int count = 5)
        {
            try
            {
                var tour = await _context.Tours
                    .Include(t => t.Destination)
                    .FirstOrDefaultAsync(t => t.Id == tourId);

                if (tour == null)
                    return new List<Tour>();

                // Find similar tours based on:
                // 1. Same destination
                // 2. Similar price range
                // 3. Similar duration
                var similarTours = await _context.Tours
                    .Include(t => t.Destination)
                    .Include(t => t.Reviews)
                    .Where(t => t.Id != tourId && t.Quantity > 0)
                    .Where(t => 
                        t.DestinationId == tour.DestinationId ||
                        (t.Price >= tour.Price * 0.7m && t.Price <= tour.Price * 1.3m) ||
                        (t.Duration >= tour.Duration - 2 && t.Duration <= tour.Duration + 2))
                    .OrderByDescending(t => t.Reviews.Count > 0 ? t.Reviews.Average(r => Convert.ToDouble(r.Rating)) : 0)
                    .ThenByDescending(t => t.Orders.Count)
                    .Take(count)
                    .ToListAsync();

                return similarTours;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting similar tours for tour {TourId}", tourId);
                return new List<Tour>();
            }
        }

        public async Task<List<Tour>> GetTrendingToursAsync(int count = 10)
        {
            try
            {
                // Check cache
                var cacheKey = $"recommendations:trending:{count}";
                var cachedData = await _cache.GetStringAsync(cacheKey);
                
                if (!string.IsNullOrEmpty(cachedData))
                {
                    var tourIds = JsonSerializer.Deserialize<List<int>>(cachedData) ?? new List<int>();
                    return await _context.Tours
                        .Where(t => tourIds.Contains(t.Id))
                        .Include(t => t.Destination)
                        .ToListAsync();
                }

                // Get tours with most bookings in last 30 days
                var thirtyDaysAgo = DateTime.Now.AddDays(-30);
                
                var trendingTours = await _context.Tours
                    .Include(t => t.Destination)
                    .Include(t => t.Reviews)
                    .Where(t => t.Quantity > 0)
                    .OrderByDescending(t => t.Orders.Count(o => o.OrderDate >= thirtyDaysAgo))
                    .ThenByDescending(t => t.Reviews.Count > 0 ? t.Reviews.Average(r => Convert.ToDouble(r.Rating)) : 0)
                    .Take(count)
                    .ToListAsync();

                // Cache for 2 hours
                var tourIds2 = trendingTours.Select(t => t.Id).ToList();
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(tourIds2), cacheOptions);

                return trendingTours;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending tours");
                return new List<Tour>();
            }
        }

        public async Task<List<Tour>> GetRecommendationsByPreferencesAsync(
            UserPreferences preferences, int count = 10)
        {
            try
            {
                var query = _context.Tours
                    .Include(t => t.Destination)
                    .Include(t => t.Reviews)
                    .Where(t => t.Quantity > 0)
                    .AsQueryable();

                // Filter by budget
                if (preferences.MaxBudget > 0)
                {
                    query = query.Where(t => t.Price >= preferences.MinBudget && 
                                           t.Price <= preferences.MaxBudget);
                }

                // Filter by duration
                if (preferences.PreferredDuration > 0)
                {
                    query = query.Where(t => 
                        t.Duration >= preferences.PreferredDuration - 2 && 
                        t.Duration <= preferences.PreferredDuration + 2);
                }

                // Filter by destinations
                if (preferences.PreferredDestinations.Any())
                {
                    query = query.Where(t => 
                        preferences.PreferredDestinations.Contains(t.Destination.Name));
                }

                var tours = await query
                    .OrderByDescending(t => t.Reviews.Count > 0 ? t.Reviews.Average(r => Convert.ToDouble(r.Rating)) : 0)
                    .ThenByDescending(t => t.Orders.Count)
                    .Take(count)
                    .ToListAsync();

                return tours;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations by preferences");
                return new List<Tour>();
            }
        }

        public async Task<List<Tour>> GetCollaborativeRecommendationsAsync(int tourId, int count = 5)
        {
            try
            {
                // Find users who ordered this tour
                var usersWhoOrdered = await _context.Orders
                    .Where(o => o.TourId == tourId)
                    .Select(o => o.UserId)
                    .Distinct()
                    .ToListAsync();

                if (!usersWhoOrdered.Any())
                    return await GetSimilarToursAsync(tourId, count);

                // Find other tours these users ordered
                var otherTours = await _context.Orders
                    .Where(o => usersWhoOrdered.Contains(o.UserId) && o.TourId != tourId)
                    .GroupBy(o => o.TourId)
                    .Select(g => new { TourId = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(count)
                    .ToListAsync();

                var tourIds = otherTours.Select(x => x.TourId).ToList();
                
                var recommendations = await _context.Tours
                    .Include(t => t.Destination)
                    .Include(t => t.Reviews)
                    .Where(t => tourIds.Contains(t.Id) && t.Quantity > 0)
                    .ToListAsync();

                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collaborative recommendations");
                return new List<Tour>();
            }
        }

        public async Task<List<Tour>> GetSeasonalRecommendationsAsync(int month, int count = 10)
        {
            try
            {
                // Define seasonal preferences
                var seasonalDestinations = GetSeasonalDestinations(month);

                var tours = await _context.Tours
                    .Include(t => t.Destination)
                    .Include(t => t.Reviews)
                    .Where(t => t.Quantity > 0 && 
                               seasonalDestinations.Contains(t.Destination.Name))
                    .OrderByDescending(t => t.Reviews.Count > 0 ? t.Reviews.Average(r => Convert.ToDouble(r.Rating)) : 0)
                    .ThenByDescending(t => t.Orders.Count)
                    .Take(count)
                    .ToListAsync();

                return tours;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seasonal recommendations");
                return new List<Tour>();
            }
        }

        public async Task UpdateUserPreferencesAsync(int userId, int tourId, string action)
        {
            try
            {
                // Track user behavior for future recommendations
                // Actions: "view", "wishlist", "order", "review"
                
                _logger.LogInformation(
                    "User {UserId} performed action {Action} on tour {TourId}", 
                    userId, action, tourId);

                // Clear user's recommendation cache
                var cacheKey = $"recommendations:user:{userId}:*";
                // Note: In production, use a more sophisticated cache invalidation strategy
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user preferences");
            }
        }

        // Helper methods
        private double CalculatePersonalizedScore(
            Tour tour, 
            List<Orders> userOrders, 
            List<Wishlist> userWishlist)
        {
            double score = 0;

            // Base score from tour quality
            var avgRating = tour.Reviews.Count > 0 
                ? tour.Reviews.Average(r => Convert.ToDouble(r.Rating)) 
                : 0;
            var totalBookings = tour.Orders.Count;
            
            score += avgRating * 10;
            score += Math.Min(totalBookings / 10.0, 20);

            // Boost if in wishlist
            if (userWishlist.Any(w => w.ItemId == tour.Id))
            {
                score += 30;
            }

            // Boost if similar to previously ordered tours
            foreach (var order in userOrders)
            {
                if (order.Tour?.DestinationId == tour.DestinationId)
                {
                    score += 15;
                }
                
                if (order.Tour != null && 
                    Math.Abs(order.Tour.Price - tour.Price) < order.Tour.Price * 0.3m)
                {
                    score += 10;
                }
            }

            // Boost for popular tours
            if (totalBookings > 100)
            {
                score += 10;
            }

            // Boost for highly rated tours
            if (avgRating >= 4.5)
            {
                score += 15;
            }

            return score;
        }

        private List<string> GetSeasonalDestinations(int month)
        {
            // Best destinations by month in Vietnam
            return month switch
            {
                1 or 2 or 3 => new List<string> { "Phú Quốc", "Nha Trang", "Đà Lạt", "Sài Gòn" },
                4 or 5 => new List<string> { "Đà Nẵng", "Hội An", "Huế", "Quy Nhơn" },
                6 or 7 or 8 => new List<string> { "Sapa", "Hà Giang", "Ninh Bình", "Hạ Long" },
                9 or 10 or 11 => new List<string> { "Hà Nội", "Hạ Long", "Ninh Bình", "Đà Nẵng" },
                12 => new List<string> { "Phú Quốc", "Nha Trang", "Đà Lạt", "Sài Gòn" },
                _ => new List<string> { "Hà Nội", "Sài Gòn", "Đà Nẵng" }
            };
        }
    }
}

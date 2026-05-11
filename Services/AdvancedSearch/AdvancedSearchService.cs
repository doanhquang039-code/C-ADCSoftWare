using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using System.Text.Json;

namespace WEBDULICH.Services.AdvancedSearch
{
    public class AdvancedSearchService : IAdvancedSearchService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdvancedSearchService> _logger;

        public AdvancedSearchService(
            ApplicationDbContext context,
            ILogger<AdvancedSearchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Dictionary<string, object>> SearchAllAsync(string query, int page = 1, int pageSize = 20)
        {
            var tours = await SearchToursAsync(query);
            var hotels = await SearchHotelsAsync(query);
            var destinations = await SearchDestinationsAsync(query);

            var skip = (page - 1) * pageSize;

            return new Dictionary<string, object>
            {
                ["query"] = query,
                ["tours"] = tours.Skip(skip).Take(pageSize).ToList(),
                ["hotels"] = hotels.Skip(skip).Take(pageSize).ToList(),
                ["destinations"] = destinations.Skip(skip).Take(pageSize).ToList(),
                ["totalTours"] = tours.Count,
                ["totalHotels"] = hotels.Count,
                ["totalDestinations"] = destinations.Count,
                ["page"] = page,
                ["pageSize"] = pageSize
            };
        }

        public async Task<List<Tour>> SearchToursAsync(string query, Dictionary<string, object> filters = null)
        {
            var toursQuery = _context.Tours.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                query = query.ToLower();
                toursQuery = toursQuery.Where(t =>
                    t.Name.ToLower().Contains(query) ||
                    t.Description.ToLower().Contains(query) ||
                    t.Location.ToLower().Contains(query));
            }

            if (filters != null)
            {
                toursQuery = ApplyTourFilters(toursQuery, filters);
            }

            return await toursQuery
                .Include(t => t.Category)
                .Include(t => t.Destination)
                .OrderByDescending(t => t.Rating)
                .ToListAsync();
        }

        public async Task<List<Hotel>> SearchHotelsAsync(string query, Dictionary<string, object> filters = null)
        {
            var hotelsQuery = _context.Hotels.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                query = query.ToLower();
                hotelsQuery = hotelsQuery.Where(h =>
                    h.Name.ToLower().Contains(query) ||
                    h.Description.ToLower().Contains(query) ||
                    h.Location.ToLower().Contains(query));
            }

            if (filters != null)
            {
                hotelsQuery = ApplyHotelFilters(hotelsQuery, filters);
            }

            return await hotelsQuery
                .OrderByDescending(h => h.Rating)
                .ToListAsync();
        }

        public async Task<List<Destination>> SearchDestinationsAsync(string query)
        {
            if (string.IsNullOrEmpty(query))
                return await _context.Destinations.ToListAsync();

            query = query.ToLower();
            return await _context.Destinations
                .Where(d =>
                    d.Name.ToLower().Contains(query) ||
                    d.Description.ToLower().Contains(query) ||
                    d.Location.ToLower().Contains(query))
                .ToListAsync();
        }

        public async Task<List<Tour>> FilterToursAsync(Dictionary<string, object> filters)
        {
            var query = _context.Tours.AsQueryable();
            query = ApplyTourFilters(query, filters);

            return await query
                .Include(t => t.Category)
                .Include(t => t.Destination)
                .ToListAsync();
        }

        public async Task<List<Hotel>> FilterHotelsAsync(Dictionary<string, object> filters)
        {
            var query = _context.Hotels.AsQueryable();
            query = ApplyHotelFilters(query, filters);

            return await query.ToListAsync();
        }

        private IQueryable<Tour> ApplyTourFilters(IQueryable<Tour> query, Dictionary<string, object> filters)
        {
            if (filters.ContainsKey("minPrice") && filters["minPrice"] != null)
                query = query.Where(t => t.Price >= Convert.ToDecimal(filters["minPrice"]));

            if (filters.ContainsKey("maxPrice") && filters["maxPrice"] != null)
                query = query.Where(t => t.Price <= Convert.ToDecimal(filters["maxPrice"]));

            if (filters.ContainsKey("minRating") && filters["minRating"] != null)
                query = query.Where(t => t.Rating >= Convert.ToDecimal(filters["minRating"]));

            if (filters.ContainsKey("categoryId") && filters["categoryId"] != null)
                query = query.Where(t => t.CategoryId == Convert.ToInt32(filters["categoryId"]));

            if (filters.ContainsKey("destinationId") && filters["destinationId"] != null)
                query = query.Where(t => t.DestinationId == Convert.ToInt32(filters["destinationId"]));

            if (filters.ContainsKey("duration") && filters["duration"] != null)
                query = query.Where(t => t.Duration == filters["duration"].ToString());

            if (filters.ContainsKey("available") && filters["available"] != null)
                query = query.Where(t => t.Available == Convert.ToBoolean(filters["available"]));

            return query;
        }

        private IQueryable<Hotel> ApplyHotelFilters(IQueryable<Hotel> query, Dictionary<string, object> filters)
        {
            if (filters.ContainsKey("minPrice") && filters["minPrice"] != null)
                query = query.Where(h => h.PricePerNight >= Convert.ToDecimal(filters["minPrice"]));

            if (filters.ContainsKey("maxPrice") && filters["maxPrice"] != null)
                query = query.Where(h => h.PricePerNight <= Convert.ToDecimal(filters["maxPrice"]));

            if (filters.ContainsKey("minRating") && filters["minRating"] != null)
                query = query.Where(h => h.Rating >= Convert.ToDecimal(filters["minRating"]));

            if (filters.ContainsKey("stars") && filters["stars"] != null)
                query = query.Where(h => h.Stars == Convert.ToInt32(filters["stars"]));

            return query;
        }

        public async Task<Dictionary<string, object>> GetSearchFacetsAsync(string entityType, string query = null)
        {
            if (entityType == "Tour")
            {
                var tours = await SearchToursAsync(query);
                return new Dictionary<string, object>
                {
                    ["priceRanges"] = GetPriceRanges(tours.Select(t => t.Price).ToList()),
                    ["categories"] = tours.GroupBy(t => t.Category?.Name).Select(g => new { name = g.Key, count = g.Count() }).ToList(),
                    ["destinations"] = tours.GroupBy(t => t.Destination?.Name).Select(g => new { name = g.Key, count = g.Count() }).ToList(),
                    ["ratings"] = tours.GroupBy(t => Math.Floor(t.Rating)).Select(g => new { rating = g.Key, count = g.Count() }).ToList()
                };
            }
            else if (entityType == "Hotel")
            {
                var hotels = await SearchHotelsAsync(query);
                return new Dictionary<string, object>
                {
                    ["priceRanges"] = GetPriceRanges(hotels.Select(h => h.PricePerNight).ToList()),
                    ["stars"] = hotels.GroupBy(h => h.Stars).Select(g => new { stars = g.Key, count = g.Count() }).ToList(),
                    ["ratings"] = hotels.GroupBy(h => Math.Floor(h.Rating)).Select(g => new { rating = g.Key, count = g.Count() }).ToList()
                };
            }

            return new Dictionary<string, object>();
        }

        private List<Dictionary<string, object>> GetPriceRanges(List<decimal> prices)
        {
            if (!prices.Any()) return new List<Dictionary<string, object>>();

            var ranges = new List<Dictionary<string, object>>
            {
                new() { ["range"] = "0-1000000", ["min"] = 0, ["max"] = 1000000, ["count"] = prices.Count(p => p < 1000000) },
                new() { ["range"] = "1000000-3000000", ["min"] = 1000000, ["max"] = 3000000, ["count"] = prices.Count(p => p >= 1000000 && p < 3000000) },
                new() { ["range"] = "3000000-5000000", ["min"] = 3000000, ["max"] = 5000000, ["count"] = prices.Count(p => p >= 3000000 && p < 5000000) },
                new() { ["range"] = "5000000+", ["min"] = 5000000, ["max"] = 999999999, ["count"] = prices.Count(p => p >= 5000000) }
            };

            return ranges.Where(r => (int)r["count"] > 0).ToList();
        }

        public async Task<Dictionary<string, List<string>>> GetAvailableFiltersAsync(string entityType)
        {
            var filters = new Dictionary<string, List<string>>();

            if (entityType == "Tour")
            {
                filters["categories"] = await _context.Categories.Select(c => c.Name).ToListAsync();
                filters["destinations"] = await _context.Destinations.Select(d => d.Name).ToListAsync();
                filters["durations"] = new List<string> { "1 day", "2-3 days", "4-7 days", "1 week+" };
            }
            else if (entityType == "Hotel")
            {
                filters["stars"] = new List<string> { "1", "2", "3", "4", "5" };
            }

            filters["priceRanges"] = new List<string> { "0-1M", "1M-3M", "3M-5M", "5M+" };
            filters["ratings"] = new List<string> { "4+", "3+", "2+", "1+" };

            return filters;
        }

        public async Task<List<string>> GetAutocompleteSuggestionsAsync(string query, string entityType = "all")
        {
            if (string.IsNullOrEmpty(query) || query.Length < 2)
                return new List<string>();

            query = query.ToLower();
            var suggestions = new HashSet<string>();

            if (entityType == "all" || entityType == "Tour")
            {
                var tourNames = await _context.Tours
                    .Where(t => t.Name.ToLower().Contains(query))
                    .Select(t => t.Name)
                    .Take(5)
                    .ToListAsync();
                foreach (var name in tourNames) suggestions.Add(name);
            }

            if (entityType == "all" || entityType == "Hotel")
            {
                var hotelNames = await _context.Hotels
                    .Where(h => h.Name.ToLower().Contains(query))
                    .Select(h => h.Name)
                    .Take(5)
                    .ToListAsync();
                foreach (var name in hotelNames) suggestions.Add(name);
            }

            if (entityType == "all" || entityType == "Destination")
            {
                var destNames = await _context.Destinations
                    .Where(d => d.Name.ToLower().Contains(query))
                    .Select(d => d.Name)
                    .Take(5)
                    .ToListAsync();
                foreach (var name in destNames) suggestions.Add(name);
            }

            return suggestions.Take(10).ToList();
        }

        public async Task<List<string>> GetSearchSuggestionsAsync(string query)
        {
            return await GetAutocompleteSuggestionsAsync(query);
        }

        public async Task<List<string>> GetPopularSearchesAsync(int count = 10)
        {
            // Would need search log table in real implementation
            return new List<string>
            {
                "Ha Long Bay",
                "Sapa",
                "Phu Quoc",
                "Da Nang",
                "Hoi An",
                "Nha Trang",
                "Dalat",
                "Mui Ne"
            }.Take(count).ToList();
        }

        public async Task<List<Tour>> SearchToursByLocationAsync(decimal latitude, decimal longitude, decimal radiusKm)
        {
            // Simplified geo search - would use spatial queries in production
            var tours = await _context.Tours
                .Include(t => t.Category)
                .Include(t => t.Destination)
                .ToListAsync();

            // Filter by approximate distance (simplified)
            return tours.Take(20).ToList();
        }

        public async Task<List<Hotel>> SearchHotelsByLocationAsync(decimal latitude, decimal longitude, decimal radiusKm)
        {
            var hotels = await _context.Hotels.ToListAsync();
            return hotels.Take(20).ToList();
        }

        public async Task<List<Destination>> SearchDestinationsByLocationAsync(decimal latitude, decimal longitude, decimal radiusKm)
        {
            var destinations = await _context.Destinations.ToListAsync();
            return destinations.Take(20).ToList();
        }

        public async Task<Dictionary<string, object>> SmartSearchAsync(string query, int userId = 0)
        {
            var results = await SearchAllAsync(query);

            // Add personalization if user is logged in
            if (userId > 0)
            {
                var personalizedTours = await GetPersonalizedSearchResultsAsync(userId, query);
                results["personalizedTours"] = personalizedTours;
            }

            // Add suggestions
            results["suggestions"] = await GetSearchSuggestionsAsync(query);

            // Log search
            var totalResults = (int)results["totalTours"] + (int)results["totalHotels"] + (int)results["totalDestinations"];
            await LogSearchAsync(query, "all", userId, totalResults);

            return results;
        }

        public async Task<List<Tour>> GetPersonalizedSearchResultsAsync(int userId, string query)
        {
            // Get user's booking history
            var userBookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Tour)
                .ToListAsync();

            var preferredCategories = userBookings
                .Where(b => b.Tour != null)
                .Select(b => b.Tour.CategoryId)
                .Distinct()
                .ToList();

            // Search with preference boost
            var tours = await SearchToursAsync(query);

            return tours
                .OrderByDescending(t => preferredCategories.Contains(t.CategoryId) ? 1 : 0)
                .ThenByDescending(t => t.Rating)
                .Take(10)
                .ToList();
        }

        public async Task LogSearchAsync(string query, string entityType, int userId, int resultsCount)
        {
            // Would log to search analytics table
            _logger.LogInformation($"Search: {query} ({entityType}) by user {userId} - {resultsCount} results");
        }

        public async Task<Dictionary<string, object>> GetSearchAnalyticsAsync(int days = 30)
        {
            // Would query search log table
            return new Dictionary<string, object>
            {
                ["totalSearches"] = 1000,
                ["uniqueQueries"] = 500,
                ["averageResults"] = 15,
                ["topQueries"] = await GetPopularSearchesAsync(10)
            };
        }

        public async Task<List<string>> GetTrendingSearchesAsync(int count = 10)
        {
            return await GetPopularSearchesAsync(count);
        }

        public async Task<List<Tour>> GetSimilarToursAsync(int tourId, int count = 10)
        {
            var tour = await _context.Tours.FindAsync(tourId);
            if (tour == null) return new List<Tour>();

            return await _context.Tours
                .Where(t => t.Id != tourId && 
                           (t.CategoryId == tour.CategoryId || 
                            t.DestinationId == tour.DestinationId))
                .OrderByDescending(t => t.Rating)
                .Take(count)
                .Include(t => t.Category)
                .Include(t => t.Destination)
                .ToListAsync();
        }

        public async Task<List<Hotel>> GetSimilarHotelsAsync(int hotelId, int count = 10)
        {
            var hotel = await _context.Hotels.FindAsync(hotelId);
            if (hotel == null) return new List<Hotel>();

            return await _context.Hotels
                .Where(h => h.Id != hotelId && h.Stars == hotel.Stars)
                .OrderByDescending(h => h.Rating)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Dictionary<string, object>>> GetUserSearchHistoryAsync(int userId, int count = 20)
        {
            // Would query search history table
            return new List<Dictionary<string, object>>();
        }

        public async Task ClearUserSearchHistoryAsync(int userId)
        {
            // Would clear search history table
            _logger.LogInformation($"Cleared search history for user {userId}");
        }
    }
}

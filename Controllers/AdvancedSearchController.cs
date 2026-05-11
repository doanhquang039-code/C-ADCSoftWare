using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Services.AdvancedSearch;

namespace WEBDULICH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvancedSearchController : ControllerBase
    {
        private readonly IAdvancedSearchService _searchService;
        private readonly ILogger<AdvancedSearchController> _logger;

        public AdvancedSearchController(
            IAdvancedSearchService searchService,
            ILogger<AdvancedSearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        /// <summary>
        /// Search all entities
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchAll([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var results = await _searchService.SearchAllAsync(query, page, pageSize);
                return Ok(new { success = true, data = results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Search tours
        /// </summary>
        [HttpGet("tours")]
        public async Task<IActionResult> SearchTours([FromQuery] string query, [FromQuery] Dictionary<string, object> filters = null)
        {
            try
            {
                var tours = await _searchService.SearchToursAsync(query, filters);
                return Ok(new { success = true, data = tours, count = tours.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tours");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Search hotels
        /// </summary>
        [HttpGet("hotels")]
        public async Task<IActionResult> SearchHotels([FromQuery] string query, [FromQuery] Dictionary<string, object> filters = null)
        {
            try
            {
                var hotels = await _searchService.SearchHotelsAsync(query, filters);
                return Ok(new { success = true, data = hotels, count = hotels.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching hotels");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Search destinations
        /// </summary>
        [HttpGet("destinations")]
        public async Task<IActionResult> SearchDestinations([FromQuery] string query)
        {
            try
            {
                var destinations = await _searchService.SearchDestinationsAsync(query);
                return Ok(new { success = true, data = destinations, count = destinations.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching destinations");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Filter tours
        /// </summary>
        [HttpPost("tours/filter")]
        public async Task<IActionResult> FilterTours([FromBody] Dictionary<string, object> filters)
        {
            try
            {
                var tours = await _searchService.FilterToursAsync(filters);
                return Ok(new { success = true, data = tours, count = tours.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering tours");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Filter hotels
        /// </summary>
        [HttpPost("hotels/filter")]
        public async Task<IActionResult> FilterHotels([FromBody] Dictionary<string, object> filters)
        {
            try
            {
                var hotels = await _searchService.FilterHotelsAsync(filters);
                return Ok(new { success = true, data = hotels, count = hotels.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering hotels");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get search facets
        /// </summary>
        [HttpGet("facets/{entityType}")]
        public async Task<IActionResult> GetSearchFacets(string entityType, [FromQuery] string query = null)
        {
            try
            {
                var facets = await _searchService.GetSearchFacetsAsync(entityType, query);
                return Ok(new { success = true, data = facets });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facets");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get available filters
        /// </summary>
        [HttpGet("filters/{entityType}")]
        public async Task<IActionResult> GetAvailableFilters(string entityType)
        {
            try
            {
                var filters = await _searchService.GetAvailableFiltersAsync(entityType);
                return Ok(new { success = true, data = filters });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filters");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get autocomplete suggestions
        /// </summary>
        [HttpGet("autocomplete")]
        public async Task<IActionResult> GetAutocompleteSuggestions([FromQuery] string query, [FromQuery] string entityType = "all")
        {
            try
            {
                var suggestions = await _searchService.GetAutocompleteSuggestionsAsync(query, entityType);
                return Ok(new { success = true, data = suggestions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting autocomplete");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get search suggestions
        /// </summary>
        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSearchSuggestions([FromQuery] string query)
        {
            try
            {
                var suggestions = await _searchService.GetSearchSuggestionsAsync(query);
                return Ok(new { success = true, data = suggestions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suggestions");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get popular searches
        /// </summary>
        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularSearches([FromQuery] int count = 10)
        {
            try
            {
                var searches = await _searchService.GetPopularSearchesAsync(count);
                return Ok(new { success = true, data = searches });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular searches");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Search tours by location
        /// </summary>
        [HttpGet("tours/location")]
        public async Task<IActionResult> SearchToursByLocation([FromQuery] decimal latitude, [FromQuery] decimal longitude, [FromQuery] decimal radiusKm = 50)
        {
            try
            {
                var tours = await _searchService.SearchToursByLocationAsync(latitude, longitude, radiusKm);
                return Ok(new { success = true, data = tours, count = tours.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching by location");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Search hotels by location
        /// </summary>
        [HttpGet("hotels/location")]
        public async Task<IActionResult> SearchHotelsByLocation([FromQuery] decimal latitude, [FromQuery] decimal longitude, [FromQuery] decimal radiusKm = 50)
        {
            try
            {
                var hotels = await _searchService.SearchHotelsByLocationAsync(latitude, longitude, radiusKm);
                return Ok(new { success = true, data = hotels, count = hotels.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching by location");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Smart search with personalization
        /// </summary>
        [HttpGet("smart")]
        public async Task<IActionResult> SmartSearch([FromQuery] string query, [FromQuery] int userId = 0)
        {
            try
            {
                var results = await _searchService.SmartSearchAsync(query, userId);
                return Ok(new { success = true, data = results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in smart search");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get personalized search results
        /// </summary>
        [HttpGet("personalized/{userId}")]
        public async Task<IActionResult> GetPersonalizedResults(int userId, [FromQuery] string query)
        {
            try
            {
                var results = await _searchService.GetPersonalizedSearchResultsAsync(userId, query);
                return Ok(new { success = true, data = results, count = results.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting personalized results");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get search analytics
        /// </summary>
        [HttpGet("analytics")]
        public async Task<IActionResult> GetSearchAnalytics([FromQuery] int days = 30)
        {
            try
            {
                var analytics = await _searchService.GetSearchAnalyticsAsync(days);
                return Ok(new { success = true, data = analytics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get trending searches
        /// </summary>
        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingSearches([FromQuery] int count = 10)
        {
            try
            {
                var trending = await _searchService.GetTrendingSearchesAsync(count);
                return Ok(new { success = true, data = trending });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get similar tours
        /// </summary>
        [HttpGet("tours/{tourId}/similar")]
        public async Task<IActionResult> GetSimilarTours(int tourId, [FromQuery] int count = 10)
        {
            try
            {
                var tours = await _searchService.GetSimilarToursAsync(tourId, count);
                return Ok(new { success = true, data = tours, count = tours.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting similar tours");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get similar hotels
        /// </summary>
        [HttpGet("hotels/{hotelId}/similar")]
        public async Task<IActionResult> GetSimilarHotels(int hotelId, [FromQuery] int count = 10)
        {
            try
            {
                var hotels = await _searchService.GetSimilarHotelsAsync(hotelId, count);
                return Ok(new { success = true, data = hotels, count = hotels.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting similar hotels");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get user search history
        /// </summary>
        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetUserSearchHistory(int userId, [FromQuery] int count = 20)
        {
            try
            {
                var history = await _searchService.GetUserSearchHistoryAsync(userId, count);
                return Ok(new { success = true, data = history });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Clear user search history
        /// </summary>
        [HttpDelete("history/{userId}")]
        public async Task<IActionResult> ClearUserSearchHistory(int userId)
        {
            try
            {
                await _searchService.ClearUserSearchHistoryAsync(userId);
                return Ok(new { success = true, message = "History cleared" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing history");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}

using WEBDULICH.Models;

namespace WEBDULICH.Services.AdvancedSearch
{
    public interface IAdvancedSearchService
    {
        // Multi-entity Search
        Task<Dictionary<string, object>> SearchAllAsync(string query, int page = 1, int pageSize = 20);
        Task<List<Tour>> SearchToursAsync(string query, Dictionary<string, object> filters = null);
        Task<List<Hotel>> SearchHotelsAsync(string query, Dictionary<string, object> filters = null);
        Task<List<Destination>> SearchDestinationsAsync(string query);

        // Advanced Filters
        Task<List<Tour>> FilterToursAsync(Dictionary<string, object> filters);
        Task<List<Hotel>> FilterHotelsAsync(Dictionary<string, object> filters);

        // Faceted Search
        Task<Dictionary<string, object>> GetSearchFacetsAsync(string entityType, string query = null);
        Task<Dictionary<string, List<string>>> GetAvailableFiltersAsync(string entityType);

        // Autocomplete & Suggestions
        Task<List<string>> GetAutocompleteSuggestionsAsync(string query, string entityType = "all");
        Task<List<string>> GetSearchSuggestionsAsync(string query);
        Task<List<string>> GetPopularSearchesAsync(int count = 10);

        // Geo Search
        Task<List<Tour>> SearchToursByLocationAsync(decimal latitude, decimal longitude, decimal radiusKm);
        Task<List<Hotel>> SearchHotelsByLocationAsync(decimal latitude, decimal longitude, decimal radiusKm);
        Task<List<Destination>> SearchDestinationsByLocationAsync(decimal latitude, decimal longitude, decimal radiusKm);

        // Smart Search
        Task<Dictionary<string, object>> SmartSearchAsync(string query, int userId = 0);
        Task<List<Tour>> GetPersonalizedSearchResultsAsync(int userId, string query);

        // Search Analytics
        Task LogSearchAsync(string query, string entityType, int userId, int resultsCount);
        Task<Dictionary<string, object>> GetSearchAnalyticsAsync(int days = 30);
        Task<List<string>> GetTrendingSearchesAsync(int count = 10);

        // Similar Items
        Task<List<Tour>> GetSimilarToursAsync(int tourId, int count = 10);
        Task<List<Hotel>> GetSimilarHotelsAsync(int hotelId, int count = 10);

        // Search History
        Task<List<Dictionary<string, object>>> GetUserSearchHistoryAsync(int userId, int count = 20);
        Task ClearUserSearchHistoryAsync(int userId);
    }
}

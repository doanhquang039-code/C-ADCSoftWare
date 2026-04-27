using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface IMapService
    {
        Task<MapSearchResult> SearchLocationsAsync(MapSearchRequest request);
        Task<Location> GetLocationByIdAsync(int id);
        Task<List<Location>> GetLocationsByTypeAsync(string locationType);
        Task<Location> CreateLocationAsync(Location location);
        Task<Location> UpdateLocationAsync(Location location);
        Task DeleteLocationAsync(int id);
        
        // Geocoding
        Task<(double Latitude, double Longitude)> GeocodeAddressAsync(string address);
        Task<string> ReverseGeocodeAsync(double latitude, double longitude);
        
        // Distance and routing
        Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2);
        Task<RouteResponse> GetRouteAsync(RouteRequest request);
        
        // Tour/Hotel integration
        Task SyncTourLocationsAsync();
        Task SyncHotelLocationsAsync();
        Task<List<Location>> GetNearbyAttractionsAsync(double latitude, double longitude, double radiusKm = 5);
    }
}
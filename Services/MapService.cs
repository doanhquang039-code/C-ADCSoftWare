using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class MapService : IMapService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public MapService(ApplicationDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<MapSearchResult> SearchLocationsAsync(MapSearchRequest request)
        {
            var query = _context.Locations.Where(l => l.IsVisible);

            // Filter by location type
            if (!string.IsNullOrEmpty(request.LocationType))
            {
                query = query.Where(l => l.LocationType == request.LocationType);
            }

            // Filter by search term
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(l => l.Name.Contains(request.SearchTerm) || 
                                        l.Address.Contains(request.SearchTerm) ||
                                        l.Description.Contains(request.SearchTerm));
            }

            // Filter by radius if coordinates provided
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                var locations = await query.ToListAsync();
                locations = locations.Where(l => 
                    CalculateDistance(request.Latitude.Value, request.Longitude.Value, l.Latitude, l.Longitude) <= request.Radius)
                    .ToList();

                var totalCount = locations.Count;
                var pagedLocations = locations
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                return new MapSearchResult
                {
                    Locations = pagedLocations,
                    TotalCount = totalCount,
                    CenterLatitude = request.Latitude.Value,
                    CenterLongitude = request.Longitude.Value,
                    SearchRadius = request.Radius
                };
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new MapSearchResult
            {
                Locations = items,
                TotalCount = totalItems,
                CenterLatitude = 21.0285, // Hanoi default
                CenterLongitude = 105.8542,
                SearchRadius = request.Radius
            };
        }

        public async Task<Location> GetLocationByIdAsync(int id)
        {
            return await _context.Locations.FindAsync(id);
        }

        public async Task<List<Location>> GetLocationsByTypeAsync(string locationType)
        {
            return await _context.Locations
                .Where(l => l.LocationType == locationType && l.IsVisible)
                .OrderBy(l => l.Name)
                .ToListAsync();
        }

        public async Task<Location> CreateLocationAsync(Location location)
        {
            location.CreatedAt = DateTime.Now;
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task<Location> UpdateLocationAsync(Location location)
        {
            location.UpdatedAt = DateTime.Now;
            _context.Locations.Update(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task DeleteLocationAsync(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location != null)
            {
                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(double Latitude, double Longitude)> GeocodeAddressAsync(string address)
        {
            try
            {
                // Using OpenStreetMap Nominatim API (free alternative to Google Maps)
                var encodedAddress = Uri.EscapeDataString(address);
                var url = $"https://nominatim.openstreetmap.org/search?format=json&q={encodedAddress}&limit=1";
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "WEBDULICH/1.0");
                
                var response = await _httpClient.GetStringAsync(url);
                var results = JsonConvert.DeserializeObject<dynamic[]>(response);
                
                if (results != null && results.Length > 0)
                {
                    var lat = double.Parse(results[0].lat.ToString());
                    var lon = double.Parse(results[0].lon.ToString());
                    return (lat, lon);
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Geocoding error: {ex.Message}");
            }

            // Default to Hanoi if geocoding fails
            return (21.0285, 105.8542);
        }

        public async Task<string> ReverseGeocodeAsync(double latitude, double longitude)
        {
            try
            {
                var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}";
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "WEBDULICH/1.0");
                
                var response = await _httpClient.GetStringAsync(url);
                var result = JsonConvert.DeserializeObject<dynamic>(response);
                
                return result?.display_name?.ToString() ?? "Unknown location";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reverse geocoding error: {ex.Message}");
                return "Unknown location";
            }
        }

        public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2)
        {
            return CalculateDistance(lat1, lon1, lat2, lon2);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Haversine formula
            const double R = 6371; // Earth's radius in kilometers

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public async Task<RouteResponse> GetRouteAsync(RouteRequest request)
        {
            try
            {
                // Using OpenRouteService API (free alternative)
                var url = $"https://api.openrouteservice.org/v2/directions/{request.TravelMode}";
                var coordinates = new double[,] 
                { 
                    { request.StartLongitude, request.StartLatitude }, 
                    { request.EndLongitude, request.EndLatitude } 
                };

                var requestBody = new
                {
                    coordinates = coordinates,
                    format = "json",
                    instructions = true
                };

                // Note: You'll need to get a free API key from openrouteservice.org
                var apiKey = _configuration["OpenRouteService:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    // Return straight line distance as fallback
                    var fallbackDistance = CalculateDistance(request.StartLatitude, request.StartLongitude, 
                                                   request.EndLatitude, request.EndLongitude);
                    return new RouteResponse
                    {
                        DistanceKm = fallbackDistance,
                        DurationMinutes = (int)(fallbackDistance / 50 * 60), // Assume 50km/h average speed
                        Steps = new List<RouteStep>
                        {
                            new RouteStep
                            {
                                Instruction = "Head to destination",
                                DistanceKm = fallbackDistance,
                                DurationMinutes = (int)(fallbackDistance / 50 * 60),
                                StartLatitude = request.StartLatitude,
                                StartLongitude = request.StartLongitude,
                                EndLatitude = request.EndLatitude,
                                EndLongitude = request.EndLongitude
                            }
                        }
                    };
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", apiKey);

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Parse response and return RouteResponse
                // This is a simplified implementation
                var distance = CalculateDistance(request.StartLatitude, request.StartLongitude, 
                                               request.EndLatitude, request.EndLongitude);
                
                return new RouteResponse
                {
                    DistanceKm = distance,
                    DurationMinutes = (int)(distance / 50 * 60),
                    Steps = new List<RouteStep>()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Routing error: {ex.Message}");
                
                // Fallback to straight line
                var distance = CalculateDistance(request.StartLatitude, request.StartLongitude, 
                                               request.EndLatitude, request.EndLongitude);
                return new RouteResponse
                {
                    DistanceKm = distance,
                    DurationMinutes = (int)(distance / 50 * 60),
                    Steps = new List<RouteStep>()
                };
            }
        }

        public async Task SyncTourLocationsAsync()
        {
            var tours = await _context.Tours.Include(t => t.Destination).ToListAsync();
            
            foreach (var tour in tours)
            {
                var existingLocation = await _context.Locations
                    .FirstOrDefaultAsync(l => l.LocationType == "Tour" && l.RelatedId == tour.Id);

                if (existingLocation == null && tour.Destination != null)
                {
                    // Try to geocode destination name
                    var (lat, lon) = await GeocodeAddressAsync(tour.Destination.Name);
                    
                    var location = new Location
                    {
                        Name = tour.Name,
                        Address = tour.Destination.Name,
                        Latitude = lat,
                        Longitude = lon,
                        LocationType = "Tour",
                        RelatedId = tour.Id,
                        Description = tour.Description,
                        Image = tour.Image,
                        IsVisible = true
                    };

                    await CreateLocationAsync(location);
                }
            }
        }

        public async Task SyncHotelLocationsAsync()
        {
            var hotels = await _context.Hotels.ToListAsync();
            
            foreach (var hotel in hotels)
            {
                var existingLocation = await _context.Locations
                    .FirstOrDefaultAsync(l => l.LocationType == "Hotel" && l.RelatedId == hotel.Id);

                if (existingLocation == null && !string.IsNullOrEmpty(hotel.Address))
                {
                    var (lat, lon) = await GeocodeAddressAsync(hotel.Address);
                    
                    var location = new Location
                    {
                        Name = hotel.Name,
                        Address = hotel.Address,
                        Latitude = lat,
                        Longitude = lon,
                        LocationType = "Hotel",
                        RelatedId = hotel.Id,
                        Description = $"Hotel with {hotel.Rating} star rating",
                        Image = hotel.Image,
                        Rating = hotel.Rating,
                        IsVisible = true
                    };

                    await CreateLocationAsync(location);
                }
            }
        }

        public async Task<List<Location>> GetNearbyAttractionsAsync(double latitude, double longitude, double radiusKm = 5)
        {
            var allLocations = await _context.Locations
                .Where(l => l.IsVisible && (l.LocationType == "Attraction" || l.LocationType == "Tour"))
                .ToListAsync();

            return allLocations
                .Where(l => CalculateDistance(latitude, longitude, l.Latitude, l.Longitude) <= radiusKm)
                .OrderBy(l => CalculateDistance(latitude, longitude, l.Latitude, l.Longitude))
                .ToList();
        }
    }
}
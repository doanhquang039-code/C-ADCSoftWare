using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace WEBDULICH.Services.Weather
{
    /// <summary>
    /// Weather Service Implementation
    /// Uses OpenWeatherMap API (or similar)
    /// </summary>
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;
        private readonly ILogger<WeatherService> _logger;
        private readonly string _apiKey;
        private readonly string _apiBaseUrl;

        public WeatherService(
            HttpClient httpClient,
            IConfiguration configuration,
            IDistributedCache cache,
            ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
            _apiKey = configuration["Weather:ApiKey"] ?? "demo_key";
            _apiBaseUrl = configuration["Weather:ApiBaseUrl"] ?? "https://api.openweathermap.org/data/2.5";
        }

        public async Task<WeatherInfo> GetCurrentWeatherAsync(string location)
        {
            try
            {
                // Check cache first
                var cacheKey = $"weather:current:{location}";
                var cachedData = await _cache.GetStringAsync(cacheKey);
                
                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("Weather data retrieved from cache for {Location}", location);
                    return JsonSerializer.Deserialize<WeatherInfo>(cachedData) ?? new WeatherInfo();
                }

                // Call weather API
                var url = $"{_apiBaseUrl}/weather?q={location}&appid={_apiKey}&units=metric&lang=vi";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Weather API returned {StatusCode} for {Location}", response.StatusCode, location);
                    return GetDefaultWeather(location);
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiData = JsonSerializer.Deserialize<JsonElement>(content);

                var weatherInfo = new WeatherInfo
                {
                    Location = location,
                    Temperature = apiData.GetProperty("main").GetProperty("temp").GetDouble(),
                    FeelsLike = apiData.GetProperty("main").GetProperty("feels_like").GetDouble(),
                    Condition = apiData.GetProperty("weather")[0].GetProperty("main").GetString() ?? "",
                    Description = apiData.GetProperty("weather")[0].GetProperty("description").GetString() ?? "",
                    Humidity = apiData.GetProperty("main").GetProperty("humidity").GetInt32(),
                    WindSpeed = apiData.GetProperty("wind").GetProperty("speed").GetDouble(),
                    Pressure = apiData.GetProperty("main").GetProperty("pressure").GetDouble(),
                    Visibility = apiData.GetProperty("visibility").GetInt32(),
                    Icon = apiData.GetProperty("weather")[0].GetProperty("icon").GetString() ?? "",
                    UpdatedAt = DateTime.UtcNow
                };

                // Add sunrise/sunset if available
                if (apiData.TryGetProperty("sys", out var sys))
                {
                    if (sys.TryGetProperty("sunrise", out var sunrise))
                    {
                        weatherInfo.Sunrise = DateTimeOffset.FromUnixTimeSeconds(sunrise.GetInt64()).DateTime;
                    }
                    if (sys.TryGetProperty("sunset", out var sunset))
                    {
                        weatherInfo.Sunset = DateTimeOffset.FromUnixTimeSeconds(sunset.GetInt64()).DateTime;
                    }
                }

                // Cache for 30 minutes
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(weatherInfo), cacheOptions);

                _logger.LogInformation("Weather data fetched successfully for {Location}", location);
                return weatherInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather for {Location}", location);
                return GetDefaultWeather(location);
            }
        }

        public async Task<List<WeatherForecast>> GetWeatherForecastAsync(string location, int days = 7)
        {
            try
            {
                // Check cache
                var cacheKey = $"weather:forecast:{location}:{days}";
                var cachedData = await _cache.GetStringAsync(cacheKey);
                
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<List<WeatherForecast>>(cachedData) ?? new List<WeatherForecast>();
                }

                // Call forecast API
                var url = $"{_apiBaseUrl}/forecast?q={location}&appid={_apiKey}&units=metric&lang=vi&cnt={days * 8}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return GetDefaultForecast(days);
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiData = JsonSerializer.Deserialize<JsonElement>(content);

                var forecasts = new List<WeatherForecast>();
                var list = apiData.GetProperty("list");

                // Group by day and get daily summary
                var dailyData = new Dictionary<string, List<JsonElement>>();
                
                foreach (var item in list.EnumerateArray())
                {
                    var dt = DateTimeOffset.FromUnixTimeSeconds(item.GetProperty("dt").GetInt64()).DateTime;
                    var dateKey = dt.ToString("yyyy-MM-dd");
                    
                    if (!dailyData.ContainsKey(dateKey))
                    {
                        dailyData[dateKey] = new List<JsonElement>();
                    }
                    dailyData[dateKey].Add(item);
                }

                foreach (var day in dailyData.Take(days))
                {
                    var dayData = day.Value;
                    var temps = dayData.Select(d => d.GetProperty("main").GetProperty("temp").GetDouble()).ToList();
                    
                    var forecast = new WeatherForecast
                    {
                        Date = DateTime.Parse(day.Key),
                        MaxTemp = temps.Max(),
                        MinTemp = temps.Min(),
                        Condition = dayData.First().GetProperty("weather")[0].GetProperty("main").GetString() ?? "",
                        Description = dayData.First().GetProperty("weather")[0].GetProperty("description").GetString() ?? "",
                        ChanceOfRain = CalculateRainChance(dayData),
                        WindSpeed = dayData.Average(d => d.GetProperty("wind").GetProperty("speed").GetDouble()),
                        Humidity = (int)dayData.Average(d => d.GetProperty("main").GetProperty("humidity").GetInt32()),
                        Icon = dayData.First().GetProperty("weather")[0].GetProperty("icon").GetString() ?? ""
                    };
                    
                    forecasts.Add(forecast);
                }

                // Cache for 2 hours
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(forecasts), cacheOptions);

                return forecasts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching forecast for {Location}", location);
                return GetDefaultForecast(days);
            }
        }

        public async Task<WeatherInfo> GetWeatherByCoordinatesAsync(double latitude, double longitude)
        {
            try
            {
                var url = $"{_apiBaseUrl}/weather?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric&lang=vi";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return GetDefaultWeather($"{latitude},{longitude}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiData = JsonSerializer.Deserialize<JsonElement>(content);

                return new WeatherInfo
                {
                    Location = $"{latitude},{longitude}",
                    Temperature = apiData.GetProperty("main").GetProperty("temp").GetDouble(),
                    FeelsLike = apiData.GetProperty("main").GetProperty("feels_like").GetDouble(),
                    Condition = apiData.GetProperty("weather")[0].GetProperty("main").GetString() ?? "",
                    Description = apiData.GetProperty("weather")[0].GetProperty("description").GetString() ?? "",
                    Humidity = apiData.GetProperty("main").GetProperty("humidity").GetInt32(),
                    WindSpeed = apiData.GetProperty("wind").GetProperty("speed").GetDouble(),
                    Icon = apiData.GetProperty("weather")[0].GetProperty("icon").GetString() ?? "",
                    UpdatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather by coordinates");
                return GetDefaultWeather($"{latitude},{longitude}");
            }
        }

        public async Task<bool> IsSuitableForTravelAsync(string location)
        {
            var weather = await GetCurrentWeatherAsync(location);
            
            // Good travel conditions:
            // - Temperature between 15-30°C
            // - Not raining heavily
            // - Wind speed < 20 km/h
            // - Visibility > 5000m
            
            var isTempGood = weather.Temperature >= 15 && weather.Temperature <= 30;
            var isNotRaining = !weather.Condition.Contains("Rain") && !weather.Condition.Contains("Storm");
            var isWindGood = weather.WindSpeed < 20;
            var isVisibilityGood = weather.Visibility > 5000;

            return isTempGood && isNotRaining && isWindGood && isVisibilityGood;
        }

        public async Task<List<string>> GetBestMonthsToVisitAsync(string location)
        {
            // This would ideally use historical weather data
            // For now, return general recommendations based on location
            
            var bestMonths = new List<string>();
            
            // Vietnam locations
            if (location.Contains("Hà Nội") || location.Contains("Hanoi"))
            {
                bestMonths = new List<string> { "Tháng 3", "Tháng 4", "Tháng 10", "Tháng 11" };
            }
            else if (location.Contains("Sài Gòn") || location.Contains("Ho Chi Minh"))
            {
                bestMonths = new List<string> { "Tháng 12", "Tháng 1", "Tháng 2", "Tháng 3" };
            }
            else if (location.Contains("Đà Nẵng") || location.Contains("Da Nang"))
            {
                bestMonths = new List<string> { "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5" };
            }
            else if (location.Contains("Phú Quốc") || location.Contains("Phu Quoc"))
            {
                bestMonths = new List<string> { "Tháng 11", "Tháng 12", "Tháng 1", "Tháng 2", "Tháng 3" };
            }
            else if (location.Contains("Nha Trang"))
            {
                bestMonths = new List<string> { "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4" };
            }
            else
            {
                // Default
                bestMonths = new List<string> { "Tháng 3", "Tháng 4", "Tháng 10", "Tháng 11" };
            }

            return await Task.FromResult(bestMonths);
        }

        // Helper methods
        private WeatherInfo GetDefaultWeather(string location)
        {
            return new WeatherInfo
            {
                Location = location,
                Temperature = 25,
                FeelsLike = 26,
                Condition = "Clear",
                Description = "Trời quang đãng",
                Humidity = 70,
                WindSpeed = 10,
                Pressure = 1013,
                Visibility = 10000,
                Icon = "01d",
                UpdatedAt = DateTime.UtcNow
            };
        }

        private List<WeatherForecast> GetDefaultForecast(int days)
        {
            var forecasts = new List<WeatherForecast>();
            for (int i = 0; i < days; i++)
            {
                forecasts.Add(new WeatherForecast
                {
                    Date = DateTime.Today.AddDays(i),
                    MaxTemp = 28 + (i % 3),
                    MinTemp = 22 + (i % 2),
                    Condition = "Clear",
                    Description = "Trời quang đãng",
                    ChanceOfRain = 10,
                    WindSpeed = 10,
                    Humidity = 70,
                    Icon = "01d"
                });
            }
            return forecasts;
        }

        private int CalculateRainChance(List<JsonElement> dayData)
        {
            var rainCount = dayData.Count(d => 
            {
                var weather = d.GetProperty("weather")[0];
                var main = weather.GetProperty("main").GetString() ?? "";
                return main.Contains("Rain");
            });
            
            return (int)((double)rainCount / dayData.Count * 100);
        }
    }
}

using WEBDULICH.Models;

namespace WEBDULICH.Services.Weather
{
    /// <summary>
    /// Interface for Weather Service
    /// Provides weather information for destinations
    /// </summary>
    public interface IWeatherService
    {
        /// <summary>
        /// Get current weather for a location
        /// </summary>
        Task<WeatherInfo> GetCurrentWeatherAsync(string location);

        /// <summary>
        /// Get weather forecast for next 7 days
        /// </summary>
        Task<List<WeatherForecast>> GetWeatherForecastAsync(string location, int days = 7);

        /// <summary>
        /// Get weather by coordinates
        /// </summary>
        Task<WeatherInfo> GetWeatherByCoordinatesAsync(double latitude, double longitude);

        /// <summary>
        /// Check if weather is suitable for travel
        /// </summary>
        Task<bool> IsSuitableForTravelAsync(string location);

        /// <summary>
        /// Get best time to visit based on weather
        /// </summary>
        Task<List<string>> GetBestMonthsToVisitAsync(string location);
    }

    /// <summary>
    /// Weather Information Model
    /// </summary>
    public class WeatherInfo
    {
        public string Location { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public double Pressure { get; set; }
        public int Visibility { get; set; }
        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }
        public string Icon { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Weather Forecast Model
    /// </summary>
    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public double MaxTemp { get; set; }
        public double MinTemp { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ChanceOfRain { get; set; }
        public double WindSpeed { get; set; }
        public int Humidity { get; set; }
        public string Icon { get; set; } = string.Empty;
    }
}

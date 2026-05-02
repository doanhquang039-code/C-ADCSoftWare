using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Services.Weather;

namespace WEBDULICH.Controllers
{
    /// <summary>
    /// Weather Controller
    /// Provides weather information for travel destinations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly ILogger<WeatherController> _logger;

        public WeatherController(
            IWeatherService weatherService,
            ILogger<WeatherController> logger)
        {
            _weatherService = weatherService;
            _logger = logger;
        }

        /// <summary>
        /// Get current weather for a location
        /// </summary>
        [HttpGet("current/{location}")]
        public async Task<IActionResult> GetCurrentWeather(string location)
        {
            try
            {
                var weather = await _weatherService.GetCurrentWeatherAsync(location);
                return Ok(new
                {
                    success = true,
                    data = weather
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current weather");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin thời tiết" });
            }
        }

        /// <summary>
        /// Get weather forecast
        /// </summary>
        [HttpGet("forecast/{location}")]
        public async Task<IActionResult> GetWeatherForecast(string location, [FromQuery] int days = 7)
        {
            try
            {
                var forecast = await _weatherService.GetWeatherForecastAsync(location, days);
                return Ok(new
                {
                    success = true,
                    data = forecast
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weather forecast");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy dự báo thời tiết" });
            }
        }

        /// <summary>
        /// Get weather by coordinates
        /// </summary>
        [HttpGet("coordinates")]
        public async Task<IActionResult> GetWeatherByCoordinates(
            [FromQuery] double latitude, 
            [FromQuery] double longitude)
        {
            try
            {
                var weather = await _weatherService.GetWeatherByCoordinatesAsync(latitude, longitude);
                return Ok(new
                {
                    success = true,
                    data = weather
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weather by coordinates");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin thời tiết" });
            }
        }

        /// <summary>
        /// Check if weather is suitable for travel
        /// </summary>
        [HttpGet("suitable/{location}")]
        public async Task<IActionResult> IsSuitableForTravel(string location)
        {
            try
            {
                var isSuitable = await _weatherService.IsSuitableForTravelAsync(location);
                var weather = await _weatherService.GetCurrentWeatherAsync(location);
                
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        isSuitable,
                        weather,
                        recommendation = isSuitable 
                            ? "Thời tiết thuận lợi cho du lịch" 
                            : "Nên cân nhắc thời tiết trước khi đi"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking travel suitability");
                return StatusCode(500, new { success = false, message = "Lỗi khi kiểm tra thời tiết" });
            }
        }

        /// <summary>
        /// Get best months to visit
        /// </summary>
        [HttpGet("best-months/{location}")]
        public async Task<IActionResult> GetBestMonthsToVisit(string location)
        {
            try
            {
                var bestMonths = await _weatherService.GetBestMonthsToVisitAsync(location);
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        location,
                        bestMonths,
                        message = $"Thời điểm tốt nhất để đến {location}"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting best months");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin" });
            }
        }
    }
}

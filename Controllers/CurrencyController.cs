using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Services.Currency;

namespace WEBDULICH.Controllers
{
    /// <summary>
    /// Currency Controller
    /// Provides currency conversion services
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(
            ICurrencyService currencyService,
            ILogger<CurrencyController> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        /// <summary>
        /// Convert currency
        /// </summary>
        [HttpGet("convert")]
        public async Task<IActionResult> Convert(
            [FromQuery] decimal amount,
            [FromQuery] string from,
            [FromQuery] string to)
        {
            try
            {
                var convertedAmount = await _currencyService.ConvertAsync(amount, from, to);
                var rate = await _currencyService.GetExchangeRateAsync(from, to);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        originalAmount = amount,
                        fromCurrency = from,
                        convertedAmount,
                        toCurrency = to,
                        exchangeRate = rate,
                        formattedOriginal = _currencyService.FormatCurrency(amount, from),
                        formattedConverted = _currencyService.FormatCurrency(convertedAmount, to),
                        timestamp = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting currency");
                return StatusCode(500, new { success = false, message = "Lỗi khi chuyển đổi tiền tệ" });
            }
        }

        /// <summary>
        /// Get exchange rate
        /// </summary>
        [HttpGet("rate")]
        public async Task<IActionResult> GetExchangeRate(
            [FromQuery] string from,
            [FromQuery] string to)
        {
            try
            {
                var rate = await _currencyService.GetExchangeRateAsync(from, to);
                
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        fromCurrency = from,
                        toCurrency = to,
                        rate,
                        timestamp = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exchange rate");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy tỷ giá" });
            }
        }

        /// <summary>
        /// Get all supported currencies
        /// </summary>
        [HttpGet("currencies")]
        public async Task<IActionResult> GetSupportedCurrencies()
        {
            try
            {
                var currencies = await _currencyService.GetSupportedCurrenciesAsync();
                
                return Ok(new
                {
                    success = true,
                    data = currencies
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supported currencies");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách tiền tệ" });
            }
        }

        /// <summary>
        /// Get latest rates for a base currency
        /// </summary>
        [HttpGet("rates/{baseCurrency}")]
        public async Task<IActionResult> GetLatestRates(string baseCurrency = "VND")
        {
            try
            {
                var rates = await _currencyService.GetLatestRatesAsync(baseCurrency);
                
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        baseCurrency,
                        rates,
                        timestamp = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest rates");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy tỷ giá" });
            }
        }

        /// <summary>
        /// Format currency with symbol
        /// </summary>
        [HttpGet("format")]
        public IActionResult FormatCurrency(
            [FromQuery] decimal amount,
            [FromQuery] string currency)
        {
            try
            {
                var formatted = _currencyService.FormatCurrency(amount, currency);
                
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        amount,
                        currency,
                        formatted
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting currency");
                return StatusCode(500, new { success = false, message = "Lỗi khi định dạng tiền tệ" });
            }
        }
    }
}

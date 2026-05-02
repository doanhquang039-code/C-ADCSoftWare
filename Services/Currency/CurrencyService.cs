using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace WEBDULICH.Services.Currency
{
    /// <summary>
    /// Currency Conversion Service Implementation
    /// Uses Exchange Rate API or similar service
    /// </summary>
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;
        private readonly ILogger<CurrencyService> _logger;
        private readonly string _apiKey;
        private readonly string _apiBaseUrl;

        // Common currencies in Vietnam tourism
        private readonly Dictionary<string, string> _supportedCurrencies = new()
        {
            { "VND", "Vietnamese Dong" },
            { "USD", "US Dollar" },
            { "EUR", "Euro" },
            { "GBP", "British Pound" },
            { "JPY", "Japanese Yen" },
            { "CNY", "Chinese Yuan" },
            { "KRW", "South Korean Won" },
            { "THB", "Thai Baht" },
            { "SGD", "Singapore Dollar" },
            { "AUD", "Australian Dollar" },
            { "CAD", "Canadian Dollar" },
            { "HKD", "Hong Kong Dollar" }
        };

        private readonly Dictionary<string, string> _currencySymbols = new()
        {
            { "VND", "₫" },
            { "USD", "$" },
            { "EUR", "€" },
            { "GBP", "£" },
            { "JPY", "¥" },
            { "CNY", "¥" },
            { "KRW", "₩" },
            { "THB", "฿" },
            { "SGD", "S$" },
            { "AUD", "A$" },
            { "CAD", "C$" },
            { "HKD", "HK$" }
        };

        public CurrencyService(
            HttpClient httpClient,
            IConfiguration configuration,
            IDistributedCache cache,
            ILogger<CurrencyService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
            _apiKey = configuration["Currency:ApiKey"] ?? "demo_key";
            _apiBaseUrl = configuration["Currency:ApiBaseUrl"] ?? "https://api.exchangerate-api.com/v4/latest";
        }

        public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            try
            {
                if (fromCurrency == toCurrency)
                {
                    return amount;
                }

                var rate = await GetExchangeRateAsync(fromCurrency, toCurrency);
                var convertedAmount = amount * rate;

                _logger.LogInformation(
                    "Converted {Amount} {From} to {Result} {To} at rate {Rate}",
                    amount, fromCurrency, convertedAmount, toCurrency, rate);

                return Math.Round(convertedAmount, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting currency from {From} to {To}", fromCurrency, toCurrency);
                return amount; // Return original amount on error
            }
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            try
            {
                // Check cache first
                var cacheKey = $"currency:rate:{fromCurrency}:{toCurrency}";
                var cachedRate = await _cache.GetStringAsync(cacheKey);
                
                if (!string.IsNullOrEmpty(cachedRate))
                {
                    return decimal.Parse(cachedRate);
                }

                // Get all rates for base currency
                var rates = await GetLatestRatesAsync(fromCurrency);
                
                if (rates.ContainsKey(toCurrency))
                {
                    var rate = rates[toCurrency];
                    
                    // Cache for 1 hour
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                    };
                    await _cache.SetStringAsync(cacheKey, rate.ToString(), cacheOptions);
                    
                    return rate;
                }

                // Fallback to default rates
                return GetDefaultRate(fromCurrency, toCurrency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exchange rate");
                return GetDefaultRate(fromCurrency, toCurrency);
            }
        }

        public async Task<Dictionary<string, string>> GetSupportedCurrenciesAsync()
        {
            return await Task.FromResult(_supportedCurrencies);
        }

        public async Task<Dictionary<string, decimal>> GetLatestRatesAsync(string baseCurrency = "VND")
        {
            try
            {
                // Check cache
                var cacheKey = $"currency:rates:{baseCurrency}";
                var cachedData = await _cache.GetStringAsync(cacheKey);
                
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<Dictionary<string, decimal>>(cachedData) 
                        ?? new Dictionary<string, decimal>();
                }

                // Call API
                var url = $"{_apiBaseUrl}/{baseCurrency}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return GetDefaultRates(baseCurrency);
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiData = JsonSerializer.Deserialize<JsonElement>(content);
                
                var rates = new Dictionary<string, decimal>();
                var ratesElement = apiData.GetProperty("rates");

                foreach (var currency in _supportedCurrencies.Keys)
                {
                    if (ratesElement.TryGetProperty(currency, out var rateValue))
                    {
                        rates[currency] = rateValue.GetDecimal();
                    }
                }

                // Cache for 1 hour
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(rates), cacheOptions);

                _logger.LogInformation("Exchange rates fetched successfully for {BaseCurrency}", baseCurrency);
                return rates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rates for {BaseCurrency}", baseCurrency);
                return GetDefaultRates(baseCurrency);
            }
        }

        public string FormatCurrency(decimal amount, string currencyCode)
        {
            var symbol = _currencySymbols.ContainsKey(currencyCode) 
                ? _currencySymbols[currencyCode] 
                : currencyCode;

            if (currencyCode == "VND")
            {
                // Vietnamese Dong - no decimals
                return $"{amount:N0} {symbol}";
            }
            else if (currencyCode == "JPY" || currencyCode == "KRW")
            {
                // Japanese Yen and Korean Won - no decimals
                return $"{symbol}{amount:N0}";
            }
            else
            {
                // Other currencies - 2 decimals
                return $"{symbol}{amount:N2}";
            }
        }

        // Helper methods
        private decimal GetDefaultRate(string fromCurrency, string toCurrency)
        {
            // Default exchange rates (approximate, for fallback only)
            var defaultRates = new Dictionary<string, Dictionary<string, decimal>>
            {
                ["VND"] = new Dictionary<string, decimal>
                {
                    ["USD"] = 0.000041m,
                    ["EUR"] = 0.000038m,
                    ["GBP"] = 0.000033m,
                    ["JPY"] = 0.0056m,
                    ["CNY"] = 0.00029m,
                    ["KRW"] = 0.053m,
                    ["THB"] = 0.0014m,
                    ["SGD"] = 0.000055m
                },
                ["USD"] = new Dictionary<string, decimal>
                {
                    ["VND"] = 24000m,
                    ["EUR"] = 0.92m,
                    ["GBP"] = 0.79m,
                    ["JPY"] = 135m,
                    ["CNY"] = 7.1m,
                    ["KRW"] = 1300m,
                    ["THB"] = 34m,
                    ["SGD"] = 1.35m
                }
            };

            if (fromCurrency == toCurrency)
                return 1m;

            if (defaultRates.ContainsKey(fromCurrency) && 
                defaultRates[fromCurrency].ContainsKey(toCurrency))
            {
                return defaultRates[fromCurrency][toCurrency];
            }

            // If no default rate, try inverse
            if (defaultRates.ContainsKey(toCurrency) && 
                defaultRates[toCurrency].ContainsKey(fromCurrency))
            {
                return 1m / defaultRates[toCurrency][fromCurrency];
            }

            return 1m; // Last resort
        }

        private Dictionary<string, decimal> GetDefaultRates(string baseCurrency)
        {
            var rates = new Dictionary<string, decimal>();
            
            foreach (var currency in _supportedCurrencies.Keys)
            {
                if (currency != baseCurrency)
                {
                    rates[currency] = GetDefaultRate(baseCurrency, currency);
                }
            }
            
            return rates;
        }
    }
}

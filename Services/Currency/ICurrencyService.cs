namespace WEBDULICH.Services.Currency
{
    /// <summary>
    /// Interface for Currency Conversion Service
    /// </summary>
    public interface ICurrencyService
    {
        /// <summary>
        /// Convert amount from one currency to another
        /// </summary>
        Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);

        /// <summary>
        /// Get current exchange rate
        /// </summary>
        Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);

        /// <summary>
        /// Get all supported currencies
        /// </summary>
        Task<Dictionary<string, string>> GetSupportedCurrenciesAsync();

        /// <summary>
        /// Get latest exchange rates for a base currency
        /// </summary>
        Task<Dictionary<string, decimal>> GetLatestRatesAsync(string baseCurrency = "VND");

        /// <summary>
        /// Format currency with symbol
        /// </summary>
        string FormatCurrency(decimal amount, string currencyCode);
    }

    /// <summary>
    /// Currency Information Model
    /// </summary>
    public class CurrencyInfo
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Conversion Result Model
    /// </summary>
    public class ConversionResult
    {
        public decimal OriginalAmount { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public decimal ConvertedAmount { get; set; }
        public string ToCurrency { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public DateTime ConversionDate { get; set; }
    }
}

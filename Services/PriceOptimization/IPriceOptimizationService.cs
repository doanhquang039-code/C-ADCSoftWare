using WEBDULICH.Models;

namespace WEBDULICH.Services.PriceOptimization
{
    public interface IPriceOptimizationService
    {
        // Price History
        Task<PriceHistory> RecordPriceChangeAsync(string entityType, int entityId, decimal oldPrice, decimal newPrice, string reason);
        Task<List<PriceHistory>> GetPriceHistoryAsync(string entityType, int entityId);
        Task<List<PriceHistory>> GetRecentPriceChangesAsync(int days = 30);

        // Dynamic Pricing Rules
        Task<DynamicPricingRule> CreateRuleAsync(DynamicPricingRule rule);
        Task<DynamicPricingRule> GetRuleByIdAsync(int id);
        Task<List<DynamicPricingRule>> GetAllRulesAsync(bool activeOnly = true);
        Task<DynamicPricingRule> UpdateRuleAsync(DynamicPricingRule rule);
        Task<bool> DeleteRuleAsync(int id);

        // Price Optimization
        Task<decimal> CalculateOptimalPriceAsync(string entityType, int entityId);
        Task<decimal> ApplyDynamicPricingAsync(string entityType, int entityId, decimal basePrice);
        Task<Dictionary<string, decimal>> GetPriceSuggestionsAsync(string entityType, int entityId);

        // Demand Analysis
        Task<string> AnalyzeDemandLevelAsync(string entityType, int entityId);
        Task<decimal> CalculateOccupancyRateAsync(string entityType, int entityId, DateTime startDate, DateTime endDate);
        Task<Dictionary<string, object>> GetDemandForecastAsync(string entityType, int entityId, int days = 30);

        // Season & Trends
        Task<string> GetCurrentSeasonAsync();
        Task<Dictionary<string, object>> GetSeasonalTrendsAsync(string entityType, int entityId);
        Task<Dictionary<string, object>> GetPricingTrendsAsync(string entityType, int entityId, int months = 6);

        // Competitor Analysis
        Task<Dictionary<string, object>> GetCompetitorPricingAsync(string entityType, int entityId);
        Task<decimal> GetMarketAveragePriceAsync(string entityType);

        // Statistics & Reports
        Task<Dictionary<string, object>> GetPriceOptimizationReportAsync(string entityType, int entityId);
        Task<Dictionary<string, object>> GetOverallPricingStatisticsAsync();
        Task<List<Dictionary<string, object>>> GetPricePerformanceAsync(int days = 30);
    }
}

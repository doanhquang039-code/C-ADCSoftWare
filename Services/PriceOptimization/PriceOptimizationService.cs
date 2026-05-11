using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using System.Text.Json;

namespace WEBDULICH.Services.PriceOptimization
{
    public class PriceOptimizationService : IPriceOptimizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PriceOptimizationService> _logger;

        public PriceOptimizationService(
            ApplicationDbContext context,
            ILogger<PriceOptimizationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PriceHistory> RecordPriceChangeAsync(string entityType, int entityId, decimal oldPrice, decimal newPrice, string reason)
        {
            var changePercentage = oldPrice > 0 ? ((newPrice - oldPrice) / oldPrice) * 100 : 0;

            var history = new PriceHistory
            {
                EntityType = entityType,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                ChangePercentage = changePercentage,
                ChangeReason = reason,
                EffectiveDate = DateTime.Now,
                Season = GetCurrentSeasonAsync().Result,
                DemandLevel = await AnalyzeDemandLevelAsync(entityType, entityId),
                IsAutomatic = reason.Contains("Automatic") || reason.Contains("Dynamic"),
                CreatedAt = DateTime.Now
            };

            if (entityType == "Tour")
                history.TourId = entityId;
            else if (entityType == "Hotel")
                history.HotelId = entityId;

            // Calculate occupancy and bookings
            var startDate = DateTime.Now.AddDays(-7);
            var endDate = DateTime.Now.AddDays(7);
            history.OccupancyRate = await CalculateOccupancyRateAsync(entityType, entityId, startDate, endDate);
            history.BookingsLast7Days = await GetBookingCountAsync(entityType, entityId, DateTime.Now.AddDays(-7), DateTime.Now);
            history.BookingsNext7Days = await GetBookingCountAsync(entityType, entityId, DateTime.Now, DateTime.Now.AddDays(7));

            _context.PriceHistories.Add(history);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Recorded price change for {entityType} {entityId}: {oldPrice} -> {newPrice}");
            return history;
        }

        public async Task<List<PriceHistory>> GetPriceHistoryAsync(string entityType, int entityId)
        {
            var query = _context.PriceHistories.AsQueryable();

            if (entityType == "Tour")
                query = query.Where(h => h.TourId == entityId);
            else if (entityType == "Hotel")
                query = query.Where(h => h.HotelId == entityId);

            return await query
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PriceHistory>> GetRecentPriceChangesAsync(int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            return await _context.PriceHistories
                .Where(h => h.CreatedAt >= cutoffDate)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<DynamicPricingRule> CreateRuleAsync(DynamicPricingRule rule)
        {
            rule.CreatedAt = DateTime.Now;
            _context.DynamicPricingRules.Add(rule);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Created dynamic pricing rule: {rule.Name}");
            return rule;
        }

        public async Task<DynamicPricingRule> GetRuleByIdAsync(int id)
        {
            return await _context.DynamicPricingRules.FindAsync(id);
        }

        public async Task<List<DynamicPricingRule>> GetAllRulesAsync(bool activeOnly = true)
        {
            var query = _context.DynamicPricingRules.AsQueryable();

            if (activeOnly)
                query = query.Where(r => r.IsActive);

            return await query
                .OrderByDescending(r => r.Priority)
                .ToListAsync();
        }

        public async Task<DynamicPricingRule> UpdateRuleAsync(DynamicPricingRule rule)
        {
            rule.UpdatedAt = DateTime.Now;
            _context.DynamicPricingRules.Update(rule);
            await _context.SaveChangesAsync();
            return rule;
        }

        public async Task<bool> DeleteRuleAsync(int id)
        {
            var rule = await _context.DynamicPricingRules.FindAsync(id);
            if (rule == null) return false;

            rule.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> CalculateOptimalPriceAsync(string entityType, int entityId)
        {
            decimal basePrice = await GetBasePriceAsync(entityType, entityId);
            if (basePrice == 0) return 0;

            // Get demand level
            var demandLevel = await AnalyzeDemandLevelAsync(entityType, entityId);
            var season = await GetCurrentSeasonAsync();

            decimal multiplier = 1.0m;

            // Demand adjustment
            if (demandLevel == "High") multiplier += 0.20m;
            else if (demandLevel == "Medium") multiplier += 0.10m;
            else if (demandLevel == "Low") multiplier -= 0.10m;

            // Season adjustment
            if (season == "Peak") multiplier += 0.15m;
            else if (season == "High") multiplier += 0.10m;
            else if (season == "Low") multiplier -= 0.15m;

            // Occupancy adjustment
            var occupancy = await CalculateOccupancyRateAsync(entityType, entityId, DateTime.Now, DateTime.Now.AddDays(30));
            if (occupancy > 80) multiplier += 0.10m;
            else if (occupancy < 30) multiplier -= 0.15m;

            decimal optimalPrice = basePrice * multiplier;

            // Apply min/max constraints
            decimal minPrice = basePrice * 0.7m; // Max 30% discount
            decimal maxPrice = basePrice * 1.5m; // Max 50% increase

            optimalPrice = Math.Max(minPrice, Math.Min(maxPrice, optimalPrice));

            return Math.Round(optimalPrice, 0);
        }

        public async Task<decimal> ApplyDynamicPricingAsync(string entityType, int entityId, decimal basePrice)
        {
            var rules = await GetAllRulesAsync(true);
            decimal finalPrice = basePrice;

            foreach (var rule in rules)
            {
                // Check if rule applies
                if (!RuleApplies(rule, entityType, entityId))
                    continue;

                // Check date validity
                if (rule.ValidFrom.HasValue && DateTime.Now < rule.ValidFrom.Value)
                    continue;
                if (rule.ValidTo.HasValue && DateTime.Now > rule.ValidTo.Value)
                    continue;

                // Apply rule
                if (rule.ActionType == "Increase")
                {
                    finalPrice += finalPrice * (rule.ActionValue / 100);
                }
                else if (rule.ActionType == "Decrease")
                {
                    finalPrice -= finalPrice * (rule.ActionValue / 100);
                }
                else if (rule.ActionType == "SetFixed")
                {
                    finalPrice = rule.ActionValue;
                }

                // Apply min/max limits
                if (rule.MinPrice.HasValue)
                    finalPrice = Math.Max(finalPrice, rule.MinPrice.Value);
                if (rule.MaxPrice.HasValue)
                    finalPrice = Math.Min(finalPrice, rule.MaxPrice.Value);
            }

            return Math.Round(finalPrice, 0);
        }

        private bool RuleApplies(DynamicPricingRule rule, string entityType, int entityId)
        {
            if (rule.AppliesTo == "All") return true;
            if (rule.AppliesTo != entityType) return false;

            if (string.IsNullOrEmpty(rule.EntityIds)) return true;

            try
            {
                var ids = JsonSerializer.Deserialize<List<int>>(rule.EntityIds);
                return ids?.Contains(entityId) ?? false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Dictionary<string, decimal>> GetPriceSuggestionsAsync(string entityType, int entityId)
        {
            decimal basePrice = await GetBasePriceAsync(entityType, entityId);
            decimal optimalPrice = await CalculateOptimalPriceAsync(entityType, entityId);
            decimal dynamicPrice = await ApplyDynamicPricingAsync(entityType, entityId, basePrice);
            decimal marketAverage = await GetMarketAveragePriceAsync(entityType);

            return new Dictionary<string, decimal>
            {
                ["currentPrice"] = basePrice,
                ["optimalPrice"] = optimalPrice,
                ["dynamicPrice"] = dynamicPrice,
                ["marketAverage"] = marketAverage,
                ["recommendedPrice"] = Math.Round((optimalPrice + dynamicPrice) / 2, 0)
            };
        }

        public async Task<string> AnalyzeDemandLevelAsync(string entityType, int entityId)
        {
            var bookings = await GetBookingCountAsync(entityType, entityId, DateTime.Now.AddDays(-30), DateTime.Now);
            var views = await GetViewCountAsync(entityType, entityId);

            // Simple demand analysis
            if (bookings > 20 || views > 500) return "High";
            if (bookings > 10 || views > 200) return "Medium";
            return "Low";
        }

        public async Task<decimal> CalculateOccupancyRateAsync(string entityType, int entityId, DateTime startDate, DateTime endDate)
        {
            var totalDays = (endDate - startDate).Days;
            if (totalDays <= 0) return 0;

            var bookings = await GetBookingCountAsync(entityType, entityId, startDate, endDate);
            var capacity = await GetCapacityAsync(entityType, entityId);

            if (capacity == 0) return 0;

            return Math.Round((decimal)bookings / (capacity * totalDays) * 100, 2);
        }

        public async Task<Dictionary<string, object>> GetDemandForecastAsync(string entityType, int entityId, int days = 30)
        {
            var historicalBookings = await GetBookingCountAsync(entityType, entityId, DateTime.Now.AddDays(-days), DateTime.Now);
            var upcomingBookings = await GetBookingCountAsync(entityType, entityId, DateTime.Now, DateTime.Now.AddDays(days));

            var trend = upcomingBookings > historicalBookings ? "Increasing" : "Decreasing";
            var changePercentage = historicalBookings > 0 
                ? ((decimal)(upcomingBookings - historicalBookings) / historicalBookings) * 100 
                : 0;

            return new Dictionary<string, object>
            {
                ["historicalBookings"] = historicalBookings,
                ["upcomingBookings"] = upcomingBookings,
                ["trend"] = trend,
                ["changePercentage"] = Math.Round(changePercentage, 2),
                ["forecastDays"] = days
            };
        }

        public async Task<string> GetCurrentSeasonAsync()
        {
            var month = DateTime.Now.Month;

            // Vietnam tourism seasons
            if (month >= 12 || month <= 2) return "Peak"; // Winter holidays
            if (month >= 6 && month <= 8) return "High"; // Summer
            if (month >= 4 && month <= 5) return "High"; // Spring
            return "Low";
        }

        public async Task<Dictionary<string, object>> GetSeasonalTrendsAsync(string entityType, int entityId)
        {
            var history = await GetPriceHistoryAsync(entityType, entityId);

            var seasonalData = history
                .GroupBy(h => h.Season)
                .Select(g => new
                {
                    Season = g.Key,
                    AveragePrice = g.Average(h => h.NewPrice),
                    BookingCount = g.Sum(h => h.BookingsNext7Days),
                    OccupancyRate = g.Average(h => h.OccupancyRate)
                })
                .ToDictionary(x => x.Season, x => (object)new
                {
                    x.AveragePrice,
                    x.BookingCount,
                    x.OccupancyRate
                });

            return seasonalData;
        }

        public async Task<Dictionary<string, object>> GetPricingTrendsAsync(string entityType, int entityId, int months = 6)
        {
            var cutoffDate = DateTime.Now.AddMonths(-months);
            var history = await _context.PriceHistories
                .Where(h => h.CreatedAt >= cutoffDate)
                .ToListAsync();

            if (entityType == "Tour")
                history = history.Where(h => h.TourId == entityId).ToList();
            else if (entityType == "Hotel")
                history = history.Where(h => h.HotelId == entityId).ToList();

            var monthlyTrends = history
                .GroupBy(h => new { h.CreatedAt.Year, h.CreatedAt.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    AveragePrice = g.Average(h => h.NewPrice),
                    PriceChanges = g.Count(),
                    AverageChange = g.Average(h => h.ChangePercentage)
                })
                .OrderBy(x => x.Month)
                .ToList();

            return new Dictionary<string, object>
            {
                ["trends"] = monthlyTrends,
                ["totalChanges"] = history.Count,
                ["averagePrice"] = history.Any() ? history.Average(h => h.NewPrice) : 0
            };
        }

        public async Task<Dictionary<string, object>> GetCompetitorPricingAsync(string entityType, int entityId)
        {
            // Simplified competitor analysis
            var marketAverage = await GetMarketAveragePriceAsync(entityType);
            var currentPrice = await GetBasePriceAsync(entityType, entityId);

            var difference = currentPrice - marketAverage;
            var differencePercentage = marketAverage > 0 ? (difference / marketAverage) * 100 : 0;

            return new Dictionary<string, object>
            {
                ["currentPrice"] = currentPrice,
                ["marketAverage"] = marketAverage,
                ["difference"] = Math.Round(difference, 0),
                ["differencePercentage"] = Math.Round(differencePercentage, 2),
                ["position"] = differencePercentage > 10 ? "Above Market" : differencePercentage < -10 ? "Below Market" : "At Market"
            };
        }

        public async Task<decimal> GetMarketAveragePriceAsync(string entityType)
        {
            if (entityType == "Tour")
            {
                var tours = await _context.Tours.Where(t => t.Price > 0).ToListAsync();
                return tours.Any() ? tours.Average(t => t.Price) : 0;
            }
            else if (entityType == "Hotel")
            {
                var hotels = await _context.Hotels.Where(h => h.PricePerNight > 0).ToListAsync();
                return hotels.Any() ? hotels.Average(h => h.PricePerNight) : 0;
            }

            return 0;
        }

        public async Task<Dictionary<string, object>> GetPriceOptimizationReportAsync(string entityType, int entityId)
        {
            var currentPrice = await GetBasePriceAsync(entityType, entityId);
            var optimalPrice = await CalculateOptimalPriceAsync(entityType, entityId);
            var suggestions = await GetPriceSuggestionsAsync(entityType, entityId);
            var demandLevel = await AnalyzeDemandLevelAsync(entityType, entityId);
            var season = await GetCurrentSeasonAsync();
            var occupancy = await CalculateOccupancyRateAsync(entityType, entityId, DateTime.Now, DateTime.Now.AddDays(30));

            var potentialRevenue = (optimalPrice - currentPrice) * await GetBookingCountAsync(entityType, entityId, DateTime.Now, DateTime.Now.AddDays(30));

            return new Dictionary<string, object>
            {
                ["currentPrice"] = currentPrice,
                ["optimalPrice"] = optimalPrice,
                ["suggestions"] = suggestions,
                ["demandLevel"] = demandLevel,
                ["season"] = season,
                ["occupancyRate"] = occupancy,
                ["potentialRevenueIncrease"] = Math.Round(potentialRevenue, 0),
                ["recommendation"] = optimalPrice > currentPrice ? "Increase Price" : "Decrease Price"
            };
        }

        public async Task<Dictionary<string, object>> GetOverallPricingStatisticsAsync()
        {
            var allHistory = await _context.PriceHistories.ToListAsync();
            var rules = await _context.DynamicPricingRules.ToListAsync();

            return new Dictionary<string, object>
            {
                ["totalPriceChanges"] = allHistory.Count,
                ["automaticChanges"] = allHistory.Count(h => h.IsAutomatic),
                ["manualChanges"] = allHistory.Count(h => !h.IsAutomatic),
                ["totalRules"] = rules.Count,
                ["activeRules"] = rules.Count(r => r.IsActive),
                ["averagePriceChange"] = allHistory.Any() ? Math.Round(allHistory.Average(h => h.ChangePercentage), 2) : 0,
                ["last30Days"] = allHistory.Count(h => h.CreatedAt >= DateTime.Now.AddDays(-30))
            };
        }

        public async Task<List<Dictionary<string, object>>> GetPricePerformanceAsync(int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            var history = await _context.PriceHistories
                .Where(h => h.CreatedAt >= cutoffDate)
                .ToListAsync();

            var performance = history
                .GroupBy(h => new { h.EntityType, EntityId = h.TourId ?? h.HotelId ?? 0 })
                .Select(g => new Dictionary<string, object>
                {
                    ["entityType"] = g.Key.EntityType,
                    ["entityId"] = g.Key.EntityId,
                    ["priceChanges"] = g.Count(),
                    ["averageChange"] = Math.Round(g.Average(h => h.ChangePercentage), 2),
                    ["totalBookings"] = g.Sum(h => h.BookingsNext7Days),
                    ["averageOccupancy"] = Math.Round(g.Average(h => h.OccupancyRate), 2)
                })
                .OrderByDescending(x => (int)x["priceChanges"])
                .ToList();

            return performance;
        }

        // Helper methods
        private async Task<decimal> GetBasePriceAsync(string entityType, int entityId)
        {
            if (entityType == "Tour")
            {
                var tour = await _context.Tours.FindAsync(entityId);
                return tour?.Price ?? 0;
            }
            else if (entityType == "Hotel")
            {
                var hotel = await _context.Hotels.FindAsync(entityId);
                return hotel?.PricePerNight ?? 0;
            }
            return 0;
        }

        private async Task<int> GetBookingCountAsync(string entityType, int entityId, DateTime startDate, DateTime endDate)
        {
            if (entityType == "Tour")
            {
                return await _context.Bookings
                    .Where(b => b.TourId == entityId && b.CreatedAt >= startDate && b.CreatedAt <= endDate)
                    .CountAsync();
            }
            else if (entityType == "Hotel")
            {
                return await _context.Bookings
                    .Where(b => b.HotelId == entityId && b.CreatedAt >= startDate && b.CreatedAt <= endDate)
                    .CountAsync();
            }
            return 0;
        }

        private async Task<int> GetViewCountAsync(string entityType, int entityId)
        {
            // Simplified - would need view tracking table in real implementation
            return 100; // Placeholder
        }

        private async Task<int> GetCapacityAsync(string entityType, int entityId)
        {
            if (entityType == "Tour")
            {
                var tour = await _context.Tours.FindAsync(entityId);
                return tour?.MaxGroupSize ?? 10;
            }
            else if (entityType == "Hotel")
            {
                // Assume average hotel has 50 rooms
                return 50;
            }
            return 10;
        }
    }
}

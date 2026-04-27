using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface ILoyaltyService
    {
        // Account management
        Task<LoyaltyAccount> GetOrCreateAccountAsync(int userId);
        Task<LoyaltyAccount> GetAccountByUserIdAsync(int userId);
        Task<List<PointTransaction>> GetTransactionHistoryAsync(int userId, int page = 1, int pageSize = 20);
        
        // Points management
        Task<bool> AwardPointsAsync(int userId, int points, string source, string description, int? relatedId = null, string relatedType = null);
        Task<bool> SpendPointsAsync(int userId, int points, string description, int? relatedId = null, string relatedType = null);
        Task<bool> AdjustPointsAsync(int userId, int points, string reason);
        Task ProcessPointsExpiryAsync();
        
        // Tier management
        Task UpdateUserTierAsync(int userId);
        Task<List<LoyaltyTier>> GetTiersAsync();
        Task<LoyaltyTier> CreateTierAsync(LoyaltyTier tier);
        Task<LoyaltyTier> UpdateTierAsync(LoyaltyTier tier);
        
        // Rewards management
        Task<List<Reward>> GetAvailableRewardsAsync(int userId);
        Task<List<Reward>> GetAllRewardsAsync();
        Task<Reward> CreateRewardAsync(Reward reward);
        Task<Reward> UpdateRewardAsync(Reward reward);
        Task<bool> DeleteRewardAsync(int id);
        
        // Redemption
        Task<RewardRedemption> RedeemRewardAsync(int userId, int rewardId);
        Task<List<RewardRedemption>> GetUserRedemptionsAsync(int userId);
        Task<bool> UseRedemptionAsync(string redemptionCode, string usageDetails);
        Task<bool> CancelRedemptionAsync(int redemptionId);
        
        // Rules management
        Task<List<PointsRule>> GetPointsRulesAsync();
        Task<PointsRule> CreateRuleAsync(PointsRule rule);
        Task<PointsRule> UpdateRuleAsync(PointsRule rule);
        Task<bool> DeleteRuleAsync(int id);
        
        // Automation
        Task ProcessBookingPointsAsync(int bookingId);
        Task ProcessReviewPointsAsync(int reviewId);
        Task ProcessReferralPointsAsync(int referrerId, int referredUserId);
        Task ProcessBirthdayPointsAsync();
        
        // Analytics
        Task<LoyaltyStats> GetLoyaltyStatsAsync();
        Task<List<PointsActivity>> GetPointsActivityAsync(DateTime fromDate, DateTime toDate);
        
        // Utilities
        Task<int> CalculatePointsForBookingAsync(decimal bookingAmount);
        Task<decimal> CalculateTierDiscountAsync(int userId, decimal amount);
        Task<bool> CanRedeemRewardAsync(int userId, int rewardId);
    }
}
using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    public class LoyaltyAccount
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
        
        /// <summary>
        /// Current points balance
        /// </summary>
        public int CurrentPoints { get; set; }
        
        /// <summary>
        /// Total points earned all time
        /// </summary>
        public int TotalPointsEarned { get; set; }
        
        /// <summary>
        /// Total points spent all time
        /// </summary>
        public int TotalPointsSpent { get; set; }
        
        /// <summary>
        /// Current tier level
        /// </summary>
        public int TierLevel { get; set; } = 1;
        
        /// <summary>
        /// Tier name (Bronze, Silver, Gold, Platinum)
        /// </summary>
        public string TierName { get; set; } = "Bronze";
        
        /// <summary>
        /// Points needed for next tier
        /// </summary>
        public int PointsToNextTier { get; set; }
        
        /// <summary>
        /// Account status
        /// </summary>
        public string Status { get; set; } = "Active";
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastActivityAt { get; set; }
        
        /// <summary>
        /// Points expiry date (if applicable)
        /// </summary>
        public DateTime? PointsExpiryDate { get; set; }

        public ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();
        public ICollection<RewardRedemption> RewardRedemptions { get; set; } = new List<RewardRedemption>();
    }

    public class PointTransaction
    {
        public int Id { get; set; }
        
        public int LoyaltyAccountId { get; set; }
        public LoyaltyAccount LoyaltyAccount { get; set; }
        
        /// <summary>
        /// Points amount (positive for earned, negative for spent)
        /// </summary>
        public int Points { get; set; }
        
        /// <summary>
        /// "Earned", "Spent", "Expired", "Adjusted"
        /// </summary>
        public string TransactionType { get; set; }
        
        /// <summary>
        /// "Booking", "Review", "Referral", "Bonus", "Redemption", "Expiry"
        /// </summary>
        public string Source { get; set; }
        
        public string Description { get; set; }
        
        /// <summary>
        /// Related entity ID (BookingId, ReviewId, etc.)
        /// </summary>
        public int? RelatedId { get; set; }
        
        /// <summary>
        /// Related entity type
        /// </summary>
        public string RelatedType { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// When these points expire (if applicable)
        /// </summary>
        public DateTime? ExpiryDate { get; set; }
        
        /// <summary>
        /// Balance after this transaction
        /// </summary>
        public int BalanceAfter { get; set; }
    }

    public class LoyaltyTier
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public int Level { get; set; }
        
        /// <summary>
        /// Minimum points required for this tier
        /// </summary>
        public int MinPoints { get; set; }
        
        /// <summary>
        /// Points multiplier for this tier
        /// </summary>
        public decimal PointsMultiplier { get; set; } = 1.0m;
        
        /// <summary>
        /// Discount percentage for this tier
        /// </summary>
        public decimal DiscountPercentage { get; set; }
        
        /// <summary>
        /// Special benefits (JSON)
        /// </summary>
        public string Benefits { get; set; }
        
        public string Color { get; set; }
        public string Icon { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class Reward
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        /// <summary>
        /// Points required to redeem
        /// </summary>
        public int PointsCost { get; set; }
        
        /// <summary>
        /// "Discount", "Free_Tour", "Free_Hotel", "Upgrade", "Gift", "Voucher"
        /// </summary>
        public string RewardType { get; set; }
        
        /// <summary>
        /// Reward value (discount amount, etc.)
        /// </summary>
        public decimal Value { get; set; }
        
        /// <summary>
        /// Reward details (JSON)
        /// </summary>
        public string RewardData { get; set; }
        
        public string Image { get; set; }
        
        /// <summary>
        /// Available quantity (-1 for unlimited)
        /// </summary>
        public int Quantity { get; set; } = -1;
        
        /// <summary>
        /// Redeemed count
        /// </summary>
        public int RedeemedCount { get; set; }
        
        /// <summary>
        /// Minimum tier level required
        /// </summary>
        public int MinTierLevel { get; set; } = 1;
        
        /// <summary>
        /// Expiry date for the reward offer
        /// </summary>
        public DateTime? ExpiryDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<RewardRedemption> Redemptions { get; set; } = new List<RewardRedemption>();
    }

    public class RewardRedemption
    {
        public int Id { get; set; }
        
        public int LoyaltyAccountId { get; set; }
        public LoyaltyAccount LoyaltyAccount { get; set; }
        
        public int RewardId { get; set; }
        public Reward Reward { get; set; }
        
        /// <summary>
        /// Points spent for this redemption
        /// </summary>
        public int PointsSpent { get; set; }
        
        /// <summary>
        /// "Pending", "Approved", "Used", "Expired", "Cancelled"
        /// </summary>
        public string Status { get; set; } = "Pending";
        
        /// <summary>
        /// Redemption code/voucher
        /// </summary>
        public string RedemptionCode { get; set; }
        
        /// <summary>
        /// When the reward expires
        /// </summary>
        public DateTime? ExpiryDate { get; set; }
        
        /// <summary>
        /// When the reward was used
        /// </summary>
        public DateTime? UsedAt { get; set; }
        
        /// <summary>
        /// Where/how the reward was used
        /// </summary>
        public string UsageDetails { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class PointsRule
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        /// <summary>
        /// "Booking", "Review", "Referral", "Registration", "Birthday"
        /// </summary>
        public string TriggerEvent { get; set; }
        
        /// <summary>
        /// Points awarded
        /// </summary>
        public int Points { get; set; }
        
        /// <summary>
        /// Multiplier based on amount spent (for bookings)
        /// </summary>
        public decimal PointsPerAmount { get; set; }
        
        /// <summary>
        /// Minimum amount required (for bookings)
        /// </summary>
        public decimal MinAmount { get; set; }
        
        /// <summary>
        /// Maximum points per transaction
        /// </summary>
        public int MaxPoints { get; set; }
        
        /// <summary>
        /// Rule conditions (JSON)
        /// </summary>
        public string Conditions { get; set; }
        
        /// <summary>
        /// How many times this rule can be applied per user
        /// </summary>
        public int MaxUsagePerUser { get; set; } = -1;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; }
    }

    public class LoyaltyStats
    {
        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
        public int TotalPointsIssued { get; set; }
        public int TotalPointsRedeemed { get; set; }
        public int TotalRedemptions { get; set; }
        public decimal AveragePointsPerMember { get; set; }
        public List<TierDistribution> TierDistribution { get; set; } = new();
        public List<PopularReward> PopularRewards { get; set; } = new();
        public List<PointsActivity> RecentActivity { get; set; } = new();
    }

    public class TierDistribution
    {
        public string TierName { get; set; }
        public int MemberCount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class PopularReward
    {
        public int RewardId { get; set; }
        public string RewardName { get; set; }
        public int RedemptionCount { get; set; }
        public int PointsCost { get; set; }
    }

    public class PointsActivity
    {
        public DateTime Date { get; set; }
        public int PointsEarned { get; set; }
        public int PointsSpent { get; set; }
        public int TransactionCount { get; set; }
    }
}
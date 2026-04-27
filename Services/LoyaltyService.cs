using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class LoyaltyService : ILoyaltyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public LoyaltyService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        #region Account Management

        public async Task<LoyaltyAccount> GetOrCreateAccountAsync(int userId)
        {
            var account = await _context.LoyaltyAccounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null)
            {
                account = new LoyaltyAccount
                {
                    UserId = userId,
                    CurrentPoints = 0,
                    TotalPointsEarned = 0,
                    TotalPointsSpent = 0,
                    TierLevel = 1,
                    TierName = "Bronze",
                    Status = "Active"
                };

                _context.LoyaltyAccounts.Add(account);
                await _context.SaveChangesAsync();

                // Award welcome points
                await AwardPointsAsync(userId, 100, "Registration", "Welcome bonus for joining our loyalty program");
            }

            return account;
        }

        public async Task<LoyaltyAccount> GetAccountByUserIdAsync(int userId)
        {
            return await _context.LoyaltyAccounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<List<PointTransaction>> GetTransactionHistoryAsync(int userId, int page = 1, int pageSize = 20)
        {
            var account = await GetAccountByUserIdAsync(userId);
            if (account == null) return new List<PointTransaction>();

            return await _context.PointTransactions
                .Where(t => t.LoyaltyAccountId == account.Id)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        #endregion

        #region Points Management

        public async Task<bool> AwardPointsAsync(int userId, int points, string source, string description, int? relatedId = null, string relatedType = null)
        {
            if (points <= 0) return false;

            var account = await GetOrCreateAccountAsync(userId);
            
            // Apply tier multiplier
            var tier = await _context.LoyaltyTiers.FirstOrDefaultAsync(t => t.Level == account.TierLevel);
            if (tier != null)
            {
                points = (int)(points * tier.PointsMultiplier);
            }

            // Create transaction
            var transaction = new PointTransaction
            {
                LoyaltyAccountId = account.Id,
                Points = points,
                TransactionType = "Earned",
                Source = source,
                Description = description,
                RelatedId = relatedId,
                RelatedType = relatedType,
                ExpiryDate = DateTime.Now.AddYears(1), // Points expire in 1 year
                BalanceAfter = account.CurrentPoints + points
            };

            _context.PointTransactions.Add(transaction);

            // Update account
            account.CurrentPoints += points;
            account.TotalPointsEarned += points;
            account.LastActivityAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Check for tier upgrade
            await UpdateUserTierAsync(userId);

            return true;
        }

        public async Task<bool> SpendPointsAsync(int userId, int points, string description, int? relatedId = null, string relatedType = null)
        {
            if (points <= 0) return false;

            var account = await GetAccountByUserIdAsync(userId);
            if (account == null || account.CurrentPoints < points) return false;

            // Create transaction
            var transaction = new PointTransaction
            {
                LoyaltyAccountId = account.Id,
                Points = -points,
                TransactionType = "Spent",
                Source = "Redemption",
                Description = description,
                RelatedId = relatedId,
                RelatedType = relatedType,
                BalanceAfter = account.CurrentPoints - points
            };

            _context.PointTransactions.Add(transaction);

            // Update account
            account.CurrentPoints -= points;
            account.TotalPointsSpent += points;
            account.LastActivityAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AdjustPointsAsync(int userId, int points, string reason)
        {
            var account = await GetAccountByUserIdAsync(userId);
            if (account == null) return false;

            var transaction = new PointTransaction
            {
                LoyaltyAccountId = account.Id,
                Points = points,
                TransactionType = "Adjusted",
                Source = "Manual",
                Description = reason,
                BalanceAfter = account.CurrentPoints + points
            };

            _context.PointTransactions.Add(transaction);

            account.CurrentPoints += points;
            if (points > 0)
            {
                account.TotalPointsEarned += points;
            }
            else
            {
                account.TotalPointsSpent += Math.Abs(points);
            }

            await _context.SaveChangesAsync();

            if (points > 0)
            {
                await UpdateUserTierAsync(userId);
            }

            return true;
        }

        public async Task ProcessPointsExpiryAsync()
        {
            var expiredTransactions = await _context.PointTransactions
                .Where(t => t.ExpiryDate <= DateTime.Now && t.TransactionType == "Earned")
                .Include(t => t.LoyaltyAccount)
                .ToListAsync();

            foreach (var transaction in expiredTransactions)
            {
                // Create expiry transaction
                var expiryTransaction = new PointTransaction
                {
                    LoyaltyAccountId = transaction.LoyaltyAccountId,
                    Points = -transaction.Points,
                    TransactionType = "Expired",
                    Source = "Expiry",
                    Description = $"Points expired from transaction on {transaction.CreatedAt:dd/MM/yyyy}",
                    BalanceAfter = transaction.LoyaltyAccount.CurrentPoints - transaction.Points
                };

                _context.PointTransactions.Add(expiryTransaction);

                // Update account
                transaction.LoyaltyAccount.CurrentPoints -= transaction.Points;
                
                // Mark original transaction as processed
                transaction.ExpiryDate = null;
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Tier Management

        public async Task UpdateUserTierAsync(int userId)
        {
            var account = await GetAccountByUserIdAsync(userId);
            if (account == null) return;

            var tiers = await _context.LoyaltyTiers
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.MinPoints)
                .ToListAsync();

            var newTier = tiers.FirstOrDefault(t => account.TotalPointsEarned >= t.MinPoints);
            if (newTier != null && newTier.Level > account.TierLevel)
            {
                var oldTierName = account.TierName;
                account.TierLevel = newTier.Level;
                account.TierName = newTier.Name;

                // Calculate points to next tier
                var nextTier = tiers.FirstOrDefault(t => t.Level > newTier.Level);
                account.PointsToNextTier = nextTier != null ? nextTier.MinPoints - account.TotalPointsEarned : 0;

                await _context.SaveChangesAsync();

                // Award tier upgrade bonus
                await AwardPointsAsync(userId, 500, "Tier_Upgrade", $"Congratulations on reaching {newTier.Name} tier!");
            }
        }

        public async Task<List<LoyaltyTier>> GetTiersAsync()
        {
            return await _context.LoyaltyTiers
                .Where(t => t.IsActive)
                .OrderBy(t => t.Level)
                .ToListAsync();
        }

        public async Task<LoyaltyTier> CreateTierAsync(LoyaltyTier tier)
        {
            _context.LoyaltyTiers.Add(tier);
            await _context.SaveChangesAsync();
            return tier;
        }

        public async Task<LoyaltyTier> UpdateTierAsync(LoyaltyTier tier)
        {
            _context.LoyaltyTiers.Update(tier);
            await _context.SaveChangesAsync();
            return tier;
        }

        #endregion

        #region Rewards Management

        public async Task<List<Reward>> GetAvailableRewardsAsync(int userId)
        {
            var account = await GetAccountByUserIdAsync(userId);
            if (account == null) return new List<Reward>();

            return await _context.Rewards
                .Where(r => r.IsActive && 
                           r.MinTierLevel <= account.TierLevel &&
                           r.PointsCost <= account.CurrentPoints &&
                           (r.ExpiryDate == null || r.ExpiryDate > DateTime.Now) &&
                           (r.Quantity == -1 || r.RedeemedCount < r.Quantity))
                .OrderBy(r => r.PointsCost)
                .ToListAsync();
        }

        public async Task<List<Reward>> GetAllRewardsAsync()
        {
            return await _context.Rewards
                .OrderBy(r => r.PointsCost)
                .ToListAsync();
        }

        public async Task<Reward> CreateRewardAsync(Reward reward)
        {
            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();
            return reward;
        }

        public async Task<Reward> UpdateRewardAsync(Reward reward)
        {
            _context.Rewards.Update(reward);
            await _context.SaveChangesAsync();
            return reward;
        }

        public async Task<bool> DeleteRewardAsync(int id)
        {
            var reward = await _context.Rewards.FindAsync(id);
            if (reward != null)
            {
                reward.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        #endregion

        #region Redemption

        public async Task<RewardRedemption> RedeemRewardAsync(int userId, int rewardId)
        {
            var account = await GetAccountByUserIdAsync(userId);
            var reward = await _context.Rewards.FindAsync(rewardId);

            if (account == null || reward == null || !await CanRedeemRewardAsync(userId, rewardId))
            {
                return null;
            }

            // Spend points
            if (!await SpendPointsAsync(userId, reward.PointsCost, $"Redeemed: {reward.Name}", rewardId, "Reward"))
            {
                return null;
            }

            // Create redemption
            var redemption = new RewardRedemption
            {
                LoyaltyAccountId = account.Id,
                RewardId = rewardId,
                PointsSpent = reward.PointsCost,
                Status = "Approved",
                RedemptionCode = GenerateRedemptionCode(),
                ExpiryDate = DateTime.Now.AddDays(30) // Redemption expires in 30 days
            };

            _context.RewardRedemptions.Add(redemption);

            // Update reward stats
            reward.RedeemedCount++;

            await _context.SaveChangesAsync();

            return redemption;
        }

        public async Task<List<RewardRedemption>> GetUserRedemptionsAsync(int userId)
        {
            var account = await GetAccountByUserIdAsync(userId);
            if (account == null) return new List<RewardRedemption>();

            return await _context.RewardRedemptions
                .Include(r => r.Reward)
                .Where(r => r.LoyaltyAccountId == account.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UseRedemptionAsync(string redemptionCode, string usageDetails)
        {
            var redemption = await _context.RewardRedemptions
                .FirstOrDefaultAsync(r => r.RedemptionCode == redemptionCode && r.Status == "Approved");

            if (redemption != null && (redemption.ExpiryDate == null || redemption.ExpiryDate > DateTime.Now))
            {
                redemption.Status = "Used";
                redemption.UsedAt = DateTime.Now;
                redemption.UsageDetails = usageDetails;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> CancelRedemptionAsync(int redemptionId)
        {
            var redemption = await _context.RewardRedemptions
                .Include(r => r.LoyaltyAccount)
                .Include(r => r.Reward)
                .FirstOrDefaultAsync(r => r.Id == redemptionId && r.Status == "Approved");

            if (redemption != null)
            {
                // Refund points
                await AwardPointsAsync(redemption.LoyaltyAccount.UserId, redemption.PointsSpent, 
                    "Refund", $"Refund for cancelled redemption: {redemption.Reward.Name}");

                redemption.Status = "Cancelled";
                redemption.Reward.RedeemedCount--;

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        #endregion

        #region Rules Management

        public async Task<List<PointsRule>> GetPointsRulesAsync()
        {
            return await _context.PointsRules
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<PointsRule> CreateRuleAsync(PointsRule rule)
        {
            _context.PointsRules.Add(rule);
            await _context.SaveChangesAsync();
            return rule;
        }

        public async Task<PointsRule> UpdateRuleAsync(PointsRule rule)
        {
            _context.PointsRules.Update(rule);
            await _context.SaveChangesAsync();
            return rule;
        }

        public async Task<bool> DeleteRuleAsync(int id)
        {
            var rule = await _context.PointsRules.FindAsync(id);
            if (rule != null)
            {
                rule.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        #endregion

        #region Automation

        public async Task ProcessBookingPointsAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking?.User != null && booking.Status == "Confirmed")
            {
                var points = await CalculatePointsForBookingAsync(booking.TotalPrice);
                await AwardPointsAsync(booking.UserId, points, "Booking", 
                    $"Points earned from booking #{bookingId}", bookingId, "Booking");
            }
        }

        public async Task ProcessReviewPointsAsync(int reviewId)
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review?.User != null)
            {
                var rule = await _context.PointsRules
                    .FirstOrDefaultAsync(r => r.TriggerEvent == "Review" && r.IsActive);

                if (rule != null)
                {
                    await AwardPointsAsync(review.UserId ?? 0, rule.Points, "Review", 
                        "Points earned for writing a review", reviewId, "Review");
                }
            }
        }

        public async Task ProcessReferralPointsAsync(int referrerId, int referredUserId)
        {
            var rule = await _context.PointsRules
                .FirstOrDefaultAsync(r => r.TriggerEvent == "Referral" && r.IsActive);

            if (rule != null)
            {
                // Award points to referrer
                await AwardPointsAsync(referrerId, rule.Points, "Referral", 
                    "Points earned for referring a friend", referredUserId, "Referral");

                // Award bonus points to referred user
                await AwardPointsAsync(referredUserId, rule.Points / 2, "Referral", 
                    "Welcome bonus for being referred", referrerId, "Referral");
            }
        }

        public async Task ProcessBirthdayPointsAsync()
        {
            var today = DateTime.Today;
            var users = await _context.Users
                .Where(u => u.DateOfBirth.HasValue && 
                           u.DateOfBirth.Value.Month == today.Month && 
                           u.DateOfBirth.Value.Day == today.Day)
                .ToListAsync();

            var rule = await _context.PointsRules
                .FirstOrDefaultAsync(r => r.TriggerEvent == "Birthday" && r.IsActive);

            if (rule != null)
            {
                foreach (var user in users)
                {
                    await AwardPointsAsync(user.Id, rule.Points, "Birthday", 
                        "Happy Birthday! Enjoy your special day bonus points");
                }
            }
        }

        #endregion

        #region Analytics

        public async Task<LoyaltyStats> GetLoyaltyStatsAsync()
        {
            var totalMembers = await _context.LoyaltyAccounts.CountAsync();
            var activeMembers = await _context.LoyaltyAccounts.CountAsync(a => a.Status == "Active");
            var totalPointsIssued = await _context.PointTransactions
                .Where(t => t.TransactionType == "Earned")
                .SumAsync(t => t.Points);
            var totalPointsRedeemed = await _context.PointTransactions
                .Where(t => t.TransactionType == "Spent")
                .SumAsync(t => Math.Abs(t.Points));
            var totalRedemptions = await _context.RewardRedemptions.CountAsync();

            var avgPointsPerMember = totalMembers > 0 ? (decimal)totalPointsIssued / totalMembers : 0;

            // Tier distribution
            var tierDistribution = await _context.LoyaltyAccounts
                .GroupBy(a => a.TierName)
                .Select(g => new TierDistribution
                {
                    TierName = g.Key,
                    MemberCount = g.Count(),
                    Percentage = totalMembers > 0 ? (decimal)g.Count() / totalMembers * 100 : 0
                })
                .ToListAsync();

            // Popular rewards
            var popularRewards = await _context.Rewards
                .OrderByDescending(r => r.RedeemedCount)
                .Take(5)
                .Select(r => new PopularReward
                {
                    RewardId = r.Id,
                    RewardName = r.Name,
                    RedemptionCount = r.RedeemedCount,
                    PointsCost = r.PointsCost
                })
                .ToListAsync();

            return new LoyaltyStats
            {
                TotalMembers = totalMembers,
                ActiveMembers = activeMembers,
                TotalPointsIssued = totalPointsIssued,
                TotalPointsRedeemed = totalPointsRedeemed,
                TotalRedemptions = totalRedemptions,
                AveragePointsPerMember = avgPointsPerMember,
                TierDistribution = tierDistribution,
                PopularRewards = popularRewards
            };
        }

        public async Task<List<PointsActivity>> GetPointsActivityAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.PointTransactions
                .Where(t => t.CreatedAt >= fromDate && t.CreatedAt <= toDate)
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new PointsActivity
                {
                    Date = g.Key,
                    PointsEarned = g.Where(t => t.TransactionType == "Earned").Sum(t => t.Points),
                    PointsSpent = g.Where(t => t.TransactionType == "Spent").Sum(t => Math.Abs(t.Points)),
                    TransactionCount = g.Count()
                })
                .OrderBy(a => a.Date)
                .ToListAsync();
        }

        #endregion

        #region Utilities

        public async Task<int> CalculatePointsForBookingAsync(decimal bookingAmount)
        {
            var rule = await _context.PointsRules
                .FirstOrDefaultAsync(r => r.TriggerEvent == "Booking" && r.IsActive);

            if (rule != null && bookingAmount >= rule.MinAmount)
            {
                var points = (int)(bookingAmount * rule.PointsPerAmount) + rule.Points;
                return Math.Min(points, rule.MaxPoints > 0 ? rule.MaxPoints : int.MaxValue);
            }

            // Default: 1 point per 1000 VND
            return (int)(bookingAmount / 1000);
        }

        public async Task<decimal> CalculateTierDiscountAsync(int userId, decimal amount)
        {
            var account = await GetAccountByUserIdAsync(userId);
            if (account == null) return 0;

            var tier = await _context.LoyaltyTiers
                .FirstOrDefaultAsync(t => t.Level == account.TierLevel);

            return tier != null ? amount * tier.DiscountPercentage / 100 : 0;
        }

        public async Task<bool> CanRedeemRewardAsync(int userId, int rewardId)
        {
            var account = await GetAccountByUserIdAsync(userId);
            var reward = await _context.Rewards.FindAsync(rewardId);

            if (account == null || reward == null || !reward.IsActive) return false;

            return account.CurrentPoints >= reward.PointsCost &&
                   account.TierLevel >= reward.MinTierLevel &&
                   (reward.ExpiryDate == null || reward.ExpiryDate > DateTime.Now) &&
                   (reward.Quantity == -1 || reward.RedeemedCount < reward.Quantity);
        }

        private string GenerateRedemptionCode()
        {
            return $"RDM{DateTime.Now:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
        }

        #endregion
    }
}
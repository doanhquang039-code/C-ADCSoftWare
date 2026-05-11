using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using System.Text.Json;

namespace WEBDULICH.Services.CustomerSegmentation
{
    public class CustomerSegmentationService : ICustomerSegmentationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomerSegmentationService> _logger;

        public CustomerSegmentationService(
            ApplicationDbContext context,
            ILogger<CustomerSegmentationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CustomerSegment> CreateSegmentAsync(CustomerSegment segment)
        {
            _context.CustomerSegments.Add(segment);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Created customer segment: {segment.Name}");
            return segment;
        }

        public async Task<CustomerSegment> GetSegmentByIdAsync(int id)
        {
            return await _context.CustomerSegments
                .Include(s => s.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<CustomerSegment>> GetAllSegmentsAsync()
        {
            return await _context.CustomerSegments
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.CustomerCount)
                .ToListAsync();
        }

        public async Task<CustomerSegment> UpdateSegmentAsync(CustomerSegment segment)
        {
            segment.LastUpdated = DateTime.Now;
            _context.CustomerSegments.Update(segment);
            await _context.SaveChangesAsync();
            return segment;
        }

        public async Task<bool> DeleteSegmentAsync(int id)
        {
            var segment = await _context.CustomerSegments.FindAsync(id);
            if (segment == null) return false;

            segment.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CustomerSegment>> AnalyzeAndCreateSegmentsAsync()
        {
            var segments = new List<CustomerSegment>();

            // 1. High Value Customers
            var highValueSegment = new CustomerSegment
            {
                Name = "High Value Customers",
                Description = "Customers with high lifetime value and frequent bookings",
                SegmentType = "Behavioral",
                Criteria = JsonSerializer.Serialize(new
                {
                    lifetimeValue = ">10000000",
                    bookingFrequency = ">5",
                    avgSpending = ">2000000"
                }),
                Color = "#4CAF50",
                Icon = "star"
            };
            segments.Add(highValueSegment);

            // 2. At-Risk Customers
            var atRiskSegment = new CustomerSegment
            {
                Name = "At-Risk Customers",
                Description = "Customers with high churn risk",
                SegmentType = "Behavioral",
                Criteria = JsonSerializer.Serialize(new
                {
                    churnRiskScore = ">0.7",
                    daysSinceLastBooking = ">180"
                }),
                Color = "#F44336",
                Icon = "warning"
            };
            segments.Add(atRiskSegment);

            // 3. Young Travelers
            var youngTravelersSegment = new CustomerSegment
            {
                Name = "Young Travelers",
                Description = "Travelers aged 18-30",
                SegmentType = "Demographic",
                Criteria = JsonSerializer.Serialize(new
                {
                    ageRange = "18-30",
                    preferredTourTypes = new[] { "Adventure", "Backpacking", "Budget" }
                }),
                Color = "#2196F3",
                Icon = "hiking"
            };
            segments.Add(youngTravelersSegment);

            // 4. Luxury Travelers
            var luxurySegment = new CustomerSegment
            {
                Name = "Luxury Travelers",
                Description = "High-end travelers preferring premium experiences",
                SegmentType = "Psychographic",
                Criteria = JsonSerializer.Serialize(new
                {
                    avgSpending = ">5000000",
                    preferredTourTypes = new[] { "Luxury", "Premium", "VIP" }
                }),
                Color = "#FFD700",
                Icon = "diamond"
            };
            segments.Add(luxurySegment);

            // 5. Family Travelers
            var familySegment = new CustomerSegment
            {
                Name = "Family Travelers",
                Description = "Families with children",
                SegmentType = "Demographic",
                Criteria = JsonSerializer.Serialize(new
                {
                    bookingPattern = "family",
                    avgGroupSize = ">3"
                }),
                Color = "#9C27B0",
                Icon = "family"
            };
            segments.Add(familySegment);

            // Save all segments
            _context.CustomerSegments.AddRange(segments);
            await _context.SaveChangesAsync();

            // Update members for each segment
            foreach (var segment in segments)
            {
                await UpdateSegmentMembersAsync(segment.Id);
            }

            _logger.LogInformation($"Created {segments.Count} customer segments");
            return segments;
        }

        public async Task UpdateSegmentMembersAsync(int segmentId)
        {
            var segment = await GetSegmentByIdAsync(segmentId);
            if (segment == null) return;

            // Clear existing members
            var existingMembers = await _context.CustomerSegmentMembers
                .Where(m => m.CustomerSegmentId == segmentId)
                .ToListAsync();
            _context.CustomerSegmentMembers.RemoveRange(existingMembers);

            // Get all customer behaviors
            var behaviors = await _context.CustomerBehaviors.ToListAsync();

            // Match customers based on criteria
            var newMembers = new List<CustomerSegmentMember>();

            foreach (var behavior in behaviors)
            {
                var score = CalculateSegmentMatchScore(segment, behavior);
                if (score >= 0.5m) // Threshold
                {
                    newMembers.Add(new CustomerSegmentMember
                    {
                        CustomerSegmentId = segmentId,
                        UserId = behavior.UserId,
                        ConfidenceScore = score,
                        MatchingCriteria = JsonSerializer.Serialize(new { score })
                    });
                }
            }

            _context.CustomerSegmentMembers.AddRange(newMembers);

            // Update segment statistics
            segment.CustomerCount = newMembers.Count;
            segment.Percentage = behaviors.Count > 0 
                ? (decimal)newMembers.Count / behaviors.Count * 100 
                : 0;
            segment.AverageSpending = newMembers.Any()
                ? behaviors.Where(b => newMembers.Any(m => m.UserId == b.UserId))
                    .Average(b => b.AverageBookingValue)
                : 0;
            segment.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        private decimal CalculateSegmentMatchScore(CustomerSegment segment, CustomerBehavior behavior)
        {
            // Simple scoring logic - can be enhanced with ML
            decimal score = 0;

            if (segment.Name.Contains("High Value"))
            {
                if (behavior.LifetimeValue > 10000000) score += 0.4m;
                if (behavior.BookingFrequency > 5) score += 0.3m;
                if (behavior.AverageBookingValue > 2000000) score += 0.3m;
            }
            else if (segment.Name.Contains("At-Risk"))
            {
                if (behavior.ChurnRiskScore > 0.7m) score += 0.5m;
                if (behavior.DaysSinceLastBooking > 180) score += 0.5m;
            }
            else if (segment.Name.Contains("Luxury"))
            {
                if (behavior.AverageBookingValue > 5000000) score += 0.6m;
                if (behavior.LoyaltyScore > 80) score += 0.4m;
            }

            return Math.Min(score, 1.0m);
        }

        public async Task<List<User>> GetSegmentMembersAsync(int segmentId)
        {
            return await _context.CustomerSegmentMembers
                .Where(m => m.CustomerSegmentId == segmentId)
                .Include(m => m.User)
                .Select(m => m.User)
                .ToListAsync();
        }

        public async Task<CustomerSegment> GetUserPrimarySegmentAsync(int userId)
        {
            var membership = await _context.CustomerSegmentMembers
                .Where(m => m.UserId == userId)
                .Include(m => m.CustomerSegment)
                .OrderByDescending(m => m.ConfidenceScore)
                .FirstOrDefaultAsync();

            return membership?.CustomerSegment;
        }

        public async Task<CustomerBehavior> GetCustomerBehaviorAsync(int userId)
        {
            return await _context.CustomerBehaviors
                .FirstOrDefaultAsync(b => b.UserId == userId);
        }

        public async Task UpdateCustomerBehaviorAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .ToListAsync();

            var reviews = await _context.Reviews
                .Where(r => r.UserId == userId)
                .ToListAsync();

            var behavior = await _context.CustomerBehaviors
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (behavior == null)
            {
                behavior = new CustomerBehavior { UserId = userId };
                _context.CustomerBehaviors.Add(behavior);
            }

            // Calculate metrics
            behavior.TotalBookings = bookings.Count;
            behavior.TotalSpending = bookings.Sum(b => b.TotalPrice);
            behavior.AverageBookingValue = bookings.Any() 
                ? bookings.Average(b => b.TotalPrice) 
                : 0;

            var lastBooking = bookings.OrderByDescending(b => b.CreatedAt).FirstOrDefault();
            behavior.LastBookingDate = lastBooking?.CreatedAt;
            behavior.DaysSinceLastBooking = lastBooking != null
                ? (DateTime.Now - lastBooking.CreatedAt).Days
                : 0;

            behavior.CancellationRate = bookings.Any()
                ? (decimal)bookings.Count(b => b.Status == "Cancelled") / bookings.Count * 100
                : 0;

            behavior.ReviewRate = bookings.Any()
                ? (decimal)reviews.Count / bookings.Count * 100
                : 0;

            behavior.AverageReviewRating = reviews.Any()
                ? (decimal)reviews.Average(r => r.Rating)
                : 0;

            // Calculate scores
            behavior.LifetimeValue = behavior.TotalSpending;
            behavior.ChurnRiskScore = CalculateChurnRisk(behavior);
            behavior.EngagementScore = CalculateEngagementScore(behavior);
            behavior.LoyaltyScore = CalculateLoyaltyScore(behavior);

            behavior.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        private decimal CalculateChurnRisk(CustomerBehavior behavior)
        {
            decimal risk = 0;

            // Days since last booking
            if (behavior.DaysSinceLastBooking > 365) risk += 0.4m;
            else if (behavior.DaysSinceLastBooking > 180) risk += 0.2m;

            // Booking frequency
            if (behavior.BookingFrequency < 1) risk += 0.3m;

            // Cancellation rate
            if (behavior.CancellationRate > 30) risk += 0.2m;

            // Engagement
            if (behavior.EngagementScore < 30) risk += 0.1m;

            return Math.Min(risk, 1.0m);
        }

        private decimal CalculateEngagementScore(CustomerBehavior behavior)
        {
            decimal score = 0;

            // Recent activity
            if (behavior.DaysSinceLastBooking < 30) score += 30;
            else if (behavior.DaysSinceLastBooking < 90) score += 20;
            else if (behavior.DaysSinceLastBooking < 180) score += 10;

            // Booking frequency
            score += Math.Min(behavior.BookingFrequency * 10, 30);

            // Review activity
            score += Math.Min(behavior.ReviewRate, 20);

            // Referrals
            score += Math.Min(behavior.ReferralCount * 5, 20);

            return Math.Min(score, 100);
        }

        private decimal CalculateLoyaltyScore(CustomerBehavior behavior)
        {
            decimal score = 0;

            // Tenure (total bookings)
            score += Math.Min(behavior.TotalBookings * 5, 30);

            // Spending
            if (behavior.TotalSpending > 20000000) score += 30;
            else if (behavior.TotalSpending > 10000000) score += 20;
            else if (behavior.TotalSpending > 5000000) score += 10;

            // Review quality
            if (behavior.AverageReviewRating >= 4.5m) score += 20;
            else if (behavior.AverageReviewRating >= 4.0m) score += 15;
            else if (behavior.AverageReviewRating >= 3.5m) score += 10;

            // Low cancellation
            if (behavior.CancellationRate < 10) score += 20;
            else if (behavior.CancellationRate < 20) score += 10;

            return Math.Min(score, 100);
        }

        public async Task<List<CustomerBehavior>> GetHighValueCustomersAsync(int count = 100)
        {
            return await _context.CustomerBehaviors
                .OrderByDescending(b => b.LifetimeValue)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<CustomerBehavior>> GetChurnRiskCustomersAsync(decimal minRiskScore = 0.7m)
        {
            return await _context.CustomerBehaviors
                .Where(b => b.ChurnRiskScore >= minRiskScore)
                .OrderByDescending(b => b.ChurnRiskScore)
                .ToListAsync();
        }

        public async Task<Dictionary<string, object>> GetSegmentInsightsAsync(int segmentId)
        {
            var segment = await GetSegmentByIdAsync(segmentId);
            if (segment == null) return null;

            var members = await GetSegmentMembersAsync(segmentId);
            var behaviors = await _context.CustomerBehaviors
                .Where(b => members.Select(m => m.Id).Contains(b.UserId))
                .ToListAsync();

            return new Dictionary<string, object>
            {
                ["segmentName"] = segment.Name,
                ["totalMembers"] = segment.CustomerCount,
                ["percentage"] = segment.Percentage,
                ["averageSpending"] = segment.AverageSpending,
                ["averageLifetimeValue"] = behaviors.Any() ? behaviors.Average(b => b.LifetimeValue) : 0,
                ["averageBookingFrequency"] = behaviors.Any() ? behaviors.Average(b => b.BookingFrequency) : 0,
                ["averageLoyaltyScore"] = behaviors.Any() ? behaviors.Average(b => b.LoyaltyScore) : 0,
                ["churnRate"] = segment.ChurnRate
            };
        }

        public async Task<Dictionary<string, object>> GetOverallSegmentationInsightsAsync()
        {
            var segments = await GetAllSegmentsAsync();
            var totalCustomers = await _context.Users.CountAsync();

            return new Dictionary<string, object>
            {
                ["totalSegments"] = segments.Count,
                ["totalCustomers"] = totalCustomers,
                ["segments"] = segments.Select(s => new
                {
                    s.Name,
                    s.CustomerCount,
                    s.Percentage,
                    s.AverageSpending
                })
            };
        }

        public async Task<List<string>> GetMarketingRecommendationsAsync(int segmentId)
        {
            var segment = await GetSegmentByIdAsync(segmentId);
            if (segment == null) return new List<string>();

            var recommendations = new List<string>();

            if (segment.Name.Contains("High Value"))
            {
                recommendations.Add("Offer exclusive VIP packages and early access to new tours");
                recommendations.Add("Provide personalized concierge services");
                recommendations.Add("Create loyalty rewards program with premium benefits");
            }
            else if (segment.Name.Contains("At-Risk"))
            {
                recommendations.Add("Send win-back campaigns with special discounts");
                recommendations.Add("Conduct surveys to understand reasons for inactivity");
                recommendations.Add("Offer personalized tour recommendations based on past preferences");
            }
            else if (segment.Name.Contains("Young"))
            {
                recommendations.Add("Promote adventure and budget-friendly tours on social media");
                recommendations.Add("Create group booking discounts");
                recommendations.Add("Highlight Instagram-worthy destinations");
            }
            else if (segment.Name.Contains("Luxury"))
            {
                recommendations.Add("Showcase premium accommodations and exclusive experiences");
                recommendations.Add("Offer private tours and customized itineraries");
                recommendations.Add("Partner with luxury brands for co-marketing");
            }

            return recommendations;
        }
    }
}

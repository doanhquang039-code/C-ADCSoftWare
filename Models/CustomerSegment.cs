using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    /// <summary>
    /// Customer Segment - Phân khúc khách hàng
    /// </summary>
    public class CustomerSegment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// Segment type: "Demographic", "Behavioral", "Geographic", "Psychographic"
        /// </summary>
        [Required]
        public string SegmentType { get; set; }

        /// <summary>
        /// Criteria (JSON object)
        /// Example: {"ageRange": "25-35", "bookingFrequency": ">5", "avgSpending": ">5000000"}
        /// </summary>
        public string Criteria { get; set; }

        /// <summary>
        /// Số lượng khách hàng trong segment
        /// </summary>
        public int CustomerCount { get; set; }

        /// <summary>
        /// % của tổng khách hàng
        /// </summary>
        public decimal Percentage { get; set; }

        /// <summary>
        /// Avg spending per customer
        /// </summary>
        public decimal AverageSpending { get; set; }

        /// <summary>
        /// Avg booking frequency
        /// </summary>
        public decimal AverageBookingFrequency { get; set; }

        /// <summary>
        /// Lifetime value
        /// </summary>
        public decimal LifetimeValue { get; set; }

        /// <summary>
        /// Churn rate (%)
        /// </summary>
        public decimal ChurnRate { get; set; }

        /// <summary>
        /// Preferred destinations (JSON array)
        /// </summary>
        public string PreferredDestinations { get; set; }

        /// <summary>
        /// Preferred tour types (JSON array)
        /// </summary>
        public string PreferredTourTypes { get; set; }

        /// <summary>
        /// Booking patterns (JSON object)
        /// </summary>
        public string BookingPatterns { get; set; }

        /// <summary>
        /// Marketing recommendations (JSON array)
        /// </summary>
        public string MarketingRecommendations { get; set; }

        /// <summary>
        /// Color for visualization
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Icon
        /// </summary>
        public string Icon { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<CustomerSegmentMember> Members { get; set; }
    }

    /// <summary>
    /// Customer Segment Member - Khách hàng trong segment
    /// </summary>
    public class CustomerSegmentMember
    {
        public int Id { get; set; }

        public int CustomerSegmentId { get; set; }
        public CustomerSegment CustomerSegment { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        /// <summary>
        /// Confidence score (0-1)
        /// </summary>
        public decimal ConfidenceScore { get; set; }

        /// <summary>
        /// Matching criteria (JSON object)
        /// </summary>
        public string MatchingCriteria { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Customer Behavior - Hành vi khách hàng
    /// </summary>
    public class CustomerBehavior
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        /// <summary>
        /// Tổng số bookings
        /// </summary>
        public int TotalBookings { get; set; }

        /// <summary>
        /// Tổng chi tiêu
        /// </summary>
        public decimal TotalSpending { get; set; }

        /// <summary>
        /// Avg booking value
        /// </summary>
        public decimal AverageBookingValue { get; set; }

        /// <summary>
        /// Booking frequency (bookings per year)
        /// </summary>
        public decimal BookingFrequency { get; set; }

        /// <summary>
        /// Last booking date
        /// </summary>
        public DateTime? LastBookingDate { get; set; }

        /// <summary>
        /// Days since last booking
        /// </summary>
        public int DaysSinceLastBooking { get; set; }

        /// <summary>
        /// Preferred booking channel: "Web", "Mobile", "Phone"
        /// </summary>
        public string PreferredChannel { get; set; }

        /// <summary>
        /// Preferred payment method
        /// </summary>
        public string PreferredPaymentMethod { get; set; }

        /// <summary>
        /// Avg advance booking days
        /// </summary>
        public int AverageAdvanceBookingDays { get; set; }

        /// <summary>
        /// Cancellation rate (%)
        /// </summary>
        public decimal CancellationRate { get; set; }

        /// <summary>
        /// Review rate (% of bookings reviewed)
        /// </summary>
        public decimal ReviewRate { get; set; }

        /// <summary>
        /// Avg review rating
        /// </summary>
        public decimal AverageReviewRating { get; set; }

        /// <summary>
        /// Referral count
        /// </summary>
        public int ReferralCount { get; set; }

        /// <summary>
        /// Customer lifetime value
        /// </summary>
        public decimal LifetimeValue { get; set; }

        /// <summary>
        /// Churn risk score (0-1)
        /// </summary>
        public decimal ChurnRiskScore { get; set; }

        /// <summary>
        /// Engagement score (0-100)
        /// </summary>
        public decimal EngagementScore { get; set; }

        /// <summary>
        /// Loyalty score (0-100)
        /// </summary>
        public decimal LoyaltyScore { get; set; }

        /// <summary>
        /// Browsing history (JSON array)
        /// </summary>
        public string BrowsingHistory { get; set; }

        /// <summary>
        /// Search history (JSON array)
        /// </summary>
        public string SearchHistory { get; set; }

        /// <summary>
        /// Wishlist items (JSON array)
        /// </summary>
        public string WishlistItems { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}

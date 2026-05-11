using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    /// <summary>
    /// Price History - Lịch sử giá để phân tích và tối ưu
    /// </summary>
    public class PriceHistory
    {
        public int Id { get; set; }

        /// <summary>
        /// "Tour" or "Hotel"
        /// </summary>
        [Required]
        public string EntityType { get; set; }

        public int? TourId { get; set; }
        public Tour Tour { get; set; }

        public int? HotelId { get; set; }
        public Hotel Hotel { get; set; }

        /// <summary>
        /// Giá cũ
        /// </summary>
        public decimal OldPrice { get; set; }

        /// <summary>
        /// Giá mới
        /// </summary>
        public decimal NewPrice { get; set; }

        /// <summary>
        /// % thay đổi
        /// </summary>
        public decimal ChangePercentage { get; set; }

        /// <summary>
        /// Lý do thay đổi
        /// </summary>
        public string ChangeReason { get; set; }

        /// <summary>
        /// Ngày áp dụng
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Ngày hết hạn (null = vô thời hạn)
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Mùa (High, Low, Peak)
        /// </summary>
        public string Season { get; set; }

        /// <summary>
        /// Demand level (Low, Medium, High)
        /// </summary>
        public string DemandLevel { get; set; }

        /// <summary>
        /// Occupancy rate tại thời điểm thay đổi
        /// </summary>
        public decimal OccupancyRate { get; set; }

        /// <summary>
        /// Số booking trong 7 ngày trước
        /// </summary>
        public int BookingsLast7Days { get; set; }

        /// <summary>
        /// Số booking trong 7 ngày sau
        /// </summary>
        public int BookingsNext7Days { get; set; }

        /// <summary>
        /// User thay đổi giá (admin)
        /// </summary>
        public int? ChangedByUserId { get; set; }

        /// <summary>
        /// Tự động hay thủ công
        /// </summary>
        public bool IsAutomatic { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Dynamic Pricing Rule - Quy tắc định giá động
    /// </summary>
    public class DynamicPricingRule
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// "Tour" or "Hotel" or "All"
        /// </summary>
        public string AppliesTo { get; set; } = "All";

        /// <summary>
        /// Specific tour/hotel IDs (JSON array) - null = all
        /// </summary>
        public string EntityIds { get; set; }

        /// <summary>
        /// Priority (higher = applied first)
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Enabled/Disabled
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Rule type: "Occupancy", "Demand", "Season", "DayOfWeek", "Advance", "Competitor"
        /// </summary>
        [Required]
        public string RuleType { get; set; }

        /// <summary>
        /// Condition (JSON object)
        /// Example: {"occupancyRate": ">80", "daysInAdvance": "<7"}
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// Action type: "Increase", "Decrease", "SetFixed"
        /// </summary>
        [Required]
        public string ActionType { get; set; }

        /// <summary>
        /// Action value (percentage or fixed amount)
        /// </summary>
        public decimal ActionValue { get; set; }

        /// <summary>
        /// Min price limit
        /// </summary>
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Max price limit
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Valid from date
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// Valid to date
        /// </summary>
        public DateTime? ValidTo { get; set; }

        /// <summary>
        /// Days of week applicable (JSON array) - null = all days
        /// </summary>
        public string DaysOfWeek { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}

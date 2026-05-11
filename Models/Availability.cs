using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    /// <summary>
    /// Real-time Availability - Tình trạng còn chỗ real-time
    /// </summary>
    public class Availability
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
        /// Ngày cụ thể
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Tổng số chỗ
        /// </summary>
        public int TotalCapacity { get; set; }

        /// <summary>
        /// Số chỗ đã đặt
        /// </summary>
        public int BookedCapacity { get; set; }

        /// <summary>
        /// Số chỗ còn lại
        /// </summary>
        public int AvailableCapacity { get; set; }

        /// <summary>
        /// Số chỗ đang hold (pending bookings)
        /// </summary>
        public int HoldCapacity { get; set; }

        /// <summary>
        /// % occupancy
        /// </summary>
        public decimal OccupancyRate { get; set; }

        /// <summary>
        /// Giá hiện tại (có thể thay đổi theo demand)
        /// </summary>
        public decimal CurrentPrice { get; set; }

        /// <summary>
        /// Giá gốc
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// "Available", "Limited", "Full", "Closed"
        /// </summary>
        public string Status { get; set; } = "Available";

        /// <summary>
        /// Có cho phép overbooking không
        /// </summary>
        public bool AllowOverbooking { get; set; } = false;

        /// <summary>
        /// % overbooking tối đa
        /// </summary>
        public decimal MaxOverbookingPercentage { get; set; } = 10;

        /// <summary>
        /// Số lượt xem trong 24h
        /// </summary>
        public int ViewsLast24Hours { get; set; }

        /// <summary>
        /// Số booking trong 24h
        /// </summary>
        public int BookingsLast24Hours { get; set; }

        /// <summary>
        /// Demand level: "Low", "Medium", "High", "VeryHigh"
        /// </summary>
        public string DemandLevel { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Availability Block - Khóa chỗ tạm thời
    /// </summary>
    public class AvailabilityBlock
    {
        public int Id { get; set; }

        public int AvailabilityId { get; set; }
        public Availability Availability { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }

        /// <summary>
        /// Session ID (cho guest users)
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Số lượng chỗ block
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Thời gian hết hạn block
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// "Active", "Expired", "Converted", "Released"
        /// </summary>
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Booking ID nếu đã convert thành booking
        /// </summary>
        public int? BookingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

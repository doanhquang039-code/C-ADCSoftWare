using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    /// <summary>
    /// Booking system - đặt tour/hotel với calendar, số lượng người
    /// </summary>
    public class Booking
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        /// <summary>
        /// "Tour" hoặc "Hotel"
        /// </summary>
        [Required]
        public string BookingType { get; set; }

        public int? TourId { get; set; }
        public Tour Tour { get; set; }

        public int? HotelId { get; set; }
        public Hotel Hotel { get; set; }

        /// <summary>
        /// Ngày bắt đầu (check-in hoặc departure date)
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc (check-out hoặc return date)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Số lượng người lớn
        /// </summary>
        public int Adults { get; set; } = 1;

        /// <summary>
        /// Số lượng trẻ em
        /// </summary>
        public int Children { get; set; } = 0;

        /// <summary>
        /// Số phòng (cho hotel)
        /// </summary>
        public int Rooms { get; set; } = 1;

        /// <summary>
        /// Tổng giá
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// "Pending", "Confirmed", "Cancelled", "Completed"
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Ghi chú đặc biệt
        /// </summary>
        public string SpecialRequests { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Mã coupon đã áp dụng
        /// </summary>
        public string AppliedCouponCode { get; set; }

        /// <summary>
        /// Số tiền giảm giá
        /// </summary>
        public decimal DiscountAmount { get; set; }
    }
}

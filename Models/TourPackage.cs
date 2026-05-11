using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    /// <summary>
    /// Tour Package - Gói tour tùy chỉnh kết hợp nhiều tours, hotels, activities
    /// </summary>
    public class TourPackage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        /// <summary>
        /// User tạo package (null nếu là admin tạo)
        /// </summary>
        public int? UserId { get; set; }
        public User User { get; set; }

        /// <summary>
        /// "Custom" (do user tạo) hoặc "Predefined" (do admin tạo)
        /// </summary>
        [Required]
        public string PackageType { get; set; } = "Custom";

        /// <summary>
        /// Tổng giá gốc
        /// </summary>
        public decimal OriginalPrice { get; set; }

        /// <summary>
        /// Giá sau giảm (nếu có)
        /// </summary>
        public decimal FinalPrice { get; set; }

        /// <summary>
        /// Phần trăm giảm giá
        /// </summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Số ngày của package
        /// </summary>
        public int TotalDays { get; set; }

        /// <summary>
        /// Số đêm
        /// </summary>
        public int TotalNights { get; set; }

        /// <summary>
        /// Ngày bắt đầu dự kiến
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc dự kiến
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Số người tối thiểu
        /// </summary>
        public int MinPeople { get; set; } = 1;

        /// <summary>
        /// Số người tối đa
        /// </summary>
        public int MaxPeople { get; set; } = 10;

        /// <summary>
        /// "Draft", "Active", "Archived"
        /// </summary>
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// Có public không (cho phép người khác xem)
        /// </summary>
        public bool IsPublic { get; set; } = false;

        /// <summary>
        /// Số lượt xem
        /// </summary>
        public int ViewCount { get; set; } = 0;

        /// <summary>
        /// Số lượt đặt
        /// </summary>
        public int BookingCount { get; set; } = 0;

        /// <summary>
        /// Rating trung bình
        /// </summary>
        public decimal AverageRating { get; set; }

        /// <summary>
        /// Ảnh đại diện
        /// </summary>
        public string CoverImage { get; set; }

        /// <summary>
        /// Tags (JSON array)
        /// </summary>
        public string Tags { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<TourPackageItem> Items { get; set; }
        public ICollection<TourPackageBooking> Bookings { get; set; }
    }

    /// <summary>
    /// Item trong tour package (tour, hotel, activity)
    /// </summary>
    public class TourPackageItem
    {
        public int Id { get; set; }

        public int TourPackageId { get; set; }
        public TourPackage TourPackage { get; set; }

        /// <summary>
        /// "Tour", "Hotel", "Activity", "Transport"
        /// </summary>
        [Required]
        public string ItemType { get; set; }

        public int? TourId { get; set; }
        public Tour Tour { get; set; }

        public int? HotelId { get; set; }
        public Hotel Hotel { get; set; }

        /// <summary>
        /// Thứ tự trong package (Day 1, Day 2, ...)
        /// </summary>
        public int DayNumber { get; set; }

        /// <summary>
        /// Thứ tự trong ngày
        /// </summary>
        public int OrderInDay { get; set; }

        /// <summary>
        /// Tiêu đề hoạt động
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Thời gian bắt đầu
        /// </summary>
        public TimeSpan? StartTime { get; set; }

        /// <summary>
        /// Thời gian kết thúc
        /// </summary>
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        /// Giá của item này
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Booking cho tour package
    /// </summary>
    public class TourPackageBooking
    {
        public int Id { get; set; }

        public int TourPackageId { get; set; }
        public TourPackage TourPackage { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int NumberOfPeople { get; set; }

        public decimal TotalPrice { get; set; }

        /// <summary>
        /// "Pending", "Confirmed", "Cancelled", "Completed"
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Yêu cầu đặc biệt
        /// </summary>
        public string SpecialRequests { get; set; }

        /// <summary>
        /// Mã coupon
        /// </summary>
        public string CouponCode { get; set; }

        /// <summary>
        /// Số tiền giảm
        /// </summary>
        public decimal DiscountAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}

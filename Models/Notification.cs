using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    /// <summary>
    /// Notification system - thông báo đơn hàng, khuyến mãi
    /// </summary>
    public class Notification
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        /// <summary>
        /// "Order", "Promotion", "System", "Booking"
        /// </summary>
        public string Type { get; set; } = "System";

        /// <summary>
        /// Link liên quan (order detail, booking detail...)
        /// </summary>
        public string Link { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

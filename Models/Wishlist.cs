using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    /// <summary>
    /// Wishlist/Favorites - lưu tour/hotel yêu thích
    /// </summary>
    public class Wishlist
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        /// <summary>
        /// "Tour" hoặc "Hotel"
        /// </summary>
        [Required]
        public string ItemType { get; set; }

        /// <summary>
        /// ID của Tour hoặc Hotel
        /// </summary>
        public int ItemId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

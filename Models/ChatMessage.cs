using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    /// <summary>
    /// Chat/Support - hỗ trợ khách hàng
    /// </summary>
    public class ChatMessage
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }

        /// <summary>
        /// "User" hoặc "Admin"
        /// </summary>
        [Required]
        public string SenderType { get; set; }

        [Required]
        public string Message { get; set; }

        /// <summary>
        /// ID của admin nếu là admin reply
        /// </summary>
        public int? AdminId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Session ID để nhóm các tin nhắn cùng conversation
        /// </summary>
        public string SessionId { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    public class SecurityLog
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public string? Email { get; set; }

        [Required]
        public string EventType { get; set; } = string.Empty;

        public string? Details { get; set; }

        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ExpiresAt { get; set; }

        // Navigation properties
        public User? User { get; set; }
    }
}
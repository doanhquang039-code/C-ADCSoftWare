using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// "New" | "Read" | "Replied"
        /// </summary>
        public string Status { get; set; } = "New";
    }
}

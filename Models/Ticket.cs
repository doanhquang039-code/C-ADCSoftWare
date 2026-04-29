using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WEBDULICH.Services.Ticket;

namespace WEBDULICH.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TicketCode { get; set; } = string.Empty;

        [Required]
        public string QRCode { get; set; } = string.Empty;

        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.Active;

        [Required]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidUntil { get; set; }

        public DateTime? UsedAt { get; set; }

        [MaxLength(100)]
        public string? UsedBy { get; set; }

        [Required]
        [MaxLength(200)]
        public string PassengerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [EmailAddress]
        public string PassengerEmail { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PassengerPhone { get; set; } = string.Empty;

        /// <summary>
        /// Additional metadata stored as JSON
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? MetadataJson { get; set; }

        [NotMapped]
        public Dictionary<string, string> Metadata
        {
            get
            {
                if (string.IsNullOrEmpty(MetadataJson))
                    return new Dictionary<string, string>();
                
                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(MetadataJson) 
                           ?? new Dictionary<string, string>();
                }
                catch
                {
                    return new Dictionary<string, string>();
                }
            }
            set
            {
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(value);
            }
        }

        // Navigation properties
        [ForeignKey("BookingId")]
        public Orders Booking { get; set; } = null!;
    }
}

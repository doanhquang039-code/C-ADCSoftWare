using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBDULICH.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int TourId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        public DateTime? TravelDate { get; set; }

        [MaxLength(500)]
        public string? SpecialRequests { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public Orders Order { get; set; } = null!;

        [ForeignKey("TourId")]
        public Tour Tour { get; set; } = null!;
    }
}

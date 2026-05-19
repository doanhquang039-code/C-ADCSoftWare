using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBDULICH.Models
{
    /// <summary>
    /// Real-time availability for tours and hotels.
    /// </summary>
    public class Availability
    {
        public int Id { get; set; }

        /// <summary>
        /// "Tour" or "Hotel".
        /// </summary>
        [Required]
        public string EntityType { get; set; } = string.Empty;

        public int? TourId { get; set; }
        public Tour? Tour { get; set; }

        public int? HotelId { get; set; }
        public Hotel? Hotel { get; set; }

        [NotMapped]
        public int EntityId => TourId ?? HotelId ?? 0;

        [Required]
        public DateTime Date { get; set; }

        public int TotalCapacity { get; set; }
        public int BookedCapacity { get; set; }
        public int AvailableCapacity { get; set; }
        public int HoldCapacity { get; set; }
        public decimal OccupancyRate { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal BasePrice { get; set; }

        /// <summary>
        /// "Available", "Limited", "Full", or "Closed".
        /// </summary>
        public string Status { get; set; } = "Available";

        public bool AllowOverbooking { get; set; }
        public decimal MaxOverbookingPercentage { get; set; } = 10;
        public int ViewsLast24Hours { get; set; }
        public int BookingsLast24Hours { get; set; }

        /// <summary>
        /// "Low", "Medium", "High", or "VeryHigh".
        /// </summary>
        public string DemandLevel { get; set; } = "Low";

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Temporary hold for seats/rooms before a booking is completed.
    /// </summary>
    public class AvailabilityBlock
    {
        public int Id { get; set; }

        public int AvailabilityId { get; set; }
        public Availability? Availability { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        /// <summary>
        /// Session ID for guest users.
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// "Active", "Expired", "Converted", or "Released".
        /// </summary>
        public string Status { get; set; } = "Active";

        public int? BookingId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

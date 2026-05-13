using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    /// <summary>
    /// Real-time Availability - TÃ¬nh tráº¡ng cÃ²n chá»— real-time
    /// </summary>
    public class Availability
    {
        public int Id { get; set; }

        /// <summary>
        /// "Tour" or "Hotel"
        /// </summary>
        [Required]
        public string EntityType { get; set; }

        public int? TourId { get; set; }
        public Tour Tour { get; set; }

        public int? HotelId { get; set; }

        public int EntityId => TourId ?? HotelId ?? 0;
        public Hotel Hotel { get; set; }

        /// <summary>
        /// NgÃ y cá»¥ thá»ƒ
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Tá»•ng sá»‘ chá»—
        /// </summary>
        public int TotalCapacity { get; set; }

        /// <summary>
        /// Sá»‘ chá»— Ä‘Ã£ Ä‘áº·t
        /// </summary>
        public int BookedCapacity { get; set; }

        /// <summary>
        /// Sá»‘ chá»— cÃ²n láº¡i
        /// </summary>
        public int AvailableCapacity { get; set; }

        /// <summary>
        /// Sá»‘ chá»— Ä‘ang hold (pending bookings)
        /// </summary>
        public int HoldCapacity { get; set; }

        /// <summary>
        /// % occupancy
        /// </summary>
        public decimal OccupancyRate { get; set; }

        /// <summary>
        /// GiÃ¡ hiá»‡n táº¡i (cÃ³ thá»ƒ thay Ä‘á»•i theo demand)
        /// </summary>
        public decimal CurrentPrice { get; set; }

        /// <summary>
        /// GiÃ¡ gá»‘c
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// "Available", "Limited", "Full", "Closed"
        /// </summary>
        public string Status { get; set; } = "Available";

        /// <summary>
        /// CÃ³ cho phÃ©p overbooking khÃ´ng
        /// </summary>
        public bool AllowOverbooking { get; set; } = false;

        /// <summary>
        /// % overbooking tá»‘i Ä‘a
        /// </summary>
        public decimal MaxOverbookingPercentage { get; set; } = 10;

        /// <summary>
        /// Sá»‘ lÆ°á»£t xem trong 24h
        /// </summary>
        public int ViewsLast24Hours { get; set; }

        /// <summary>
        /// Sá»‘ booking trong 24h
        /// </summary>
        public int BookingsLast24Hours { get; set; }

        /// <summary>
        /// Demand level: "Low", "Medium", "High", "VeryHigh"
        /// </summary>
        public string DemandLevel { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Availability Block - KhÃ³a chá»— táº¡m thá»i
    /// </summary>
    public class AvailabilityBlock
    {
        public int Id { get; set; }

        public int AvailabilityId { get; set; }
        public Availability Availability { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }

        /// <summary>
        /// Session ID (cho guest users)
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Sá»‘ lÆ°á»£ng chá»— block
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Thá»i gian háº¿t háº¡n block
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// "Active", "Expired", "Converted", "Released"
        /// </summary>
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Booking ID náº¿u Ä‘Ã£ convert thÃ nh booking
        /// </summary>
        public int? BookingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}


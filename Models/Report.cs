using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    public class Report
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        /// <summary>
        /// "Revenue", "Booking", "User", "Tour", "Hotel"
        /// </summary>
        [Required]
        public string ReportType { get; set; }
        
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        
        /// <summary>
        /// JSON data chứa kết quả báo cáo
        /// </summary>
        public string ReportData { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }
        
        /// <summary>
        /// "Generated", "Scheduled", "Failed"
        /// </summary>
        public string Status { get; set; } = "Generated";
    }

    public class RevenueReportData
    {
        public decimal TotalRevenue { get; set; }
        public decimal TourRevenue { get; set; }
        public decimal HotelRevenue { get; set; }
        public int TotalBookings { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailyRevenue> DailyRevenues { get; set; } = new();
        public List<TopTour> TopTours { get; set; } = new();
        public List<TopHotel> TopHotels { get; set; } = new();
    }

    public class DailyRevenue
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
    }

    public class TopTour
    {
        public int TourId { get; set; }
        public string TourName { get; set; }
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
    }

    public class TopHotel
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
    }

    public class BookingReportData
    {
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CancelledBookings { get; set; }
        public decimal CancellationRate { get; set; }
        public List<BookingTrend> BookingTrends { get; set; } = new();
        public List<PopularDestination> PopularDestinations { get; set; } = new();
    }

    public class BookingTrend
    {
        public DateTime Date { get; set; }
        public int BookingCount { get; set; }
        public string Status { get; set; }
    }

    public class PopularDestination
    {
        public int DestinationId { get; set; }
        public string DestinationName { get; set; }
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
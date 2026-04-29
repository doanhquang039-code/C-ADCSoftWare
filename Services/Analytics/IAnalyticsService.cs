namespace WEBDULICH.Services.Analytics
{
    public interface IAnalyticsService
    {
        Task<DashboardMetrics> GetDashboardMetricsAsync(DateTime from, DateTime to);
        Task<List<RevenueData>> GetRevenueChartDataAsync(DateTime from, DateTime to, string groupBy = "day");
        Task<List<PopularTour>> GetPopularToursAsync(int top = 10);
        Task<List<CustomerSegment>> GetCustomerSegmentsAsync();
        Task<BookingTrends> GetBookingTrendsAsync(DateTime from, DateTime to);
        Task<List<ConversionFunnel>> GetConversionFunnelAsync(DateTime from, DateTime to);
    }

    public class DashboardMetrics
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueGrowth { get; set; }
        public int TotalBookings { get; set; }
        public double BookingGrowth { get; set; }
        public int TotalCustomers { get; set; }
        public double CustomerGrowth { get; set; }
        public double AverageOrderValue { get; set; }
        public double ConversionRate { get; set; }
        public int PendingBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
    }

    public class RevenueData
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Bookings { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class PopularTour
    {
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class CustomerSegment
    {
        public string SegmentName { get; set; } = string.Empty;
        public int CustomerCount { get; set; }
        public decimal TotalSpent { get; set; }
        public double AverageOrderValue { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class BookingTrends
    {
        public List<TrendData> Daily { get; set; } = new();
        public List<TrendData> Weekly { get; set; } = new();
        public List<TrendData> Monthly { get; set; } = new();
        public Dictionary<string, int> ByDestination { get; set; } = new();
        public Dictionary<string, int> ByCategory { get; set; } = new();
    }

    public class TrendData
    {
        public DateTime Period { get; set; }
        public int Count { get; set; }
        public decimal Value { get; set; }
    }

    public class ConversionFunnel
    {
        public string Stage { get; set; } = string.Empty;
        public int Count { get; set; }
        public double ConversionRate { get; set; }
        public double DropOffRate { get; set; }
    }
}

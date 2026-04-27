using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WEBDULICH.Models;
using System.Text;

namespace WEBDULICH.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ReportService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<RevenueReportData> GenerateRevenueReportAsync(DateTime fromDate, DateTime toDate)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.Hotel)
                .Where(b => b.CreatedAt >= fromDate && b.CreatedAt <= toDate && b.Status == "Confirmed")
                .ToListAsync();

            var orders = await _context.Orders
                .Include(o => o.Tour)
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate && o.Status == "Paid")
                .ToListAsync();

            var totalRevenue = bookings.Sum(b => b.TotalPrice) + orders.Sum(o => o.TotalPrice);
            var tourRevenue = bookings.Where(b => b.TourId.HasValue).Sum(b => b.TotalPrice) + orders.Sum(o => o.TotalPrice);
            var hotelRevenue = bookings.Where(b => b.HotelId.HasValue).Sum(b => b.TotalPrice);

            // Daily revenue
            var dailyRevenues = bookings.Concat(orders.Select(o => new Booking 
            { 
                CreatedAt = o.OrderDate, 
                TotalPrice = o.TotalPrice 
            }))
            .GroupBy(b => b.CreatedAt.Date)
            .Select(g => new DailyRevenue
            {
                Date = g.Key,
                Revenue = g.Sum(b => b.TotalPrice),
                BookingCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

            // Top tours
            var topTours = bookings.Where(b => b.TourId.HasValue)
                .Concat(orders.Select(o => new Booking 
                { 
                    TourId = o.TourId, 
                    Tour = o.Tour, 
                    TotalPrice = o.TotalPrice 
                }))
                .GroupBy(b => new { b.TourId, b.Tour?.Name })
                .Select(g => new TopTour
                {
                    TourId = g.Key.TourId ?? 0,
                    TourName = g.Key.Name ?? "Unknown",
                    Revenue = g.Sum(b => b.TotalPrice),
                    BookingCount = g.Count()
                })
                .OrderByDescending(t => t.Revenue)
                .Take(10)
                .ToList();

            // Top hotels
            var topHotels = bookings.Where(b => b.HotelId.HasValue)
                .GroupBy(b => new { b.HotelId, b.Hotel?.Name })
                .Select(g => new TopHotel
                {
                    HotelId = g.Key.HotelId ?? 0,
                    HotelName = g.Key.Name ?? "Unknown",
                    Revenue = g.Sum(b => b.TotalPrice),
                    BookingCount = g.Count()
                })
                .OrderByDescending(h => h.Revenue)
                .Take(10)
                .ToList();

            return new RevenueReportData
            {
                TotalRevenue = totalRevenue,
                TourRevenue = tourRevenue,
                HotelRevenue = hotelRevenue,
                TotalBookings = bookings.Count + orders.Count,
                AverageOrderValue = (bookings.Count + orders.Count) > 0 ? totalRevenue / (bookings.Count + orders.Count) : 0,
                DailyRevenues = dailyRevenues,
                TopTours = topTours,
                TopHotels = topHotels
            };
        }

        public async Task<BookingReportData> GenerateBookingReportAsync(DateTime fromDate, DateTime toDate)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Tour)
                    .ThenInclude(t => t.Destination)
                .Include(b => b.Hotel)
                .Where(b => b.CreatedAt >= fromDate && b.CreatedAt <= toDate)
                .ToListAsync();

            var totalBookings = bookings.Count;
            var confirmedBookings = bookings.Count(b => b.Status == "Confirmed");
            var pendingBookings = bookings.Count(b => b.Status == "Pending");
            var cancelledBookings = bookings.Count(b => b.Status == "Cancelled");

            var cancellationRate = totalBookings > 0 ? (decimal)cancelledBookings / totalBookings * 100 : 0;

            // Booking trends
            var bookingTrends = bookings
                .GroupBy(b => new { Date = b.CreatedAt.Date, b.Status })
                .Select(g => new BookingTrend
                {
                    Date = g.Key.Date,
                    Status = g.Key.Status,
                    BookingCount = g.Count()
                })
                .OrderBy(t => t.Date)
                .ToList();

            // Popular destinations
            var popularDestinations = bookings
                .Where(b => b.Tour?.Destination != null)
                .GroupBy(b => new { b.Tour.DestinationId, b.Tour.Destination.Name })
                .Select(g => new PopularDestination
                {
                    DestinationId = g.Key.DestinationId ?? 0,
                    DestinationName = g.Key.Name,
                    BookingCount = g.Count(),
                    Revenue = g.Sum(b => b.TotalPrice)
                })
                .OrderByDescending(d => d.BookingCount)
                .Take(10)
                .ToList();

            return new BookingReportData
            {
                TotalBookings = totalBookings,
                ConfirmedBookings = confirmedBookings,
                PendingBookings = pendingBookings,
                CancelledBookings = cancelledBookings,
                CancellationRate = cancellationRate,
                BookingTrends = bookingTrends,
                PopularDestinations = popularDestinations
            };
        }

        public async Task<List<Report>> GetReportsAsync()
        {
            return await _context.Reports
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Report> GetReportByIdAsync(int id)
        {
            return await _context.Reports.FindAsync(id);
        }

        public async Task<Report> SaveReportAsync(Report report)
        {
            if (report.Id == 0)
            {
                var currentUser = _currentUserService.GetCurrentUser();
                report.CreatedBy = currentUser?.Id ?? 0;
                _context.Reports.Add(report);
            }
            else
            {
                _context.Reports.Update(report);
            }

            await _context.SaveChangesAsync();
            return report;
        }

        public async Task DeleteReportAsync(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<byte[]> ExportReportToPdfAsync(int reportId)
        {
            var report = await GetReportByIdAsync(reportId);
            if (report == null) return null;

            // Simplified PDF generation - in real app, use a library like iTextSharp
            var content = $"Report: {report.Title}\nGenerated: {report.CreatedAt}\n\nData:\n{report.ReportData}";
            return Encoding.UTF8.GetBytes(content);
        }

        public async Task<byte[]> ExportReportToExcelAsync(int reportId)
        {
            var report = await GetReportByIdAsync(reportId);
            if (report == null) return null;

            // Simplified Excel generation - in real app, use a library like EPPlus
            var content = $"Report,{report.Title}\nGenerated,{report.CreatedAt}\n\nData\n{report.ReportData}";
            return Encoding.UTF8.GetBytes(content);
        }
    }
}
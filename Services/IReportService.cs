using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface IReportService
    {
        Task<RevenueReportData> GenerateRevenueReportAsync(DateTime fromDate, DateTime toDate);
        Task<BookingReportData> GenerateBookingReportAsync(DateTime fromDate, DateTime toDate);
        Task<List<Report>> GetReportsAsync();
        Task<Report> GetReportByIdAsync(int id);
        Task<Report> SaveReportAsync(Report report);
        Task DeleteReportAsync(int id);
        Task<byte[]> ExportReportToPdfAsync(int reportId);
        Task<byte[]> ExportReportToExcelAsync(int reportId);
    }
}
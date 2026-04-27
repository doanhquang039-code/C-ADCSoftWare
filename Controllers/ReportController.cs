using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var reports = await _reportService.GetReportsAsync();
            return View(reports);
        }

        public IActionResult Generate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateRevenue(DateTime fromDate, DateTime toDate, string title = "Revenue Report")
        {
            try
            {
                var reportData = await _reportService.GenerateRevenueReportAsync(fromDate, toDate);
                
                var report = new Report
                {
                    Title = title,
                    Description = $"Revenue report from {fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}",
                    ReportType = "Revenue",
                    FromDate = fromDate,
                    ToDate = toDate,
                    ReportData = JsonConvert.SerializeObject(reportData, Formatting.Indented),
                    Status = "Generated"
                };

                await _reportService.SaveReportAsync(report);

                TempData["Success"] = "Revenue report generated successfully!";
                return RedirectToAction("View", new { id = report.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating report: {ex.Message}";
                return RedirectToAction("Generate");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateBooking(DateTime fromDate, DateTime toDate, string title = "Booking Report")
        {
            try
            {
                var reportData = await _reportService.GenerateBookingReportAsync(fromDate, toDate);
                
                var report = new Report
                {
                    Title = title,
                    Description = $"Booking report from {fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}",
                    ReportType = "Booking",
                    FromDate = fromDate,
                    ToDate = toDate,
                    ReportData = JsonConvert.SerializeObject(reportData, Formatting.Indented),
                    Status = "Generated"
                };

                await _reportService.SaveReportAsync(report);

                TempData["Success"] = "Booking report generated successfully!";
                return RedirectToAction("View", new { id = report.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating report: {ex.Message}";
                return RedirectToAction("Generate");
            }
        }

        public async Task<IActionResult> View(int id)
        {
            var report = await _reportService.GetReportByIdAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            ViewBag.ReportData = report.ReportData;
            
            if (report.ReportType == "Revenue")
            {
                ViewBag.RevenueData = JsonConvert.DeserializeObject<RevenueReportData>(report.ReportData);
            }
            else if (report.ReportType == "Booking")
            {
                ViewBag.BookingData = JsonConvert.DeserializeObject<BookingReportData>(report.ReportData);
            }

            return View(report);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _reportService.DeleteReportAsync(id);
                TempData["Success"] = "Report deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting report: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ExportPdf(int id)
        {
            var pdfData = await _reportService.ExportReportToPdfAsync(id);
            if (pdfData == null)
            {
                return NotFound();
            }

            var report = await _reportService.GetReportByIdAsync(id);
            return File(pdfData, "application/pdf", $"{report.Title}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> ExportExcel(int id)
        {
            var excelData = await _reportService.ExportReportToExcelAsync(id);
            if (excelData == null)
            {
                return NotFound();
            }

            var report = await _reportService.GetReportByIdAsync(id);
            return File(excelData, "application/vnd.ms-excel", $"{report.Title}_{DateTime.Now:yyyyMMdd}.csv");
        }

        // API endpoints for charts
        [HttpGet]
        public async Task<IActionResult> GetRevenueChart(DateTime fromDate, DateTime toDate)
        {
            var reportData = await _reportService.GenerateRevenueReportAsync(fromDate, toDate);
            
            var chartData = new
            {
                labels = reportData.DailyRevenues.Select(d => d.Date.ToString("dd/MM")).ToArray(),
                datasets = new[]
                {
                    new
                    {
                        label = "Daily Revenue",
                        data = reportData.DailyRevenues.Select(d => d.Revenue).ToArray(),
                        backgroundColor = "rgba(54, 162, 235, 0.2)",
                        borderColor = "rgba(54, 162, 235, 1)",
                        borderWidth = 1
                    }
                }
            };

            return Json(chartData);
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingChart(DateTime fromDate, DateTime toDate)
        {
            var reportData = await _reportService.GenerateBookingReportAsync(fromDate, toDate);
            
            var chartData = new
            {
                labels = new[] { "Confirmed", "Pending", "Cancelled" },
                datasets = new[]
                {
                    new
                    {
                        data = new[] { reportData.ConfirmedBookings, reportData.PendingBookings, reportData.CancelledBookings },
                        backgroundColor = new[] { "#28a745", "#ffc107", "#dc3545" }
                    }
                }
            };

            return Json(chartData);
        }
    }
}
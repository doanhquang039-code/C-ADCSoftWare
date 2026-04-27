using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IOrderService _orderService;
        private readonly ITourService _tourService;
        private readonly IHotelService _hotelService;
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;

        public DashboardController(
            IBookingService bookingService,
            IOrderService orderService,
            ITourService tourService,
            IHotelService hotelService,
            IUserService userService,
            ICurrentUserService currentUserService)
        {
            _bookingService = bookingService;
            _orderService = orderService;
            _tourService = tourService;
            _hotelService = hotelService;
            _userService = userService;
            _currentUserService = currentUserService;
        }

        // GET: Dashboard (Admin/Manager only)
        public async Task<IActionResult> Index()
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || (!user.IsAdmin() && !user.IsManager())) return Forbid();

            // Get statistics
            var allBookings = await _bookingService.GetAllBookingsAsync();
            var allOrders = await _orderService.GetPaidOrdersAsync();
            var allTours = await _tourService.GetAllAsync();
            var allHotels = await _hotelService.GetAllAsync();

            // Calculate revenue
            var totalRevenue = allBookings.Where(b => b.Status == "Completed").Sum(b => b.TotalPrice);
            var monthlyRevenue = allBookings
                .Where(b => b.Status == "Completed" && b.CreatedAt.Month == DateTime.Now.Month)
                .Sum(b => b.TotalPrice);

            // Booking statistics
            var pendingBookings = allBookings.Count(b => b.Status == "Pending");
            var confirmedBookings = allBookings.Count(b => b.Status == "Confirmed");
            var completedBookings = allBookings.Count(b => b.Status == "Completed");
            var cancelledBookings = allBookings.Count(b => b.Status == "Cancelled");

            // Recent bookings
            var recentBookings = allBookings.OrderByDescending(b => b.CreatedAt).Take(10).ToList();

            // Popular tours
            var popularTours = allBookings
                .Where(b => b.TourId != null)
                .GroupBy(b => b.TourId)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { TourId = g.Key, BookingCount = g.Count() })
                .ToList();

            // User statistics
            var userStats = await _userService.GetUsersByRoleStatsAsync();
            var membershipStats = await _userService.GetUsersByMembershipStatsAsync();

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.TotalBookings = allBookings.Count;
            ViewBag.PendingBookings = pendingBookings;
            ViewBag.ConfirmedBookings = confirmedBookings;
            ViewBag.CompletedBookings = completedBookings;
            ViewBag.CancelledBookings = cancelledBookings;
            ViewBag.TotalTours = allTours.Count;
            ViewBag.TotalHotels = allHotels.Count;
            ViewBag.TotalUsers = await _userService.GetTotalUsersCountAsync();
            ViewBag.RecentBookings = recentBookings;
            ViewBag.PopularTours = popularTours;
            ViewBag.UserStats = userStats;
            ViewBag.MembershipStats = membershipStats;
            ViewBag.CurrentUserRole = user.Role;

            return View();
        }

        // GET: Dashboard/Bookings (Admin/Manager only)
        public async Task<IActionResult> Bookings()
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || (!user.IsAdmin() && !user.IsManager())) return Forbid();

            var bookings = await _bookingService.GetAllBookingsAsync();
            return View(bookings);
        }

        // POST: Dashboard/UpdateBookingStatus (Admin/Manager only)
        [HttpPost]
        public async Task<IActionResult> UpdateBookingStatus(int id, string status)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || (!user.IsAdmin() && !user.IsManager())) return Json(new { success = false });

            var result = await _bookingService.UpdateBookingStatusAsync(id, status);
            return Json(new { success = result });
        }

        // GET: Dashboard/Revenue
        public async Task<IActionResult> Revenue(int year, int month)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || user.Role != "Admin") return Forbid();

            var bookings = await _bookingService.GetAllBookingsAsync();
            
            var revenueData = bookings
                .Where(b => b.Status == "Completed" && 
                           b.CreatedAt.Year == year && 
                           b.CreatedAt.Month == month)
                .GroupBy(b => b.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(b => b.TotalPrice) })
                .OrderBy(x => x.Date)
                .ToList();

            return Json(revenueData);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService orderService;
        private readonly ICurrentUserService currentUserService;
        private readonly ApplicationDbContext db;

        public OrdersController(IOrderService orderService, ICurrentUserService currentUserService, ApplicationDbContext db)
        {
            this.orderService = orderService;
            this.currentUserService = currentUserService;
            this.db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTour(int tourId, int quantity, string paymentMethod)
        {
            var user = currentUserService.GetCurrentUser();
            if (user == null)
                return RedirectToAction("Login", "User");

            try
            {
                await orderService.CreateTourOrderAsync(user.Id, user.Email, tourId, quantity, paymentMethod);
                TempData["Success"] = "Đặt tour thành công!";
                return RedirectToAction("MyOrders");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Tour");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateHotel(int hotelId, int quantity, string paymentMethod)
        {
            var user = currentUserService.GetCurrentUser();
            if (user == null)
                return RedirectToAction("Login", "User");

            try
            {
                await orderService.CreateHotelOrderAsync(user.Id, user.Email, hotelId, quantity, paymentMethod);
                TempData["Success"] = "Đặt khách sạn thành công!";
                return RedirectToAction("MyOrders");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Hotel");
            }
        }

        [AuthenticatedOnly]
        public async Task<IActionResult> MyOrders(string? keyword, string? status,
            string? sortBy, string? sortDir, int page = 1, int pageSize = 10)
        {
            var user = currentUserService.GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "User");

            // Admin/Staff xem tất cả, User chỉ xem orders của mình
            int? userId = currentUserService.IsStaffOrAdmin() ? null : user.Id;

            var result = await orderService.GetPagedAsync(keyword, status, userId, sortBy, sortDir, page, pageSize);
            ViewBag.IsAdmin = currentUserService.IsStaffOrAdmin();
            return View(result);
        }

        public IActionResult FormTour(int tourId)
        {
            var user = currentUserService.GetCurrentUser();
            if (user == null)
                return RedirectToAction("Login", "User");

            var tour = db.Tours.Include(t => t.Destination).FirstOrDefault(t => t.Id == tourId);
            if (tour == null) return NotFound();

            ViewBag.Tour = tour;
            return View();
        }

        public IActionResult FormHotel(int hotelId)
        {
            var user = currentUserService.GetCurrentUser();
            if (user == null)
                return RedirectToAction("Login", "User");

            var hotel = db.Hotels.Include(h => h.Tour).FirstOrDefault(h => h.Id == hotelId);
            if (hotel == null) return NotFound();

            ViewBag.Hotel = hotel;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderCount()
        {
            var user = currentUserService.GetCurrentUser();
            if (user == null) return Json(new { count = 0 });

            var count = await orderService.GetOrderCountAsync(user.Id);
            return Json(new { count });
        }

        public async Task<IActionResult> Transfer(int id)
        {
            var order = await orderService.GetByIdAsync(id);
            if (order == null || order.Status != "Chưa thanh toán" || order.PaymentMethod != "Chuyển khoản")
                return RedirectToAction("MyOrders");

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmTransfer(int orderId, string email, DateTime departureDate)
        {
            var success = await orderService.ConfirmTransferAsync(orderId, email, departureDate);
            if (!success) return NotFound();

            HttpContext.Session.SetString("TransferSuccess", $"Đã xác nhận đơn hàng #{orderId}.");
            HttpContext.Session.SetString("ConfirmedEmail", email);
            HttpContext.Session.SetString("ConfirmedDate", departureDate.ToString("dd/MM/yyyy"));

            return RedirectToAction("MyOrders");
        }

        [StaffOrAdmin]
        public async Task<IActionResult> PaidOrders()
        {
            var paidOrders = await orderService.GetPaidOrdersAsync();
            return View(paidOrders);
        }
    }
}
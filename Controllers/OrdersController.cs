using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class OrdersController : Controller

    {
        private readonly ApplicationDbContext db5;
        public OrdersController(ApplicationDbContext db5)
        {
            this.db5 = db5;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateTour(int tourId, int quantity, string paymentMethod)
        {
            var user = HttpContext.Session.GetObject<User>("userLogin");
            if (user == null)
                return RedirectToAction("Login", "User");

            var tour = db5.Tours.FirstOrDefault(t => t.Id == tourId);
            if (tour == null || quantity <= 0 || tour.Quantity < quantity)
            {
                TempData["Error"] = "Không đủ chỗ hoặc dữ liệu không hợp lệ!";
                return RedirectToAction("Index", "Tour");
            }

            var order = new Orders
            {
                UserId = user.Id,
                TourId = tour.Id,
                Quantity = quantity,
                OrderDate = DateTime.Now,
                TotalPrice = quantity * tour.Price,
                Status = "Chưa thanh toán",
                PaymentMethod = paymentMethod,

                ConfirmedEmail = user.Email,
               
            };

            tour.Quantity -= quantity;

            db5.Orders.Add(order);
            db5.SaveChanges();

            TempData["Success"] = "Đặt tour thành công!";
            return RedirectToAction("MyOrders");
        }

        [HttpPost]
        public IActionResult CreateHotel(int hotelId, int quantity, string paymentMethod)
        {
            var user = HttpContext.Session.GetObject<User>("userLogin");
            if (user == null)
                return RedirectToAction("Login", "User");

            var hotel = db5.Hotels.FirstOrDefault(h => h.Id == hotelId);
            if (hotel == null || quantity <= 0 || hotel.Quantity < quantity)
            {
                TempData["Error"] = "Không đủ phòng hoặc dữ liệu không hợp lệ!";
                return RedirectToAction("Index", "Hotel");
            }

            var order = new Orders
            {
                UserId = user.Id,
                HotelId = hotel.Id,
                TourId = hotel.TourId,
                Quantity = quantity,
                OrderDate = DateTime.Now,
                TotalPrice = quantity * hotel.Price,
                Status = "Chưa thanh toán",
                PaymentMethod = paymentMethod,


                ConfirmedEmail = user.Email,
            };

            hotel.Quantity -= quantity;
            db5.Orders.Add(order);
            db5.SaveChanges();

            TempData["Success"] = "Đặt khách sạn thành công!";
            return RedirectToAction("MyOrders");
        }


        public IActionResult MyOrders()
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var orders = db5.Orders
                .Include(o => o.Tour)
                .Include(o => o.Hotel)
                .Where(o => o.User.Id == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        public IActionResult FormTour(int tourId)
        {
            var user = HttpContext.Session.GetObject<User>("userLogin");
            if (user == null)
                return RedirectToAction("Login", "User");
            var tour = db5.Tours
    .Include(t => t.Destination)
    .FirstOrDefault(t => t.Id == tourId);

            if (tour == null) return NotFound();

            ViewBag.Tour = tour;
            return View();
        }
        public IActionResult FormHotel(int hotelId)
        {
            var user = HttpContext.Session.GetObject<User>("userLogin");
            if (user == null)
                return RedirectToAction("Login", "User");
            var hotel = db5.Hotels.Include(h => h.Tour).FirstOrDefault(h => h.Id == hotelId);
            if (hotel == null) return NotFound();

            ViewBag.Hotel = hotel;
            return View();
        }

        
        [HttpGet]
        public IActionResult GetOrderCount()
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null) return Json(new { count = 0 });

            var count = db5.Orders.Count(o => o.UserId == userId);
            return Json(new { count });
        }

        public IActionResult Transfer(int id)
        {
            var order = db5.Orders
                .Include(o => o.Tour)
                .Include(o => o.Hotel)
                .FirstOrDefault(o => o.Id == id);

            if (order == null || order.Status != "Chưa thanh toán" || order.PaymentMethod != "Chuyển khoản")
                return RedirectToAction("MyOrders");

            return View(order);
        }
        [HttpPost]
        public IActionResult ConfirmTransfer(int orderId, string email, DateTime departureDate)
        {
            var order = db5.Orders
                .Include(o => o.Tour)
                .Include(o => o.Hotel)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null) 
                return NotFound();

            order.Status = "Đã thanh toán";
            order.OrderDate = DateTime.Now;
            order.ConfirmedEmail = email;
            order.DepartureDate = departureDate; 
            db5.SaveChanges();

            HttpContext.Session.SetString("TransferSuccess", $"Đã xác nhận đơn hàng #{order.Id}.");
            HttpContext.Session.SetString("ConfirmedEmail", email);
            HttpContext.Session.SetString("ConfirmedDate", departureDate.ToString("dd/MM/yyyy"));


            return RedirectToAction("MyOrders");
        }

        public IActionResult PaidOrders()
        {
            var paidOrders = db5.Orders
                .Include(o => o.Tour)
                .Include(o => o.Hotel)
                .Include(o => o.User) // nếu cần hiển thị email từ User
                .Where(o => o.Status == "Đã thanh toán")
                .ToList();

            return View(paidOrders);
        }

    }
}
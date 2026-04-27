using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ITourService _tourService;
        private readonly IHotelService _hotelService;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly ICouponService _couponService;

        public BookingController(
            IBookingService bookingService,
            ITourService tourService,
            IHotelService hotelService,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            ICouponService couponService)
        {
            _bookingService = bookingService;
            _tourService = tourService;
            _hotelService = hotelService;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _couponService = couponService;
        }

        // GET: Booking/Tour/5
        public async Task<IActionResult> Tour(int id)
        {
            var tour = await _tourService.GetByIdAsync(id);
            if (tour == null) return NotFound();

            ViewBag.Tour = tour;
            return View();
        }

        // GET: Booking/Hotel/5
        public async Task<IActionResult> Hotel(int id)
        {
            var hotel = await _hotelService.GetByIdAsync(id);
            if (hotel == null) return NotFound();

            ViewBag.Hotel = hotel;
            return View();
        }

        // POST: Booking/CreateTourBooking
        [HttpPost]
        public async Task<IActionResult> CreateTourBooking(int tourId, DateTime startDate, int adults, int children, string specialRequests, string couponCode)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "User");

            // Check availability
            var available = await _bookingService.CheckAvailabilityAsync("Tour", tourId, startDate, null);
            if (!available)
            {
                TempData["Error"] = "Tour đã hết chỗ cho ngày này";
                return RedirectToAction("Tour", new { id = tourId });
            }

            // Calculate price
            var totalPrice = await _bookingService.CalculateTotalPriceAsync("Tour", tourId, adults, children, 1, startDate, null);

            // Apply coupon if provided
            decimal discountAmount = 0;
            if (!string.IsNullOrEmpty(couponCode))
            {
                var isValid = await _couponService.ValidateCouponAsync(couponCode, totalPrice);
                if (isValid)
                {
                    discountAmount = await _couponService.CalculateDiscountAsync(couponCode, totalPrice);
                    await _couponService.ApplyCouponAsync(couponCode);
                }
            }

            var booking = new Booking
            {
                UserId = user.Id,
                BookingType = "Tour",
                TourId = tourId,
                StartDate = startDate,
                Adults = adults,
                Children = children,
                TotalPrice = totalPrice - discountAmount,
                SpecialRequests = specialRequests,
                AppliedCouponCode = couponCode,
                DiscountAmount = discountAmount,
                Status = "Pending"
            };

            await _bookingService.CreateBookingAsync(booking);

            // Send notification
            await _notificationService.CreateNotificationAsync(
                user.Id,
                "Đặt tour thành công",
                $"Bạn đã đặt tour thành công. Mã booking: #{booking.Id}",
                "Booking",
                $"/Booking/Details/{booking.Id}"
            );

            TempData["Success"] = "Đặt tour thành công!";
            return RedirectToAction("MyBookings");
        }

        // POST: Booking/CreateHotelBooking
        [HttpPost]
        public async Task<IActionResult> CreateHotelBooking(int hotelId, DateTime startDate, DateTime endDate, int rooms, string specialRequests, string couponCode)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "User");

            // Check availability
            var available = await _bookingService.CheckAvailabilityAsync("Hotel", hotelId, startDate, endDate);
            if (!available)
            {
                TempData["Error"] = "Khách sạn đã hết phòng cho ngày này";
                return RedirectToAction("Hotel", new { id = hotelId });
            }

            // Calculate price
            var totalPrice = await _bookingService.CalculateTotalPriceAsync("Hotel", hotelId, 0, 0, rooms, startDate, endDate);

            // Apply coupon
            decimal discountAmount = 0;
            if (!string.IsNullOrEmpty(couponCode))
            {
                var isValid = await _couponService.ValidateCouponAsync(couponCode, totalPrice);
                if (isValid)
                {
                    discountAmount = await _couponService.CalculateDiscountAsync(couponCode, totalPrice);
                    await _couponService.ApplyCouponAsync(couponCode);
                }
            }

            var booking = new Booking
            {
                UserId = user.Id,
                BookingType = "Hotel",
                HotelId = hotelId,
                StartDate = startDate,
                EndDate = endDate,
                Rooms = rooms,
                TotalPrice = totalPrice - discountAmount,
                SpecialRequests = specialRequests,
                AppliedCouponCode = couponCode,
                DiscountAmount = discountAmount,
                Status = "Pending"
            };

            await _bookingService.CreateBookingAsync(booking);

            // Send notification
            await _notificationService.CreateNotificationAsync(
                user.Id,
                "Đặt khách sạn thành công",
                $"Bạn đã đặt khách sạn thành công. Mã booking: #{booking.Id}",
                "Booking",
                $"/Booking/Details/{booking.Id}"
            );

            TempData["Success"] = "Đặt khách sạn thành công!";
            return RedirectToAction("MyBookings");
        }

        // GET: Booking/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "User");

            var bookings = await _bookingService.GetUserBookingsAsync(user.Id);
            return View(bookings);
        }

        // GET: Booking/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound();

            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || booking.UserId != user.Id)
            {
                return Forbid();
            }

            return View(booking);
        }

        // POST: Booking/Cancel/5
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound();

            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || booking.UserId != user.Id)
            {
                return Forbid();
            }

            await _bookingService.CancelBookingAsync(id);

            // Send notification
            await _notificationService.CreateNotificationAsync(
                user.Id,
                "Hủy booking thành công",
                $"Booking #{id} đã được hủy",
                "Booking",
                $"/Booking/Details/{id}"
            );

            TempData["Success"] = "Hủy booking thành công!";
            return RedirectToAction("MyBookings");
        }

        // API: Check availability
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(string type, int itemId, DateTime startDate, DateTime? endDate)
        {
            var available = await _bookingService.CheckAvailabilityAsync(type, itemId, startDate, endDate);
            return Json(new { available });
        }

        // API: Calculate price
        [HttpGet]
        public async Task<IActionResult> CalculatePrice(string type, int itemId, int adults, int children, int rooms, DateTime startDate, DateTime? endDate, string couponCode)
        {
            var totalPrice = await _bookingService.CalculateTotalPriceAsync(type, itemId, adults, children, rooms, startDate, endDate);
            
            decimal discountAmount = 0;
            if (!string.IsNullOrEmpty(couponCode))
            {
                var isValid = await _couponService.ValidateCouponAsync(couponCode, totalPrice);
                if (isValid)
                {
                    discountAmount = await _couponService.CalculateDiscountAsync(couponCode, totalPrice);
                }
            }

            return Json(new 
            { 
                totalPrice, 
                discountAmount, 
                finalPrice = totalPrice - discountAmount 
            });
        }
    }
}

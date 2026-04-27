using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class CouponController : Controller
    {
        private readonly ICouponService _couponService;
        private readonly ICurrentUserService _currentUserService;

        public CouponController(ICouponService couponService, ICurrentUserService currentUserService)
        {
            _couponService = couponService;
            _currentUserService = currentUserService;
        }

        // GET: Coupon (Public - show active coupons)
        public async Task<IActionResult> Index()
        {
            var coupons = await _couponService.GetActiveCouponsAsync();
            return View(coupons);
        }

        // POST: Coupon/Validate
        [HttpPost]
        public async Task<IActionResult> Validate(string code, decimal orderAmount)
        {
            var isValid = await _couponService.ValidateCouponAsync(code, orderAmount);
            if (!isValid)
            {
                return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã hết hạn" });
            }

            var discount = await _couponService.CalculateDiscountAsync(code, orderAmount);
            return Json(new { success = true, discount, finalAmount = orderAmount - discount });
        }

        // Admin/Manager: GET: Coupon/Manage
        public async Task<IActionResult> Manage()
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || (!user.IsAdmin() && !user.IsManager())) return Forbid();

            var coupons = await _couponService.GetActiveCouponsAsync();
            return View(coupons);
        }

        // Admin/Manager: GET: Coupon/Create
        public async Task<IActionResult> Create()
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || (!user.IsAdmin() && !user.IsManager())) return Forbid();

            return View();
        }

        // Admin/Manager: POST: Coupon/Create
        [HttpPost]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || (!user.IsAdmin() && !user.IsManager())) return Forbid();

            if (!ModelState.IsValid) return View(coupon);

            await _couponService.CreateCouponAsync(coupon);
            TempData["Success"] = "Tạo mã giảm giá thành công!";
            return RedirectToAction("Manage");
        }

        // Admin/Manager: GET: Coupon/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || (!user.IsAdmin() && !user.IsManager())) return Forbid();

            var coupon = await _couponService.GetCouponByCodeAsync(id.ToString());
            if (coupon == null) return NotFound();

            return View(coupon);
        }

        // Admin/Manager: POST: Coupon/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Coupon coupon)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || (!user.IsAdmin() && !user.IsManager())) return Forbid();

            if (!ModelState.IsValid) return View(coupon);

            coupon.Id = id;
            await _couponService.UpdateCouponAsync(coupon);
            TempData["Success"] = "Cập nhật mã giảm giá thành công!";
            return RedirectToAction("Manage");
        }

        // Admin only: POST: Coupon/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || !user.IsAdmin()) return Forbid();

            await _couponService.DeleteCouponAsync(id);
            TempData["Success"] = "Xóa mã giảm giá thành công!";
            return RedirectToAction("Manage");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;
        private readonly ICurrentUserService _currentUserService;

        public WishlistController(IWishlistService wishlistService, ICurrentUserService currentUserService)
        {
            _wishlistService = wishlistService;
            _currentUserService = currentUserService;
        }

        // GET: Wishlist
        public async Task<IActionResult> Index()
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "User");

            var tours = await _wishlistService.GetWishlistToursAsync(user.Id);
            var hotels = await _wishlistService.GetWishlistHotelsAsync(user.Id);

            ViewBag.Tours = tours;
            ViewBag.Hotels = hotels;
            return View();
        }

        // POST: Wishlist/Add
        [HttpPost]
        public async Task<IActionResult> Add(string itemType, int itemId)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null) return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var result = await _wishlistService.AddToWishlistAsync(user.Id, itemType, itemId);
            return Json(new { success = result, message = result ? "Đã thêm vào yêu thích" : "Đã có trong danh sách yêu thích" });
        }

        // POST: Wishlist/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(string itemType, int itemId)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null) return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var result = await _wishlistService.RemoveFromWishlistAsync(user.Id, itemType, itemId);
            return Json(new { success = result, message = result ? "Đã xóa khỏi yêu thích" : "Không tìm thấy" });
        }

        // GET: Wishlist/Check
        [HttpGet]
        public async Task<IActionResult> Check(string itemType, int itemId)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null) return Json(new { inWishlist = false });

            var inWishlist = await _wishlistService.IsInWishlistAsync(user.Id, itemType, itemId);
            return Json(new { inWishlist });
        }
    }
}

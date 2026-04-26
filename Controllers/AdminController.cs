using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Helpers;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    [AdminOnly]
    public class AdminController : Controller
    {
        private readonly IUserService userService;

        public AdminController(IUserService userService)
        {
            this.userService = userService;
        }

        /// <summary>
        /// Dashboard — thống kê tổng quan
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var stats = await userService.GetDashboardStatsAsync();
            return View(stats);
        }

        /// <summary>
        /// Quản lý user — danh sách, search, sort, phân quyền
        /// </summary>
        public async Task<IActionResult> Users(string? keyword, string? role, bool? isActive,
            string? sortBy, string? sortDir, int page = 1, int pageSize = 10)
        {
            var result = await userService.GetPagedAsync(keyword, role, isActive, sortBy, sortDir, page, pageSize);
            return View(result);
        }

        /// <summary>
        /// Đổi role cho user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangeRole(int userId, string newRole)
        {
            await userService.ChangeRoleAsync(userId, newRole);
            TempData["Success"] = "Đã cập nhật quyền thành công!";
            return RedirectToAction(nameof(Users));
        }

        /// <summary>
        /// Khóa/mở khóa tài khoản
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int userId)
        {
            await userService.ToggleActiveAsync(userId);
            TempData["Success"] = "Đã cập nhật trạng thái tài khoản!";
            return RedirectToAction(nameof(Users));
        }
    }
}

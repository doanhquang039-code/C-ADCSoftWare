using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    [Route("manager")]
    public class ManagerController : Controller
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;

        public ManagerController(IUserService userService, ICurrentUserService currentUserService)
        {
            _userService = userService;
            _currentUserService = currentUserService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var stats = await _userService.GetDashboardStatsAsync();
            return View("~/Views/Manager/Index.cshtml", stats);
        }
        
        // Các tính năng Quản lý tour, booking sẽ được gọi từ các Controller tương ứng (giới hạn role)
    }
}

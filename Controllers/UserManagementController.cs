using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class UserManagementController : Controller
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;

        public UserManagementController(
            IUserService userService,
            ICurrentUserService currentUserService,
            INotificationService notificationService)
        {
            _userService = userService;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
        }

        // GET: UserManagement (Admin/Manager only)
        public async Task<IActionResult> Index(string keyword, string role, bool? isActive, int page = 1)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsStaffOrAdmin()) return Forbid();

            var users = await _userService.GetPagedAsync(keyword, role, isActive, "created", "desc", page, 20);
            
            ViewBag.Roles = new[] { "Admin", "Manager", "Hiring", "User" };
            ViewBag.CurrentUserRole = currentUser.Role;
            
            return View(users);
        }

        // GET: UserManagement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsStaffOrAdmin()) return Forbid();

            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // GET: UserManagement/Create (Admin only)
        public async Task<IActionResult> Create()
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsAdmin()) return Forbid();

            ViewBag.Roles = new[] { "Admin", "Manager", "Hiring", "User" };
            return View();
        }

        // POST: UserManagement/Create
        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsAdmin()) return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new[] { "Admin", "Manager", "Hiring", "User" };
                return View(user);
            }

            var success = await _userService.RegisterAsync(user);
            if (!success)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
                ViewBag.Roles = new[] { "Admin", "Manager", "Hiring", "User" };
                return View(user);
            }

            TempData["Success"] = "Tạo tài khoản thành công!";
            return RedirectToAction("Index");
        }

        // GET: UserManagement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsStaffOrAdmin()) return Forbid();

            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();

            // Only admin can edit other admins
            if (user.IsAdmin() && !currentUser.IsAdmin()) return Forbid();

            ViewBag.Roles = new[] { "Admin", "Manager", "Hiring", "User" };
            ViewBag.MembershipTiers = new[] { "Bronze", "Silver", "Gold", "Platinum" };
            return View(user);
        }

        // POST: UserManagement/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, User user)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsStaffOrAdmin()) return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new[] { "Admin", "Manager", "Hiring", "User" };
                ViewBag.MembershipTiers = new[] { "Bronze", "Silver", "Gold", "Platinum" };
                return View(user);
            }

            var existingUser = await _userService.GetByIdAsync(id);
            if (existingUser == null) return NotFound();

            // Only admin can edit other admins
            if (existingUser.IsAdmin() && !currentUser.IsAdmin()) return Forbid();

            await _userService.UpdateProfileAsync(id, user);
            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Details", new { id });
        }

        // POST: UserManagement/ChangeRole
        [HttpPost]
        public async Task<IActionResult> ChangeRole(int userId, string newRole)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsAdmin()) return Json(new { success = false, message = "Không có quyền" });

            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy user" });

            var success = await _userService.ChangeRoleAsync(userId, newRole);
            if (success)
            {
                // Send notification
                await _notificationService.CreateNotificationAsync(
                    userId,
                    "Thay đổi quyền",
                    $"Quyền của bạn đã được thay đổi thành {newRole}",
                    "System"
                );
            }

            return Json(new { success, message = success ? "Thay đổi quyền thành công" : "Thay đổi quyền thất bại" });
        }

        // POST: UserManagement/ToggleActive
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int userId)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsStaffOrAdmin()) return Json(new { success = false });

            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return Json(new { success = false });

            // Cannot deactivate yourself
            if (user.Id == currentUser.Id) return Json(new { success = false, message = "Không thể khóa tài khoản của chính mình" });

            // Only admin can deactivate other admins
            if (user.IsAdmin() && !currentUser.IsAdmin()) return Json(new { success = false, message = "Không có quyền" });

            var success = await _userService.ToggleActiveAsync(userId);
            if (success)
            {
                var status = user.IsActive ? "kích hoạt" : "khóa";
                await _notificationService.CreateNotificationAsync(
                    userId,
                    "Thay đổi trạng thái tài khoản",
                    $"Tài khoản của bạn đã được {status}",
                    "System"
                );
            }

            return Json(new { success });
        }

        // POST: UserManagement/UpdateLoyaltyPoints
        [HttpPost]
        public async Task<IActionResult> UpdateLoyaltyPoints(int userId, int points)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsStaffOrAdmin()) return Json(new { success = false });

            var success = await _userService.UpdateLoyaltyPointsAsync(userId, points);
            if (success)
            {
                await _notificationService.CreateNotificationAsync(
                    userId,
                    "Cập nhật điểm tích lũy",
                    $"Bạn đã được {(points > 0 ? "cộng" : "trừ")} {Math.Abs(points)} điểm",
                    "System"
                );
            }

            return Json(new { success });
        }

        // POST: UserManagement/VerifyEmail
        [HttpPost]
        public async Task<IActionResult> VerifyEmail(int userId)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsStaffOrAdmin()) return Json(new { success = false });

            var success = await _userService.VerifyEmailAsync(userId);
            return Json(new { success });
        }

        // POST: UserManagement/VerifyPhone
        [HttpPost]
        public async Task<IActionResult> VerifyPhone(int userId)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsStaffOrAdmin()) return Json(new { success = false });

            var success = await _userService.VerifyPhoneAsync(userId);
            return Json(new { success });
        }

        // GET: UserManagement/Stats
        public async Task<IActionResult> Stats()
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsStaffOrAdmin()) return Forbid();

            var roleStats = await _userService.GetUsersByRoleStatsAsync();
            var membershipStats = await _userService.GetUsersByMembershipStatsAsync();
            var totalUsers = await _userService.GetTotalUsersCountAsync();
            var activeUsers = await _userService.GetActiveUsersCountAsync();

            ViewBag.RoleStats = roleStats;
            ViewBag.MembershipStats = membershipStats;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.ActiveUsers = activeUsers;

            return View();
        }

        // GET: UserManagement/Admins
        public async Task<IActionResult> Admins()
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsAdmin()) return Forbid();

            var admins = await _userService.GetAdminsAsync();
            return View(admins);
        }

        // GET: UserManagement/Managers
        public async Task<IActionResult> Managers()
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsStaffOrAdmin()) return Forbid();

            var managers = await _userService.GetManagersAsync();
            return View(managers);
        }

        // GET: UserManagement/HiringStaff
        public async Task<IActionResult> HiringStaff()
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsStaffOrAdmin()) return Forbid();

            var hiringStaff = await _userService.GetHiringStaffAsync();
            return View(hiringStaff);
        }
    }
}
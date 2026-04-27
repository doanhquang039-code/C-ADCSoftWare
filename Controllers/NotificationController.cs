using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        public NotificationController(INotificationService notificationService, ICurrentUserService currentUserService)
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }

        // GET: Notification
        public async Task<IActionResult> Index()
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "User");

            var notifications = await _notificationService.GetUserNotificationsAsync(user.Id);
            return View(notifications);
        }

        // GET: Notification/Unread
        public async Task<IActionResult> Unread()
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null) return Json(new { count = 0, notifications = new List<object>() });

            var notifications = await _notificationService.GetUserNotificationsAsync(user.Id, true);
            var count = await _notificationService.GetUnreadCountAsync(user.Id);

            return Json(new { count, notifications });
        }

        // POST: Notification/MarkAsRead/5
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            return Json(new { success = result });
        }

        // POST: Notification/MarkAllAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null) return Json(new { success = false });

            var result = await _notificationService.MarkAllAsReadAsync(user.Id);
            return Json(new { success = result });
        }

        // POST: Notification/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _notificationService.DeleteNotificationAsync(id);
            return Json(new { success = result });
        }
    }
}

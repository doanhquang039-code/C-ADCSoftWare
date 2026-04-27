using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly ICurrentUserService _currentUserService;

        public ChatController(IChatService chatService, ICurrentUserService currentUserService)
        {
            _chatService = chatService;
            _currentUserService = currentUserService;
        }

        // GET: Chat
        public async Task<IActionResult> Index()
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            string sessionId = user != null ? $"user_{user.Id}" : $"guest_{Guid.NewGuid()}";
            
            ViewBag.SessionId = sessionId;
            return View();
        }

        // GET: Chat/Messages
        public async Task<IActionResult> Messages(string sessionId)
        {
            var messages = await _chatService.GetSessionMessagesAsync(sessionId);
            return Json(messages);
        }

        // POST: Chat/Send
        [HttpPost]
        public async Task<IActionResult> Send(string message, string sessionId)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            
            var chatMessage = await _chatService.SendMessageAsync(
                user?.Id,
                "User",
                message,
                sessionId
            );

            return Json(new { success = true, message = chatMessage });
        }

        // Admin: GET: Chat/Admin
        public async Task<IActionResult> Admin()
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || user.Role != "Admin") return Forbid();

            var sessions = await _chatService.GetActiveSessionsAsync();
            return View(sessions);
        }

        // Admin: GET: Chat/AdminSession
        public async Task<IActionResult> AdminSession(string sessionId)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || user.Role != "Admin") return Forbid();

            var messages = await _chatService.GetSessionMessagesAsync(sessionId);
            ViewBag.SessionId = sessionId;
            return View(messages);
        }

        // Admin: POST: Chat/AdminReply
        [HttpPost]
        public async Task<IActionResult> AdminReply(string message, string sessionId)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || user.Role != "Admin") return Json(new { success = false });

            var chatMessage = await _chatService.SendMessageAsync(
                null,
                "Admin",
                message,
                sessionId,
                user.Id
            );

            return Json(new { success = true, message = chatMessage });
        }
    }
}

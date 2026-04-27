using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;

        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ChatMessage> SendMessageAsync(int? userId, string senderType, string message, string sessionId, int? adminId = null)
        {
            var chatMessage = new ChatMessage
            {
                UserId = userId,
                SenderType = senderType,
                Message = message,
                SessionId = sessionId,
                AdminId = adminId,
                CreatedAt = DateTime.Now
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();
            return chatMessage;
        }

        public async Task<List<ChatMessage>> GetSessionMessagesAsync(string sessionId)
        {
            return await _context.ChatMessages
                .Include(c => c.User)
                .Where(c => c.SessionId == sessionId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<string>> GetActiveSessionsAsync()
        {
            return await _context.ChatMessages
                .Select(c => c.SessionId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(int messageId)
        {
            var message = await _context.ChatMessages.FindAsync(messageId);
            if (message == null) return false;

            message.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(string sessionId)
        {
            return await _context.ChatMessages
                .CountAsync(c => c.SessionId == sessionId && !c.IsRead);
        }
    }
}

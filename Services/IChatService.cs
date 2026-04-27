using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface IChatService
    {
        Task<ChatMessage> SendMessageAsync(int? userId, string senderType, string message, string sessionId, int? adminId = null);
        Task<List<ChatMessage>> GetSessionMessagesAsync(string sessionId);
        Task<List<string>> GetActiveSessionsAsync();
        Task<bool> MarkAsReadAsync(int messageId);
        Task<int> GetUnreadCountAsync(string sessionId);
    }
}

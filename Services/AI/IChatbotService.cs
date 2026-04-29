namespace WEBDULICH.Services.AI
{
    public interface IChatbotService
    {
        Task<ChatbotResponse> GetResponseAsync(string message, string userId, string? conversationId = null);
        Task<List<ChatMessage>> GetConversationHistoryAsync(string conversationId);
        Task<bool> ClearConversationAsync(string conversationId);
        Task<List<string>> GetSuggestedQuestionsAsync();
        Task<ChatbotAnalytics> GetAnalyticsAsync(DateTime from, DateTime to);
    }

    public class ChatbotResponse
    {
        public string Message { get; set; } = string.Empty;
        public string ConversationId { get; set; } = string.Empty;
        public List<string> SuggestedActions { get; set; } = new();
        public ChatbotIntent Intent { get; set; } = ChatbotIntent.Unknown;
        public double Confidence { get; set; }
        public Dictionary<string, object> Entities { get; set; } = new();
        public List<QuickReply> QuickReplies { get; set; } = new();
    }

    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ConversationId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsBot { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public ChatbotIntent Intent { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class QuickReply
    {
        public string Text { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class ChatbotAnalytics
    {
        public int TotalConversations { get; set; }
        public int TotalMessages { get; set; }
        public double AverageResponseTime { get; set; }
        public Dictionary<ChatbotIntent, int> IntentDistribution { get; set; } = new();
        public double AverageConfidence { get; set; }
        public int ResolvedQueries { get; set; }
        public int EscalatedToHuman { get; set; }
    }

    public enum ChatbotIntent
    {
        Unknown,
        Greeting,
        Farewell,
        SearchTour,
        BookTour,
        CancelBooking,
        PaymentInquiry,
        PriceInquiry,
        AvailabilityCheck,
        TourDetails,
        ContactSupport,
        Complaint,
        Feedback,
        FAQ,
        LocationInfo,
        WeatherInfo,
        Recommendation
    }
}

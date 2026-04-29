using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WEBDULICH.Services.AI
{
    public class ChatbotService : IChatbotService
    {
        private readonly ILogger<ChatbotService> _logger;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;

        public ChatbotService(
            ILogger<ChatbotService> logger,
            IDistributedCache cache,
            IConfiguration configuration)
        {
            _logger = logger;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task<ChatbotResponse> GetResponseAsync(string message, string userId, string? conversationId = null)
        {
            try
            {
                conversationId ??= Guid.NewGuid().ToString();
                
                // Analyze intent
                var intent = AnalyzeIntent(message);
                var confidence = CalculateConfidence(message, intent);
                var entities = ExtractEntities(message);

                // Save message to history
                await SaveMessageAsync(conversationId, userId, message, false, intent);

                // Generate response based on intent
                var responseMessage = await GenerateResponseAsync(intent, entities, message);
                var suggestedActions = GetSuggestedActions(intent);
                var quickReplies = GetQuickReplies(intent);

                // Save bot response
                await SaveMessageAsync(conversationId, "bot", responseMessage, true, intent);

                _logger.LogInformation($"Chatbot response generated for user {userId}, intent: {intent}");

                return new ChatbotResponse
                {
                    Message = responseMessage,
                    ConversationId = conversationId,
                    Intent = intent,
                    Confidence = confidence,
                    Entities = entities,
                    SuggestedActions = suggestedActions,
                    QuickReplies = quickReplies
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating chatbot response");
                return new ChatbotResponse
                {
                    Message = "Xin lỗi, tôi đang gặp sự cố. Vui lòng thử lại sau.",
                    ConversationId = conversationId ?? Guid.NewGuid().ToString(),
                    Intent = ChatbotIntent.Unknown,
                    Confidence = 0
                };
            }
        }

        private ChatbotIntent AnalyzeIntent(string message)
        {
            message = message.ToLower().Trim();

            // Greeting patterns
            if (Regex.IsMatch(message, @"\b(xin chào|chào|hello|hi|hey)\b"))
                return ChatbotIntent.Greeting;

            // Farewell patterns
            if (Regex.IsMatch(message, @"\b(tạm biệt|bye|goodbye|cảm ơn|thank)\b"))
                return ChatbotIntent.Farewell;

            // Search tour patterns
            if (Regex.IsMatch(message, @"\b(tìm|search|tour|du lịch|đi|travel)\b"))
                return ChatbotIntent.SearchTour;

            // Booking patterns
            if (Regex.IsMatch(message, @"\b(đặt|book|booking|đặt tour|đặt vé)\b"))
                return ChatbotIntent.BookTour;

            // Cancel booking patterns
            if (Regex.IsMatch(message, @"\b(hủy|cancel|hủy tour|hủy đặt)\b"))
                return ChatbotIntent.CancelBooking;

            // Payment inquiry patterns
            if (Regex.IsMatch(message, @"\b(thanh toán|payment|pay|trả tiền|giá)\b"))
                return ChatbotIntent.PaymentInquiry;

            // Price inquiry patterns
            if (Regex.IsMatch(message, @"\b(giá|price|cost|bao nhiêu|phí)\b"))
                return ChatbotIntent.PriceInquiry;

            // Tour details patterns
            if (Regex.IsMatch(message, @"\b(chi tiết|details|thông tin|info|information)\b"))
                return ChatbotIntent.TourDetails;

            // Contact support patterns
            if (Regex.IsMatch(message, @"\b(liên hệ|contact|support|hỗ trợ|help)\b"))
                return ChatbotIntent.ContactSupport;

            // Recommendation patterns
            if (Regex.IsMatch(message, @"\b(gợi ý|recommend|suggestion|đề xuất)\b"))
                return ChatbotIntent.Recommendation;

            return ChatbotIntent.Unknown;
        }

        private double CalculateConfidence(string message, ChatbotIntent intent)
        {
            // Simple confidence calculation based on keyword matches
            if (intent == ChatbotIntent.Unknown)
                return 0.3;

            return 0.85; // High confidence for matched intents
        }

        private Dictionary<string, object> ExtractEntities(string message)
        {
            var entities = new Dictionary<string, object>();

            // Extract location
            var locationMatch = Regex.Match(message, @"\b(Hà Nội|Sài Gòn|Đà Nẵng|Nha Trang|Phú Quốc|Hạ Long|Huế|Hội An)\b", RegexOptions.IgnoreCase);
            if (locationMatch.Success)
                entities["location"] = locationMatch.Value;

            // Extract date
            var dateMatch = Regex.Match(message, @"\b(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})\b");
            if (dateMatch.Success)
                entities["date"] = dateMatch.Value;

            // Extract number of people
            var peopleMatch = Regex.Match(message, @"\b(\d+)\s*(người|people|pax)\b", RegexOptions.IgnoreCase);
            if (peopleMatch.Success)
                entities["numberOfPeople"] = int.Parse(peopleMatch.Groups[1].Value);

            // Extract price range
            var priceMatch = Regex.Match(message, @"\b(\d+)\s*(triệu|million|k|tr)\b", RegexOptions.IgnoreCase);
            if (priceMatch.Success)
                entities["priceRange"] = priceMatch.Groups[1].Value;

            return entities;
        }

        private async Task<string> GenerateResponseAsync(ChatbotIntent intent, Dictionary<string, object> entities, string originalMessage)
        {
            return intent switch
            {
                ChatbotIntent.Greeting => "Xin chào! Tôi là trợ lý ảo của WEBDULICH. Tôi có thể giúp gì cho bạn hôm nay? 😊",
                
                ChatbotIntent.Farewell => "Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi! Chúc bạn có một ngày tuyệt vời! 👋",
                
                ChatbotIntent.SearchTour => entities.ContainsKey("location")
                    ? $"Tôi sẽ tìm các tour du lịch đến {entities["location"]} cho bạn. Bạn muốn đi vào thời gian nào?"
                    : "Bạn muốn tìm tour du lịch đến đâu? Hãy cho tôi biết điểm đến bạn quan tâm nhé!",
                
                ChatbotIntent.BookTour => "Để đặt tour, bạn vui lòng cung cấp:\n1. Điểm đến\n2. Ngày khởi hành\n3. Số lượng người\nHoặc bạn có thể chọn tour từ danh sách của chúng tôi.",
                
                ChatbotIntent.CancelBooking => "Để hủy đặt tour, bạn cần cung cấp mã đặt tour. Bạn có thể tìm mã này trong email xác nhận hoặc tài khoản của bạn.",
                
                ChatbotIntent.PaymentInquiry => "Chúng tôi hỗ trợ nhiều phương thức thanh toán:\n💳 Thẻ tín dụng/ghi nợ\n📱 VNPay\n💰 MoMo\n🏦 Chuyển khoản ngân hàng\nBạn muốn biết thêm chi tiết về phương thức nào?",
                
                ChatbotIntent.PriceInquiry => "Giá tour phụ thuộc vào nhiều yếu tố như điểm đến, thời gian, số lượng người. Bạn có thể cho tôi biết tour nào bạn quan tâm để tôi cung cấp báo giá chính xác?",
                
                ChatbotIntent.TourDetails => "Tôi sẽ cung cấp thông tin chi tiết về tour. Bạn quan tâm đến tour nào? Hoặc bạn có thể cung cấp mã tour.",
                
                ChatbotIntent.ContactSupport => "Bạn có thể liên hệ với chúng tôi qua:\n📞 Hotline: 1900-xxxx\n📧 Email: support@webdulich.vn\n💬 Chat trực tiếp: Có sẵn 24/7\nBạn muốn tôi kết nối bạn với nhân viên hỗ trợ không?",
                
                ChatbotIntent.Recommendation => await GetRecommendationsAsync(entities),
                
                _ => "Xin lỗi, tôi chưa hiểu rõ câu hỏi của bạn. Bạn có thể diễn đạt lại hoặc chọn một trong các câu hỏi thường gặp bên dưới không?"
            };
        }

        private async Task<string> GetRecommendationsAsync(Dictionary<string, object> entities)
        {
            // In a real implementation, this would query the database for recommendations
            await Task.CompletedTask;
            
            return "Dựa trên sở thích của bạn, tôi gợi ý một số tour phổ biến:\n" +
                   "🏖️ Tour Phú Quốc 3N2Đ - 5.990.000đ\n" +
                   "🏔️ Tour Sapa 2N1Đ - 2.990.000đ\n" +
                   "🌊 Tour Hạ Long 2N1Đ - 3.490.000đ\n" +
                   "Bạn muốn xem chi tiết tour nào?";
        }

        private List<string> GetSuggestedActions(ChatbotIntent intent)
        {
            return intent switch
            {
                ChatbotIntent.Greeting => new List<string> { "Tìm tour", "Xem khuyến mãi", "Liên hệ hỗ trợ" },
                ChatbotIntent.SearchTour => new List<string> { "Tour trong nước", "Tour nước ngoài", "Tour giá rẻ" },
                ChatbotIntent.BookTour => new List<string> { "Xem tour phổ biến", "Nhập mã tour", "Gọi tư vấn" },
                ChatbotIntent.PaymentInquiry => new List<string> { "Thanh toán VNPay", "Thanh toán MoMo", "Chuyển khoản" },
                _ => new List<string> { "Tìm tour", "Đặt tour", "Liên hệ" }
            };
        }

        private List<QuickReply> GetQuickReplies(ChatbotIntent intent)
        {
            return intent switch
            {
                ChatbotIntent.Unknown => new List<QuickReply>
                {
                    new QuickReply { Text = "Tìm tour", Value = "search_tour", Icon = "🔍" },
                    new QuickReply { Text = "Đặt tour", Value = "book_tour", Icon = "📅" },
                    new QuickReply { Text = "Hỗ trợ", Value = "support", Icon = "💬" }
                },
                ChatbotIntent.SearchTour => new List<QuickReply>
                {
                    new QuickReply { Text = "Phú Quốc", Value = "phu_quoc", Icon = "🏖️" },
                    new QuickReply { Text = "Đà Nẵng", Value = "da_nang", Icon = "🌊" },
                    new QuickReply { Text = "Sapa", Value = "sapa", Icon = "🏔️" }
                },
                _ => new List<QuickReply>()
            };
        }

        private async Task SaveMessageAsync(string conversationId, string userId, string message, bool isBot, ChatbotIntent intent)
        {
            try
            {
                var cacheKey = $"chat_history_{conversationId}";
                var historyJson = await _cache.GetStringAsync(cacheKey);
                var history = string.IsNullOrEmpty(historyJson)
                    ? new List<ChatMessage>()
                    : JsonSerializer.Deserialize<List<ChatMessage>>(historyJson) ?? new List<ChatMessage>();

                history.Add(new ChatMessage
                {
                    ConversationId = conversationId,
                    UserId = userId,
                    Message = message,
                    IsBot = isBot,
                    Intent = intent,
                    Timestamp = DateTime.Now
                });

                // Keep only last 50 messages
                if (history.Count > 50)
                    history = history.Skip(history.Count - 50).ToList();

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(history), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving chat message");
            }
        }

        public async Task<List<ChatMessage>> GetConversationHistoryAsync(string conversationId)
        {
            try
            {
                var cacheKey = $"chat_history_{conversationId}";
                var historyJson = await _cache.GetStringAsync(cacheKey);
                
                return string.IsNullOrEmpty(historyJson)
                    ? new List<ChatMessage>()
                    : JsonSerializer.Deserialize<List<ChatMessage>>(historyJson) ?? new List<ChatMessage>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation history");
                return new List<ChatMessage>();
            }
        }

        public async Task<bool> ClearConversationAsync(string conversationId)
        {
            try
            {
                var cacheKey = $"chat_history_{conversationId}";
                await _cache.RemoveAsync(cacheKey);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing conversation");
                return false;
            }
        }

        public async Task<List<string>> GetSuggestedQuestionsAsync()
        {
            return await Task.FromResult(new List<string>
            {
                "Tìm tour du lịch Phú Quốc",
                "Giá tour Đà Nẵng bao nhiêu?",
                "Làm sao để đặt tour?",
                "Các phương thức thanh toán",
                "Chính sách hủy tour",
                "Khuyến mãi hiện tại",
                "Tour giá rẻ trong tháng",
                "Liên hệ hỗ trợ"
            });
        }

        public async Task<ChatbotAnalytics> GetAnalyticsAsync(DateTime from, DateTime to)
        {
            // In a real implementation, this would query from database
            return await Task.FromResult(new ChatbotAnalytics
            {
                TotalConversations = 150,
                TotalMessages = 450,
                AverageResponseTime = 1.2,
                AverageConfidence = 0.82,
                ResolvedQueries = 120,
                EscalatedToHuman = 30,
                IntentDistribution = new Dictionary<ChatbotIntent, int>
                {
                    { ChatbotIntent.SearchTour, 50 },
                    { ChatbotIntent.BookTour, 40 },
                    { ChatbotIntent.PaymentInquiry, 30 },
                    { ChatbotIntent.PriceInquiry, 20 },
                    { ChatbotIntent.ContactSupport, 10 }
                }
            });
        }
    }
}

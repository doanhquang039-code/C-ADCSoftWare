using Microsoft.Extensions.Caching.Distributed;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WEBDULICH.Services.AI
{
    public class ChatbotService : IChatbotService
    {
        private readonly ILogger<ChatbotService> _logger;
        private readonly IDistributedCache _cache;

        public ChatbotService(ILogger<ChatbotService> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public async Task<ChatbotResponse> GetResponseAsync(string message, string userId, string? conversationId = null)
        {
            try
            {
                conversationId ??= Guid.NewGuid().ToString();

                var intent = AnalyzeIntent(message);
                var confidence = CalculateConfidence(intent);
                var entities = ExtractEntities(message);

                await SaveMessageAsync(conversationId, userId, message, false, intent);

                var responseMessage = GenerateResponse(intent, entities);
                var suggestedActions = GetSuggestedActions(intent);
                var quickReplies = GetQuickReplies(intent);

                await SaveMessageAsync(conversationId, "bot", responseMessage, true, intent);

                _logger.LogInformation("Chatbot response generated for user {UserId}, intent: {Intent}", userId, intent);

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
                    Message = "Xin loi, toi dang gap su co. Vui long thu lai sau.",
                    ConversationId = conversationId ?? Guid.NewGuid().ToString(),
                    Intent = ChatbotIntent.Unknown,
                    Confidence = 0
                };
            }
        }

        private static ChatbotIntent AnalyzeIntent(string message)
        {
            var normalizedMessage = RemoveDiacritics(message).ToLowerInvariant().Trim();

            if (Regex.IsMatch(normalizedMessage, @"\b(xin chao|chao|hello|hi|hey)\b"))
                return ChatbotIntent.Greeting;

            if (Regex.IsMatch(normalizedMessage, @"\b(tam biet|bye|goodbye|cam on|thank)\b"))
                return ChatbotIntent.Farewell;

            if (Regex.IsMatch(normalizedMessage, @"\b(huy|cancel|huy tour|huy dat)\b"))
                return ChatbotIntent.CancelBooking;

            if (Regex.IsMatch(normalizedMessage, @"\b(thanh toan|payment|pay|tra tien)\b"))
                return ChatbotIntent.PaymentInquiry;

            if (Regex.IsMatch(normalizedMessage, @"\b(gia|price|cost|bao nhieu|phi)\b"))
                return ChatbotIntent.PriceInquiry;

            if (Regex.IsMatch(normalizedMessage, @"\b(dat|book|booking|dat tour|dat ve)\b"))
                return ChatbotIntent.BookTour;

            if (Regex.IsMatch(normalizedMessage, @"\b(chi tiet|details|thong tin|info|information)\b"))
                return ChatbotIntent.TourDetails;

            if (Regex.IsMatch(normalizedMessage, @"\b(lien he|contact|support|ho tro|help)\b"))
                return ChatbotIntent.ContactSupport;

            if (Regex.IsMatch(normalizedMessage, @"\b(goi y|recommend|suggestion|de xuat)\b"))
                return ChatbotIntent.Recommendation;

            if (Regex.IsMatch(normalizedMessage, @"\b(tim|search|tour|du lich|di|travel)\b"))
                return ChatbotIntent.SearchTour;

            return ChatbotIntent.Unknown;
        }

        private static double CalculateConfidence(ChatbotIntent intent)
        {
            return intent == ChatbotIntent.Unknown ? 0.3 : 0.85;
        }

        private static Dictionary<string, object> ExtractEntities(string message)
        {
            var entities = new Dictionary<string, object>();
            var normalizedMessage = RemoveDiacritics(message);

            var locations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ha noi"] = "Ha Noi",
                ["sai gon"] = "Sai Gon",
                ["tp hcm"] = "TP. Ho Chi Minh",
                ["ho chi minh"] = "TP. Ho Chi Minh",
                ["da nang"] = "Da Nang",
                ["nha trang"] = "Nha Trang",
                ["phu quoc"] = "Phu Quoc",
                ["ha long"] = "Ha Long",
                ["hue"] = "Hue",
                ["hoi an"] = "Hoi An",
                ["sapa"] = "Sapa"
            };

            foreach (var location in locations)
            {
                if (normalizedMessage.Contains(location.Key, StringComparison.OrdinalIgnoreCase))
                {
                    entities["location"] = location.Value;
                    break;
                }
            }

            var dateMatch = Regex.Match(message, @"\b(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})\b");
            if (dateMatch.Success)
                entities["date"] = dateMatch.Value;

            var peopleMatch = Regex.Match(normalizedMessage, @"\b(\d+)\s*(nguoi|people|pax)\b", RegexOptions.IgnoreCase);
            if (peopleMatch.Success && int.TryParse(peopleMatch.Groups[1].Value, out var people))
                entities["numberOfPeople"] = people;

            var priceMatch = Regex.Match(normalizedMessage, @"\b(\d+)\s*(trieu|million|k|tr)\b", RegexOptions.IgnoreCase);
            if (priceMatch.Success)
                entities["priceRange"] = priceMatch.Value;

            return entities;
        }

        private static string GenerateResponse(ChatbotIntent intent, Dictionary<string, object> entities)
        {
            return intent switch
            {
                ChatbotIntent.Greeting => "Xin chao! Toi la tro ly ao cua WEBDULICH. Toi co the giup gi cho ban hom nay?",
                ChatbotIntent.Farewell => "Cam on ban da su dung dich vu cua chung toi. Chuc ban co mot ngay tot lanh!",
                ChatbotIntent.SearchTour => entities.TryGetValue("location", out var location)
                    ? $"Toi se tim cac tour du lich den {location} cho ban. Ban muon di vao thoi gian nao?"
                    : "Ban muon tim tour du lich den dau? Hay cho toi biet diem den ban quan tam.",
                ChatbotIntent.BookTour => "De dat tour, ban vui long cung cap:\n1. Diem den\n2. Ngay khoi hanh\n3. So luong nguoi\nHoac ban co the chon tour tu danh sach cua chung toi.",
                ChatbotIntent.CancelBooking => "De huy dat tour, ban can cung cap ma dat tour. Ban co the tim ma nay trong email xac nhan hoac tai khoan cua ban.",
                ChatbotIntent.PaymentInquiry => "Chung toi ho tro nhieu phuong thuc thanh toan:\n- The tin dung/ghi no\n- VNPay\n- MoMo\n- Chuyen khoan ngan hang\nBan muon biet them chi tiet ve phuong thuc nao?",
                ChatbotIntent.PriceInquiry => "Gia tour phu thuoc vao diem den, thoi gian va so luong nguoi. Ban cho toi biet tour hoac diem den dang quan tam de toi ho tro bao gia chinh xac.",
                ChatbotIntent.TourDetails => "Toi se cung cap thong tin chi tiet ve tour. Ban quan tam den tour nao, hoac co ma tour khong?",
                ChatbotIntent.ContactSupport => "Ban co the lien he voi chung toi qua:\n- Hotline: 1900-xxxx\n- Email: support@webdulich.vn\n- Chat truc tiep: co san 24/7\nBan muon toi ket noi ban voi nhan vien ho tro khong?",
                ChatbotIntent.Recommendation => "Dua tren so thich cua ban, toi goi y mot so tour pho bien:\n- Tour Phu Quoc 3N2D - 5.990.000d\n- Tour Sapa 2N1D - 2.990.000d\n- Tour Ha Long 2N1D - 3.490.000d\nBan muon xem chi tiet tour nao?",
                _ => "Xin loi, toi chua hieu ro cau hoi cua ban. Ban co the dien dat lai hoac chon mot cau hoi goi y ben duoi khong?"
            };
        }

        private static List<string> GetSuggestedActions(ChatbotIntent intent)
        {
            return intent switch
            {
                ChatbotIntent.Greeting => new List<string> { "Tim tour", "Xem khuyen mai", "Lien he ho tro" },
                ChatbotIntent.SearchTour => new List<string> { "Tour trong nuoc", "Tour nuoc ngoai", "Tour gia re" },
                ChatbotIntent.BookTour => new List<string> { "Xem tour pho bien", "Nhap ma tour", "Goi tu van" },
                ChatbotIntent.PaymentInquiry => new List<string> { "Thanh toan VNPay", "Thanh toan MoMo", "Chuyen khoan" },
                _ => new List<string> { "Tim tour", "Dat tour", "Lien he" }
            };
        }

        private static List<QuickReply> GetQuickReplies(ChatbotIntent intent)
        {
            return intent switch
            {
                ChatbotIntent.Unknown => new List<QuickReply>
                {
                    new QuickReply { Text = "Tim tour", Value = "Tim tour", Icon = "search" },
                    new QuickReply { Text = "Dat tour", Value = "Dat tour", Icon = "calendar" },
                    new QuickReply { Text = "Ho tro", Value = "Ho tro", Icon = "chat" }
                },
                ChatbotIntent.SearchTour => new List<QuickReply>
                {
                    new QuickReply { Text = "Phu Quoc", Value = "Tim tour Phu Quoc", Icon = "beach" },
                    new QuickReply { Text = "Da Nang", Value = "Tim tour Da Nang", Icon = "waves" },
                    new QuickReply { Text = "Sapa", Value = "Tim tour Sapa", Icon = "mountain" }
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
                await _cache.RemoveAsync($"chat_history_{conversationId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing conversation");
                return false;
            }
        }

        public Task<List<string>> GetSuggestedQuestionsAsync()
        {
            return Task.FromResult(new List<string>
            {
                "Tim tour du lich Phu Quoc",
                "Gia tour Da Nang bao nhieu?",
                "Lam sao de dat tour?",
                "Cac phuong thuc thanh toan",
                "Chinh sach huy tour",
                "Khuyen mai hien tai",
                "Tour gia re trong thang",
                "Lien he ho tro"
            });
        }

        public Task<ChatbotAnalytics> GetAnalyticsAsync(DateTime from, DateTime to)
        {
            return Task.FromResult(new ChatbotAnalytics
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

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var chars = normalized
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .Select(c => c == 'đ' ? 'd' : c == 'Đ' ? 'D' : c);

            return new string(chars.ToArray()).Normalize(NormalizationForm.FormC);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Services.AI;

namespace WEBDULICH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(
            IChatbotService chatbotService,
            ILogger<ChatbotController> logger)
        {
            _chatbotService = chatbotService;
            _logger = logger;
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { success = false, message = "Message is required" });
                }

                var response = await _chatbotService.GetResponseAsync(
                    request.Message,
                    request.UserId ?? "anonymous",
                    request.ConversationId
                );

                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chatbot message");
                return StatusCode(500, new { success = false, message = "Failed to process message" });
            }
        }

        [HttpGet("history/{conversationId}")]
        public async Task<IActionResult> GetHistory(string conversationId)
        {
            try
            {
                var history = await _chatbotService.GetConversationHistoryAsync(conversationId);
                return Ok(new { success = true, data = history });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation history");
                return StatusCode(500, new { success = false, message = "Failed to retrieve history" });
            }
        }

        [HttpDelete("history/{conversationId}")]
        public async Task<IActionResult> ClearHistory(string conversationId)
        {
            try
            {
                await _chatbotService.ClearConversationAsync(conversationId);
                return Ok(new { success = true, message = "History cleared" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing conversation history");
                return StatusCode(500, new { success = false, message = "Failed to clear history" });
            }
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
                var toDate = to ?? DateTime.UtcNow;

                var analytics = await _chatbotService.GetAnalyticsAsync(fromDate, toDate);
                return Ok(new { success = true, data = analytics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chatbot analytics");
                return StatusCode(500, new { success = false, message = "Failed to retrieve analytics" });
            }
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions()
        {
            try
            {
                var suggestions = await _chatbotService.GetSuggestedQuestionsAsync();
                return Ok(new { success = true, data = suggestions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chatbot suggestions");
                return StatusCode(500, new { success = false, message = "Failed to retrieve suggestions" });
            }
        }

        [HttpPost("feedback")]
        public IActionResult SubmitFeedback([FromBody] ChatFeedbackRequest request)
        {
            try
            {
                if (request.Rating < 1 || request.Rating > 5)
                {
                    return BadRequest(new { success = false, message = "Rating must be between 1 and 5" });
                }

                _logger.LogInformation(
                    "Chatbot feedback: ConversationId={ConversationId}, Rating={Rating}, Comment={Comment}",
                    request.ConversationId,
                    request.Rating,
                    request.Comment);

                return Ok(new { success = true, message = "Feedback received" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting feedback");
                return StatusCode(500, new { success = false, message = "Failed to submit feedback" });
            }
        }
    }

    public class ChatMessageRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? ConversationId { get; set; }
    }

    public class ChatFeedbackRequest
    {
        public string ConversationId { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}

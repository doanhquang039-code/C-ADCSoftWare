using WEBDULICH.Models;

namespace WEBDULICH.Services.AI
{
    public interface ITravelAgentService
    {
        Task<TravelAgentResponse> RunAsync(TravelAgentRequest request);
    }

    public class TravelAgentRequest
    {
        public string Query { get; set; } = string.Empty;
        public string Mode { get; set; } = "agent";
        public decimal Budget { get; set; }
        public int Days { get; set; } = 3;
        public int Adults { get; set; } = 2;
        public int Children { get; set; }
        public string Destination { get; set; } = string.Empty;
        public string TravelStyle { get; set; } = "balanced";
    }

    public class TravelAgentResponse
    {
        public string Intent { get; set; } = "AGENT";
        public string Mode { get; set; } = "rule-based-agent";
        public string Answer { get; set; } = string.Empty;
        public int FitScore { get; set; }
        public Dictionary<string, object> DataProfile { get; set; } = new();
        public List<string> ModelsUsed { get; set; } = new();
        public List<string> ToolsUsed { get; set; } = new();
        public List<TravelAgentFinding> Findings { get; set; } = new();
        public List<TravelSearchResult> SearchResults { get; set; } = new();
        public List<string> NextActions { get; set; } = new();
        public TripPlanResponse? TripPlan { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }

    public class TravelAgentFinding
    {
        public string Code { get; set; } = string.Empty;
        public string Severity { get; set; } = "LOW";
        public string Message { get; set; } = string.Empty;
        public string Evidence { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    public class TravelSearchResult
    {
        public string Type { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public double Rating { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}

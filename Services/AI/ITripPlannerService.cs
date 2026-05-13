using WEBDULICH.Models;

namespace WEBDULICH.Services.AI
{
    public interface ITripPlannerService
    {
        Task<TripPlanResponse> GeneratePlanAsync(TripPlanRequest request);
    }

    public class TripPlanRequest
    {
        public string Destination { get; set; } = string.Empty;
        public int Days { get; set; } = 3;
        public int Adults { get; set; } = 2;
        public int Children { get; set; }
        public decimal Budget { get; set; }
        public string TravelStyle { get; set; } = "balanced";
        public string Interests { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
    }

    public class TripPlanResponse
    {
        public string Summary { get; set; } = string.Empty;
        public decimal EstimatedBudget { get; set; }
        public decimal BudgetGap { get; set; }
        public string BudgetStatus { get; set; } = string.Empty;
        public List<TripPlanDay> Days { get; set; } = new();
        public List<TripPlanTour> RecommendedTours { get; set; } = new();
        public List<TripPlanHotel> RecommendedHotels { get; set; } = new();
        public List<string> Tips { get; set; } = new();
    }

    public class TripPlanDay
    {
        public int DayNumber { get; set; }
        public DateTime? Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<string> Activities { get; set; } = new();
    }

    public class TripPlanTour
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public int Price { get; set; }
        public int Duration { get; set; }
        public decimal Rating { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class TripPlanHotel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int PricePerNight { get; set; }
        public int Rating { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}

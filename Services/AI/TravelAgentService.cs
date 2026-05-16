using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services.AI
{
    public class TravelAgentService : ITravelAgentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITripPlannerService _tripPlannerService;
        private readonly ILogger<TravelAgentService> _logger;

        public TravelAgentService(
            ApplicationDbContext context,
            ITripPlannerService tripPlannerService,
            ILogger<TravelAgentService> logger)
        {
            _context = context;
            _tripPlannerService = tripPlannerService;
            _logger = logger;
        }

        public async Task<TravelAgentResponse> RunAsync(TravelAgentRequest request)
        {
            Normalize(request);

            var intent = ResolveIntent(request);
            var tours = await _context.Tours
                .Include(t => t.Destination)
                .Include(t => t.Category)
                .Include(t => t.Reviews)
                .Where(t => t.Quantity > 0)
                .ToListAsync();
            var hotels = await _context.Hotels
                .Where(h => h.Quantity > 0)
                .ToListAsync();

            var searchResults = Search(request, tours, hotels, intent);
            var tripPlan = intent is "PLAN" or "AGENT"
                ? await _tripPlannerService.GeneratePlanAsync(ToTripPlanRequest(request))
                : null;
            var findings = Diagnose(request, tours, hotels, searchResults, tripPlan);
            var fitScore = CalculateFitScore(findings, searchResults, tripPlan);
            var nextActions = BuildNextActions(intent, findings, searchResults, tripPlan);

            return new TravelAgentResponse
            {
                Intent = intent,
                Mode = "rule-based-travel-agent",
                Answer = BuildAnswer(intent, request, findings, searchResults, tripPlan, fitScore),
                FitScore = fitScore,
                DataProfile = BuildDataProfile(tours, hotels, findings, searchResults, tripPlan),
                ModelsUsed = new List<string>
                {
                    "travel-intent-router-v1",
                    "tour-hotel-ranking-v1",
                    "budget-diagnosis-v1",
                    "trip-plan-orchestrator-v1"
                },
                ToolsUsed = BuildTools(intent, tripPlan),
                Findings = findings,
                SearchResults = searchResults,
                NextActions = nextActions,
                TripPlan = tripPlan,
                GeneratedAt = DateTime.Now
            };
        }

        private static void Normalize(TravelAgentRequest request)
        {
            request.Query = request.Query?.Trim() ?? string.Empty;
            request.Mode = string.IsNullOrWhiteSpace(request.Mode) ? "agent" : request.Mode.Trim().ToLowerInvariant();
            request.Destination = request.Destination?.Trim() ?? string.Empty;
            request.TravelStyle = string.IsNullOrWhiteSpace(request.TravelStyle) ? "balanced" : request.TravelStyle.Trim().ToLowerInvariant();
            request.Days = Math.Clamp(request.Days, 1, 14);
            request.Adults = Math.Clamp(request.Adults, 1, 30);
            request.Children = Math.Clamp(request.Children, 0, 30);
        }

        private static string ResolveIntent(TravelAgentRequest request)
        {
            var mode = request.Mode.ToUpperInvariant();
            if (new[] { "SEARCH", "PLAN", "DIAGNOSE", "AGENT" }.Contains(mode))
            {
                return mode;
            }

            var query = NormalizeText(request.Query);
            if (query.Contains("tim") || query.Contains("search") || query.Contains("khach san") || query.Contains("tour"))
            {
                return "SEARCH";
            }
            if (query.Contains("lich trinh") || query.Contains("ke hoach") || query.Contains("plan"))
            {
                return "PLAN";
            }
            if (query.Contains("ngan sach") || query.Contains("rui ro") || query.Contains("chan doan") || query.Contains("ket qua"))
            {
                return "DIAGNOSE";
            }
            return "AGENT";
        }

        private List<TravelSearchResult> Search(TravelAgentRequest request, List<Tour> tours, List<Hotel> hotels, string intent)
        {
            if (intent == "DIAGNOSE")
            {
                return new List<TravelSearchResult>();
            }

            var keyword = ExtractKeyword(request);
            var people = request.Adults + request.Children;

            var tourResults = tours
                .Select(t => new { Tour = t, Score = ScoreTour(t, request, keyword) })
                .Where(x => x.Score > 0 || string.IsNullOrWhiteSpace(keyword))
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Tour.Rating)
                .ThenBy(x => x.Tour.Price)
                .Take(6)
                .Select(x => new TravelSearchResult
                {
                    Type = "tour",
                    Id = x.Tour.Id,
                    Title = x.Tour.Name,
                    Subtitle = x.Tour.Destination?.Name ?? x.Tour.Location,
                    Price = x.Tour.Price * people,
                    Rating = (double)x.Tour.Rating,
                    Url = $"/Tour/Details/{x.Tour.Id}",
                    Reason = BuildTourReason(x.Tour, request, x.Score)
                });

            var hotelResults = hotels
                .Select(h => new { Hotel = h, Score = ScoreHotel(h, request, keyword) })
                .Where(x => x.Score > 0 || string.IsNullOrWhiteSpace(keyword))
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Hotel.Rating)
                .ThenBy(x => x.Hotel.Price)
                .Take(4)
                .Select(x => new TravelSearchResult
                {
                    Type = "hotel",
                    Id = x.Hotel.Id,
                    Title = x.Hotel.Name,
                    Subtitle = x.Hotel.Address,
                    Price = x.Hotel.Price * Math.Max(1, request.Days - 1),
                    Rating = x.Hotel.Rating,
                    Url = $"/Hotel",
                    Reason = BuildHotelReason(x.Hotel, request, x.Score)
                });

            return tourResults.Concat(hotelResults).ToList();
        }

        private static List<TravelAgentFinding> Diagnose(TravelAgentRequest request, List<Tour> tours, List<Hotel> hotels,
            List<TravelSearchResult> searchResults, TripPlanResponse? tripPlan)
        {
            var findings = new List<TravelAgentFinding>();
            if (!tours.Any())
            {
                findings.Add(Finding("NO_TOUR_INVENTORY", "HIGH", "Khong co tour con cho trong du lieu hien tai.", "Tours available = 0", 0.95));
            }
            if (!hotels.Any())
            {
                findings.Add(Finding("NO_HOTEL_INVENTORY", "MEDIUM", "Khong co khach san con phong trong du lieu hien tai.", "Hotels available = 0", 0.9));
            }
            if (request.Budget > 0 && tripPlan != null && tripPlan.EstimatedBudget > request.Budget)
            {
                findings.Add(Finding("BUDGET_OVER_LIMIT", "HIGH", $"Lich trinh uoc tinh vuot ngan sach {tripPlan.BudgetGap:N0} VND.", "EstimatedBudget > Budget", 0.88));
            }
            if (!string.IsNullOrWhiteSpace(request.Destination) && searchResults.Count == 0)
            {
                findings.Add(Finding("LOW_MATCH_RESULT", "MEDIUM", "Chua tim thay tour/khach san khop diem den hoac tu khoa.", "Search results = 0", 0.76));
            }
            if (request.Days > 7 && request.Budget > 0 && tripPlan != null && tripPlan.EstimatedBudget > request.Budget * 0.9m)
            {
                findings.Add(Finding("LONG_TRIP_COST_PRESSURE", "MEDIUM", "So ngay dai tao ap luc chi phi gan nguong ngan sach.", "Days > 7 and estimated cost near budget", 0.7));
            }
            if (!findings.Any())
            {
                findings.Add(Finding("TRIP_FEASIBLE", "LOW", "Yeu cau hien tai kha kha thi voi du lieu tour/khach san dang co.", "No risk rule exceeded", 0.68));
            }
            return findings;
        }

        private static int CalculateFitScore(List<TravelAgentFinding> findings, List<TravelSearchResult> results, TripPlanResponse? tripPlan)
        {
            var score = 65;
            score += Math.Min(20, results.Count * 2);
            if (tripPlan != null && tripPlan.Days.Any()) score += 10;

            foreach (var finding in findings)
            {
                score -= finding.Severity switch
                {
                    "HIGH" => 20,
                    "MEDIUM" => 10,
                    _ => 0
                };
            }

            return Math.Clamp(score, 0, 100);
        }

        private static List<string> BuildNextActions(string intent, List<TravelAgentFinding> findings,
            List<TravelSearchResult> results, TripPlanResponse? tripPlan)
        {
            var actions = new List<string>();
            if (findings.Any(f => f.Code == "BUDGET_OVER_LIMIT"))
            {
                actions.Add("Giam so ngay, chon style tiet kiem hoac tang ngan sach truoc khi dat.");
            }
            if (results.Any(r => r.Type == "tour"))
            {
                actions.Add("Mo tour phu hop nhat de kiem tra ngay khoi hanh va so cho con lai.");
            }
            if (results.Any(r => r.Type == "hotel"))
            {
                actions.Add("So sanh khach san theo vi tri va gia moi dem truoc khi chot lich trinh.");
            }
            if (tripPlan != null)
            {
                actions.Add("Luu lich trinh AI va xac nhan lai gia tai thoi diem thanh toan.");
            }
            if (intent == "SEARCH" && !results.Any())
            {
                actions.Add("Thu lai bang ten tinh/thanh pho ngan hon hoac bo trong diem den.");
            }
            if (!actions.Any())
            {
                actions.Add("Nhap diem den, ngan sach va so ngay de AI Agent lap goi y chinh xac hon.");
            }
            return actions.Distinct().Take(5).ToList();
        }

        private static string BuildAnswer(string intent, TravelAgentRequest request, List<TravelAgentFinding> findings,
            List<TravelSearchResult> results, TripPlanResponse? tripPlan, int fitScore)
        {
            var destination = string.IsNullOrWhiteSpace(request.Destination) ? ExtractKeyword(request) : request.Destination;
            if (string.IsNullOrWhiteSpace(destination)) destination = "diem den phu hop";

            var answer = $"AI Travel Agent da xu ly intent {intent}. Fit score: {fitScore}/100. ";
            answer += $"Tim thay {results.Count} goi y tour/khach san cho {destination}. ";
            if (tripPlan != null)
            {
                answer += $"Lich trinh uoc tinh {tripPlan.EstimatedBudget:N0} VND. {tripPlan.BudgetStatus} ";
            }
            answer += $"Chan doan uu tien: {findings.First().Message}";
            return answer;
        }

        private static Dictionary<string, object> BuildDataProfile(List<Tour> tours, List<Hotel> hotels,
            List<TravelAgentFinding> findings, List<TravelSearchResult> results, TripPlanResponse? tripPlan)
        {
            return new Dictionary<string, object>
            {
                ["availableTours"] = tours.Count,
                ["availableHotels"] = hotels.Count,
                ["findingsCount"] = findings.Count,
                ["searchResultsCount"] = results.Count,
                ["hasTripPlan"] = tripPlan != null,
                ["dataFreshness"] = "database-current"
            };
        }

        private static List<string> BuildTools(string intent, TripPlanResponse? tripPlan)
        {
            var tools = new List<string> { "tour-hotel-search", "budget-diagnosis" };
            if (intent is "PLAN" or "AGENT" || tripPlan != null)
            {
                tools.Add("trip-planner");
            }
            return tools;
        }

        private static TripPlanRequest ToTripPlanRequest(TravelAgentRequest request)
        {
            return new TripPlanRequest
            {
                Destination = request.Destination,
                Days = request.Days,
                Adults = request.Adults,
                Children = request.Children,
                Budget = request.Budget,
                TravelStyle = request.TravelStyle,
                Interests = request.Query,
                StartDate = DateTime.Today.AddDays(14)
            };
        }

        private static double ScoreTour(Tour tour, TravelAgentRequest request, string keyword)
        {
            var text = NormalizeText($"{tour.Name} {tour.Description} {tour.Destination?.Name} {tour.Destination?.Location} {tour.Category?.Name}");
            var score = 0.0;
            if (!string.IsNullOrWhiteSpace(keyword) && text.Contains(keyword)) score += 45;
            if (!string.IsNullOrWhiteSpace(request.Destination) && text.Contains(NormalizeText(request.Destination))) score += 35;
            if (tour.Duration <= request.Days) score += 12;
            score += (double)tour.Rating * 5;
            if (request.Budget > 0 && tour.Price * (request.Adults + request.Children) <= request.Budget) score += 10;
            if (request.TravelStyle == "saving" && tour.Price <= 3000000) score += 8;
            if (request.TravelStyle == "premium" && tour.Rating >= 4) score += 8;
            return score;
        }

        private static double ScoreHotel(Hotel hotel, TravelAgentRequest request, string keyword)
        {
            var text = NormalizeText($"{hotel.Name} {hotel.Address}");
            var score = 0.0;
            if (!string.IsNullOrWhiteSpace(keyword) && text.Contains(keyword)) score += 40;
            if (!string.IsNullOrWhiteSpace(request.Destination) && text.Contains(NormalizeText(request.Destination))) score += 35;
            score += hotel.Rating * 6;
            if (request.Budget > 0 && hotel.Price * Math.Max(1, request.Days - 1) <= request.Budget) score += 10;
            if (request.TravelStyle == "saving" && hotel.Price <= 800000) score += 8;
            if (request.TravelStyle == "premium" && hotel.Rating >= 4) score += 8;
            return score;
        }

        private static string BuildTourReason(Tour tour, TravelAgentRequest request, double score)
        {
            if (!string.IsNullOrWhiteSpace(request.Destination) && NormalizeText($"{tour.Name} {tour.Location}").Contains(NormalizeText(request.Destination)))
            {
                return "Khop diem den nguoi dung dang tim.";
            }
            if (tour.Rating >= 4) return "Danh gia tot va con so luong trong kho.";
            if (score > 0) return "Phu hop voi mot phan query, ngan sach hoac thoi luong.";
            return "Goi y bo sung tu inventory hien tai.";
        }

        private static string BuildHotelReason(Hotel hotel, TravelAgentRequest request, double score)
        {
            if (!string.IsNullOrWhiteSpace(request.Destination) && NormalizeText(hotel.Address).Contains(NormalizeText(request.Destination)))
            {
                return "Dia chi gan diem den dang tim.";
            }
            if (hotel.Rating >= 4) return "Danh gia khach san cao.";
            if (score > 0) return "Phu hop voi bo loc hien tai.";
            return "Lua chon tham khao tu inventory khach san.";
        }

        private static TravelAgentFinding Finding(string code, string severity, string message, string evidence, double confidence)
        {
            return new TravelAgentFinding
            {
                Code = code,
                Severity = severity,
                Message = message,
                Evidence = evidence,
                Confidence = confidence
            };
        }

        private static string ExtractKeyword(TravelAgentRequest request)
        {
            var value = string.IsNullOrWhiteSpace(request.Destination) ? request.Query : request.Destination;
            return NormalizeText(value)
                .Replace("tim kiem", " ")
                .Replace("tim", " ")
                .Replace("tour", " ")
                .Replace("khach san", " ")
                .Replace("lich trinh", " ")
                .Replace("di", " ")
                .Replace("du lich", " ")
                .Trim();
        }

        private static string NormalizeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var text = value.ToLowerInvariant().Trim().Replace('đ', 'd');
            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            return new string(normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray())
                .Normalize(System.Text.NormalizationForm.FormC);
        }
    }
}

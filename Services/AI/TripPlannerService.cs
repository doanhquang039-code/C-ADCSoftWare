using Microsoft.EntityFrameworkCore;
using WEBDULICH.Services;

namespace WEBDULICH.Services.AI
{
    public class TripPlannerService : ITripPlannerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TripPlannerService> _logger;

        public TripPlannerService(ApplicationDbContext context, ILogger<TripPlannerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TripPlanResponse> GeneratePlanAsync(TripPlanRequest request)
        {
            Normalize(request);

            var tours = await _context.Tours
                .Include(t => t.Destination)
                .Include(t => t.Reviews)
                .Where(t => t.Quantity > 0)
                .ToListAsync();

            var hotels = await _context.Hotels
                .Where(h => h.Quantity > 0)
                .ToListAsync();

            var rankedTours = tours
                .Select(t => new { Tour = t, Score = ScoreTour(t, request) })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Tour.Rating)
                .ThenBy(x => x.Tour.Price)
                .Take(6)
                .ToList();

            var rankedHotels = hotels
                .Select(h => new { Hotel = h, Score = ScoreHotel(h, request) })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Hotel.Rating)
                .ThenBy(x => x.Hotel.Price)
                .Take(4)
                .ToList();

            var selectedTours = rankedTours.Take(Math.Min(request.Days, rankedTours.Count)).Select(x => x.Tour).ToList();
            var selectedHotel = rankedHotels.FirstOrDefault()?.Hotel;
            var people = request.Adults + request.Children;
            var tourBudget = selectedTours.Sum(t => (decimal)t.Price * people);
            var hotelBudget = selectedHotel == null ? 0 : (decimal)selectedHotel.Price * Math.Max(1, request.Days - 1);
            var estimatedBudget = tourBudget + hotelBudget;

            return new TripPlanResponse
            {
                Summary = BuildSummary(request, selectedTours.Count, selectedHotel != null),
                EstimatedBudget = estimatedBudget,
                BudgetGap = request.Budget <= 0 ? 0 : estimatedBudget - request.Budget,
                BudgetStatus = GetBudgetStatus(request.Budget, estimatedBudget),
                Days = BuildDays(request, rankedTours.Select(x => x.Tour).ToList(), selectedHotel),
                RecommendedTours = rankedTours.Select(x => new TripPlanTour
                {
                    Id = x.Tour.Id,
                    Name = x.Tour.Name,
                    Destination = x.Tour.Destination?.Name ?? x.Tour.Location,
                    Price = x.Tour.Price,
                    Duration = x.Tour.Duration,
                    Rating = x.Tour.Rating,
                    ImageUrl = FirstNotEmpty(x.Tour.ImageUrl, x.Tour.Image),
                    Reason = BuildTourReason(x.Score, x.Tour, request)
                }).ToList(),
                RecommendedHotels = rankedHotels.Select(x => new TripPlanHotel
                {
                    Id = x.Hotel.Id,
                    Name = x.Hotel.Name,
                    Address = x.Hotel.Address,
                    PricePerNight = x.Hotel.Price,
                    Rating = x.Hotel.Rating,
                    ImageUrl = x.Hotel.Image,
                    Reason = BuildHotelReason(x.Score, x.Hotel, request)
                }).ToList(),
                Tips = BuildTips(request, estimatedBudget, rankedTours.Count, rankedHotels.Count)
            };
        }

        private static void Normalize(TripPlanRequest request)
        {
            request.Days = Math.Clamp(request.Days, 1, 14);
            request.Adults = Math.Clamp(request.Adults, 1, 30);
            request.Children = Math.Clamp(request.Children, 0, 30);
            request.Destination = request.Destination?.Trim() ?? string.Empty;
            request.TravelStyle = string.IsNullOrWhiteSpace(request.TravelStyle) ? "balanced" : request.TravelStyle.Trim().ToLowerInvariant();
            request.Interests = request.Interests?.Trim() ?? string.Empty;
        }

        private static double ScoreTour(Models.Tour tour, TripPlanRequest request)
        {
            var score = 0.0;
            var destinationText = $"{tour.Name} {tour.Description} {tour.Destination?.Name} {tour.Destination?.Location}".ToLowerInvariant();
            var destination = request.Destination.ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(destination) && destinationText.Contains(destination)) score += 45;
            if (tour.Duration <= request.Days) score += 15;
            if (tour.Rating > 0) score += (double)tour.Rating * 5;
            if (request.Budget > 0 && tour.Price <= request.Budget) score += 10;
            if (request.TravelStyle == "saving" && tour.Price <= 3000000) score += 12;
            if (request.TravelStyle == "premium" && tour.Rating >= 4) score += 12;
            if (request.TravelStyle == "adventure" && ContainsAny(destinationText, "trekking", "bien", "dao", "nui", "kham pha")) score += 12;

            foreach (var interest in SplitTerms(request.Interests))
            {
                if (destinationText.Contains(interest)) score += 8;
            }

            return score;
        }

        private static double ScoreHotel(Models.Hotel hotel, TripPlanRequest request)
        {
            var score = 0.0;
            var text = $"{hotel.Name} {hotel.Address}".ToLowerInvariant();
            var destination = request.Destination.ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(destination) && text.Contains(destination)) score += 40;
            score += hotel.Rating * 8;
            if (request.Budget > 0 && hotel.Price * Math.Max(1, request.Days - 1) <= request.Budget) score += 12;
            if (request.TravelStyle == "saving" && hotel.Price <= 800000) score += 10;
            if (request.TravelStyle == "premium" && hotel.Rating >= 4) score += 10;

            return score;
        }

        private static List<TripPlanDay> BuildDays(TripPlanRequest request, List<Models.Tour> tours, Models.Hotel? hotel)
        {
            var days = new List<TripPlanDay>();
            for (var day = 1; day <= request.Days; day++)
            {
                var tour = tours.Count == 0 ? null : tours[(day - 1) % tours.Count];
                var activities = new List<string>();

                if (day == 1) activities.Add("Den noi, nhan phong va lam quen khu vuc trung tam.");
                if (tour != null) activities.Add($"Trai nghiem {tour.Name} ({tour.Duration} ngay), uu tien khung gio dep de tham quan.");
                if (hotel != null) activities.Add($"Nghi tai {hotel.Name}, dia chi {hotel.Address}.");
                if (day == request.Days) activities.Add("Tong ket lich trinh, mua dac san va chuan bi ve.");
                if (activities.Count == 0) activities.Add("Tu do kham pha diem den theo so thich ca nhan.");

                days.Add(new TripPlanDay
                {
                    DayNumber = day,
                    Date = request.StartDate?.Date.AddDays(day - 1),
                    Title = tour == null ? $"Ngay {day}: Kham pha tu do" : $"Ngay {day}: {tour.Name}",
                    Activities = activities
                });
            }

            return days;
        }

        private static string BuildSummary(TripPlanRequest request, int tourCount, bool hasHotel)
        {
            var destination = string.IsNullOrWhiteSpace(request.Destination) ? "diem den phu hop" : request.Destination;
            var hotelText = hasHotel ? "co goi y khach san" : "chua co khach san phu hop trong du lieu";
            return $"AI da lap lich trinh {request.Days} ngay cho {destination}, gom {tourCount} tour uu tien va {hotelText}.";
        }

        private static string GetBudgetStatus(decimal budget, decimal estimated)
        {
            if (budget <= 0) return "Chua nhap ngan sach nen AI chi uoc tinh chi phi.";
            if (estimated <= budget) return "Nam trong ngan sach.";
            var diff = estimated - budget;
            return $"Vuot ngan sach du kien {diff:N0} VND.";
        }

        private static string BuildTourReason(double score, Models.Tour tour, TripPlanRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Destination) && $"{tour.Name} {tour.Destination?.Name} {tour.Destination?.Location}".Contains(request.Destination, StringComparison.OrdinalIgnoreCase))
            {
                return "Khop diem den va co diem uu tien cao trong du lieu tour.";
            }

            if (tour.Rating >= 4) return "Danh gia tot, phu hop de dua vao lich trinh chinh.";
            if (tour.Price > 0) return "Gia tour hop ly de can doi ngan sach.";
            return score > 0 ? "Phu hop voi mot phan yeu cau cua ban." : "Goi y bo sung khi chua co du du lieu khop chinh xac.";
        }

        private static string BuildHotelReason(double score, Models.Hotel hotel, TripPlanRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Destination) && hotel.Address.Contains(request.Destination, StringComparison.OrdinalIgnoreCase))
            {
                return "Gan diem den ban dang tim.";
            }

            if (hotel.Rating >= 4) return "Danh gia khach san cao.";
            if (request.Budget > 0 && hotel.Price <= request.Budget) return "Gia phong phu hop ngan sach.";
            return score > 0 ? "Phu hop voi bo loc hien tai." : "Lua chon tham khao tu du lieu khach san.";
        }

        private static List<string> BuildTips(TripPlanRequest request, decimal estimatedBudget, int tourCount, int hotelCount)
        {
            var tips = new List<string>();
            if (tourCount == 0) tips.Add("Chua tim thay tour khop diem den; hay thu ten tinh/thanh pho hoac bo trong diem den.");
            if (hotelCount == 0) tips.Add("Chua tim thay khach san con phong trong du lieu hien tai.");
            if (request.Budget > 0 && estimatedBudget > request.Budget) tips.Add("Co the giam so ngay, chon style tiet kiem hoac tang ngan sach de lich trinh de dat hon.");
            if (request.StartDate.HasValue) tips.Add("Nen xac nhan lai tinh trang cho trong truoc ngay khoi hanh.");
            tips.Add("Gia chi la uoc tinh tu du lieu hien co, chua bao gom phu phi va khuyen mai theo thoi diem.");
            return tips;
        }

        private static IEnumerable<string> SplitTerms(string value)
        {
            return value.ToLowerInvariant()
                .Split(new[] { ',', ';', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => x.Length >= 3);
        }

        private static bool ContainsAny(string text, params string[] terms)
        {
            return terms.Any(term => text.Contains(term));
        }

        private static string FirstNotEmpty(params string?[] values)
        {
            return values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty;
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    public class Location
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string Address { get; set; }
        
        /// <summary>
        /// Latitude (vĩ độ)
        /// </summary>
        public double Latitude { get; set; }
        
        /// <summary>
        /// Longitude (kinh độ)
        /// </summary>
        public double Longitude { get; set; }
        
        /// <summary>
        /// "Tour", "Hotel", "Destination", "Restaurant", "Attraction"
        /// </summary>
        public string LocationType { get; set; }
        
        /// <summary>
        /// ID của đối tượng liên quan (TourId, HotelId, etc.)
        /// </summary>
        public int? RelatedId { get; set; }
        
        public string Description { get; set; }
        
        public string Image { get; set; }
        
        /// <summary>
        /// Thông tin liên hệ
        /// </summary>
        public string ContactInfo { get; set; }
        
        /// <summary>
        /// Giờ mở cửa
        /// </summary>
        public string OpeningHours { get; set; }
        
        /// <summary>
        /// Website
        /// </summary>
        public string Website { get; set; }
        
        /// <summary>
        /// Đánh giá trung bình
        /// </summary>
        public double Rating { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Có hiển thị trên bản đồ không
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }

    public class MapSearchRequest
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double Radius { get; set; } = 10; // km
        public string LocationType { get; set; }
        public string SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class MapSearchResult
    {
        public List<Location> Locations { get; set; } = new();
        public int TotalCount { get; set; }
        public double CenterLatitude { get; set; }
        public double CenterLongitude { get; set; }
        public double SearchRadius { get; set; }
    }

    public class RouteRequest
    {
        public double StartLatitude { get; set; }
        public double StartLongitude { get; set; }
        public double EndLatitude { get; set; }
        public double EndLongitude { get; set; }
        public string TravelMode { get; set; } = "driving"; // driving, walking, transit
    }

    public class RouteResponse
    {
        public string EncodedPolyline { get; set; }
        public double DistanceKm { get; set; }
        public int DurationMinutes { get; set; }
        public List<RouteStep> Steps { get; set; } = new();
    }

    public class RouteStep
    {
        public string Instruction { get; set; }
        public double DistanceKm { get; set; }
        public int DurationMinutes { get; set; }
        public double StartLatitude { get; set; }
        public double StartLongitude { get; set; }
        public double EndLatitude { get; set; }
        public double EndLongitude { get; set; }
    }
}
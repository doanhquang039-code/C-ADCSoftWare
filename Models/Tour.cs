using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    public class Tour
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public int Price { get; set; }

        public int Duration { get; set; }

        public string Image { get; set; }
        
        // Alias for Image to support ImageUrl property
        public string ImageUrl => Image;

        public int? DestinationId { get; set; }
        public Destination Destination { get; set; }
        
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public int Quantity { get; set; }

        public decimal Rating => Reviews != null && Reviews.Any(r => decimal.TryParse(r.Rating, out _))
            ? Reviews.Where(r => decimal.TryParse(r.Rating, out _)).Average(r => decimal.Parse(r.Rating))
            : 0;

        public bool Available => Quantity > 0;

        public int MaxGroupSize => Quantity > 0 ? Quantity : 10;

        public string Location => Destination?.Location ?? string.Empty;

        public ICollection<Orders> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Hotel> Hotels { get; set; }
    }

}


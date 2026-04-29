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

        public ICollection<Orders> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Hotel> Hotels { get; set; }
    }

}

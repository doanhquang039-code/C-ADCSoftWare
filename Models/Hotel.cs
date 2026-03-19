using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    public class Hotel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Address { get; set; }

        public int  Price { get; set; }

        public string Image { get; set; }

        public int Rating { get; set; }   


        public int? TourId { get; set; }
        public Tour Tour { get; set; }
        public int Quantity { get; set; }

       
    }

}

using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    public class Destination
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        public string Image { get; set; }

        public string Location { get; set; }

        public int? CategoryId { get; set; }
        public Category Category { get; set; }
        // Category thứ nhất là trong model còn Category thứ hai là Dữ liệu để tương tác tới model chứ ko liên quan gì đến bảng sql nhé.
        public ICollection<Tour> Tours { get; set; }
    }

}

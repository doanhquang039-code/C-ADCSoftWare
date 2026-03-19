using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int Age { get; set; }

        public string Gender { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public ICollection<Orders> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }

}

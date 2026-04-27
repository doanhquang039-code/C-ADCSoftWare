namespace WEBDULICH.Models
{
    public class Review
    {
        public int Id { get; set; }

        public string Rating { get; set; }

        public string Comment { get; set; }

        public DateTime ReviewDate { get; set; }  

        public int? UserId { get; set; }
        public User User { get; set; }

        public int? TourId { get; set; }
        public Tour Tour { get; set; }

        /// <summary>
        /// Ảnh đính kèm trong review
        /// </summary>
        public ICollection<ReviewImage> Images { get; set; }
    }

}

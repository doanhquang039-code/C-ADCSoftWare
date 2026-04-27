namespace WEBDULICH.Models
{
    /// <summary>
    /// Ảnh đính kèm trong review
    /// </summary>
    public class ReviewImage
    {
        public int Id { get; set; }

        public int ReviewId { get; set; }
        public Review Review { get; set; }

        public string ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

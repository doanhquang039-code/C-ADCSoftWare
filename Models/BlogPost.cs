using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    /// <summary>
    /// Blog/Travel Guide - bài viết hướng dẫn du lịch
    /// </summary>
    public class BlogPost
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string Summary { get; set; }

        public string FeaturedImage { get; set; }

        /// <summary>
        /// SEO-friendly URL slug
        /// </summary>
        [Required]
        public string Slug { get; set; }

        public int AuthorId { get; set; }
        public User Author { get; set; }

        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        public int? DestinationId { get; set; }
        public Destination Destination { get; set; }

        /// <summary>
        /// Tags phân cách bằng dấu phẩy
        /// </summary>
        public string Tags { get; set; }

        public int ViewCount { get; set; } = 0;

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? PublishedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

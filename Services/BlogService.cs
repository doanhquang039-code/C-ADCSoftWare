using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class BlogService : IBlogService
    {
        private readonly ApplicationDbContext _context;

        public BlogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BlogPost> GetByIdAsync(int id)
        {
            return await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Destination)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<BlogPost> GetBySlugAsync(string slug)
        {
            return await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Destination)
                .FirstOrDefaultAsync(b => b.Slug == slug);
        }

        public async Task<List<BlogPost>> GetPublishedPostsAsync(int page = 1, int pageSize = 10)
        {
            return await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> GetPostsByCategoryAsync(int categoryId)
        {
            return await _context.BlogPosts
                .Include(b => b.Author)
                .Where(b => b.CategoryId == categoryId && b.IsPublished)
                .OrderByDescending(b => b.PublishedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> GetPostsByDestinationAsync(int destinationId)
        {
            return await _context.BlogPosts
                .Include(b => b.Author)
                .Where(b => b.DestinationId == destinationId && b.IsPublished)
                .OrderByDescending(b => b.PublishedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> SearchPostsAsync(string keyword)
        {
            return await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.IsPublished && 
                    (b.Title.Contains(keyword) || 
                     b.Content.Contains(keyword) || 
                     b.Tags.Contains(keyword)))
                .OrderByDescending(b => b.PublishedAt)
                .ToListAsync();
        }

        public async Task<BlogPost> CreatePostAsync(BlogPost post)
        {
            post.CreatedAt = DateTime.Now;
            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<bool> UpdatePostAsync(BlogPost post)
        {
            var existing = await _context.BlogPosts.FindAsync(post.Id);
            if (existing == null) return false;

            existing.Title = post.Title;
            existing.Content = post.Content;
            existing.Summary = post.Summary;
            existing.FeaturedImage = post.FeaturedImage;
            existing.Slug = post.Slug;
            existing.CategoryId = post.CategoryId;
            existing.DestinationId = post.DestinationId;
            existing.Tags = post.Tags;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePostAsync(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return false;

            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishPostAsync(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return false;

            post.IsPublished = true;
            post.PublishedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnpublishPostAsync(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return false;

            post.IsPublished = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementViewCountAsync(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return false;

            post.ViewCount++;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

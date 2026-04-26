using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext db;

        public ReviewService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            return await db.Reviews
                .Include(r => r.User)
                .Include(r => r.Tour)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<PagedResult<Review>> GetPagedAsync(string? keyword, int? tourId, string? rating,
            string? sortBy, string? sortDir, int page, int pageSize)
        {
            var query = db.Reviews
                .Include(r => r.User)
                .Include(r => r.Tour)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(r => r.Comment.Contains(keyword) ||
                    (r.User != null && r.User.Name.Contains(keyword)));

            if (tourId.HasValue)
                query = query.Where(r => r.TourId == tourId.Value);

            if (!string.IsNullOrWhiteSpace(rating))
                query = query.Where(r => r.Rating == rating);

            query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
            {
                ("date", "asc") => query.OrderBy(r => r.ReviewDate),
                ("date", _) => query.OrderByDescending(r => r.ReviewDate),
                ("rating", "desc") => query.OrderByDescending(r => r.Rating),
                ("rating", _) => query.OrderBy(r => r.Rating),
                _ => query.OrderByDescending(r => r.ReviewDate)
            };

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Review>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Keyword = keyword,
                SortBy = sortBy,
                SortDir = sortDir,
                Filters = new Dictionary<string, string?>
                {
                    ["tourId"] = tourId?.ToString(),
                    ["rating"] = rating
                }
            };
        }

        public async Task CreateAsync(Review review)
        {
            if (review.ReviewDate == DateTime.MinValue)
                review.ReviewDate = DateTime.Now;

            db.Reviews.Add(review);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Review review)
        {
            db.Reviews.Update(review);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var review = await db.Reviews.FindAsync(id);
            if (review == null) return;

            db.Reviews.Remove(review);
            await db.SaveChangesAsync();
        }
    }
}

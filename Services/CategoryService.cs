using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext db;

        public CategoryService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await db.Categories.Include(c => c.Destinations).ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await db.Categories.Include(c => c.Destinations).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<PagedResult<Category>> GetPagedAsync(string? keyword, string? sortBy, string? sortDir, int page, int pageSize)
        {
            var query = db.Categories.Include(c => c.Destinations).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(c => c.Name.Contains(keyword));

            query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
            {
                ("name", "desc") => query.OrderByDescending(c => c.Name),
                ("name", _) => query.OrderBy(c => c.Name),
                _ => query.OrderByDescending(c => c.Id)
            };

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Category>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Keyword = keyword,
                SortBy = sortBy,
                SortDir = sortDir
            };
        }

        public async Task<bool> ExistsAsync(string name, int? excludeId = null)
        {
            var normalizedName = name.Trim().ToLower();
            var query = db.Categories.Where(c => c.Name.ToLower() == normalizedName);
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> CreateAsync(Category category)
        {
            if (await ExistsAsync(category.Name))
                return false;

            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task UpdateAsync(Category category)
        {
            db.Categories.Update(category);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await db.Categories.FindAsync(id);
            if (category == null) return;

            db.Categories.Remove(category);
            await db.SaveChangesAsync();
        }
    }
}

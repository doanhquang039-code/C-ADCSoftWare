using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class DestinationService : IDestinationService
    {
        private readonly ApplicationDbContext db;
        private readonly IImageStorageService imageStorageService;

        public DestinationService(ApplicationDbContext db, IImageStorageService imageStorageService)
        {
            this.db = db;
            this.imageStorageService = imageStorageService;
        }

        public async Task<List<Destination>> GetAllAsync()
        {
            return await db.Destinations.Include(d => d.Category).Include(d => d.Tours).ToListAsync();
        }

        public async Task<Destination?> GetByIdAsync(int id)
        {
            return await db.Destinations
                .Include(d => d.Category)
                .Include(d => d.Tours)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<PagedResult<Destination>> GetPagedAsync(string? keyword, int? categoryId, string? location,
            string? sortBy, string? sortDir, int page, int pageSize)
        {
            var query = db.Destinations.Include(d => d.Category).Include(d => d.Tours).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(d => d.Name.Contains(keyword) || d.Description.Contains(keyword));

            if (categoryId.HasValue)
                query = query.Where(d => d.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(d => d.Location.Contains(location));

            query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
            {
                ("name", "desc") => query.OrderByDescending(d => d.Name),
                ("name", _) => query.OrderBy(d => d.Name),
                ("location", "desc") => query.OrderByDescending(d => d.Location),
                ("location", _) => query.OrderBy(d => d.Location),
                _ => query.OrderByDescending(d => d.Id)
            };

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Destination>
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
                    ["categoryId"] = categoryId?.ToString(),
                    ["location"] = location
                }
            };
        }

        public async Task CreateAsync(Destination destination, IFormFile? imageFile)
        {
            destination.Image = await imageStorageService.SaveAsync(imageFile, "ImageDestination") ?? string.Empty;
            db.Destinations.Add(destination);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Destination destination, IFormFile? imageFile)
        {
            var existing = await db.Destinations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == destination.Id);
            if (existing == null) return;

            var newImage = await imageStorageService.SaveAsync(imageFile, "ImageDestination");
            if (!string.IsNullOrWhiteSpace(newImage))
            {
                imageStorageService.Delete("ImageDestination", existing.Image);
                destination.Image = newImage;
            }
            else
            {
                destination.Image = existing.Image;
            }

            db.Destinations.Update(destination);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var destination = await db.Destinations.FindAsync(id);
            if (destination == null) return;

            imageStorageService.Delete("ImageDestination", destination.Image);
            db.Destinations.Remove(destination);
            await db.SaveChangesAsync();
        }
    }
}

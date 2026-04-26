using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class TourService : ITourService
    {
        private readonly ApplicationDbContext db;
        private readonly IImageStorageService imageStorageService;

        public TourService(ApplicationDbContext db, IImageStorageService imageStorageService)
        {
            this.db = db;
            this.imageStorageService = imageStorageService;
        }

        public async Task<List<Tour>> GetAllAsync()
        {
            return await db.Tours
                .Include(t => t.Destination)
                .Include(t => t.Orders)
                .Include(t => t.Reviews)
                .Include(t => t.Hotels)
                .ToListAsync();
        }

        public async Task<Tour?> GetByIdAsync(int id)
        {
            return await db.Tours
                .Include(t => t.Destination)
                .Include(t => t.Hotels)
                .Include(t => t.Reviews)
                .Include(t => t.Orders)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<PagedResult<Tour>> GetPagedAsync(string? keyword, decimal? minPrice, decimal? maxPrice,
            int? duration, int? destinationId, string? sortBy, string? sortDir, int page, int pageSize)
        {
            var query = db.Tours
                .Include(t => t.Destination)
                .Include(t => t.Orders)
                .Include(t => t.Reviews)
                .Include(t => t.Hotels)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(t => t.Name.Contains(keyword) || t.Description.Contains(keyword));
            }

            // Filters
            if (minPrice.HasValue)
                query = query.Where(t => t.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(t => t.Price <= maxPrice.Value);

            if (duration.HasValue)
                query = query.Where(t => t.Duration == duration.Value);

            if (destinationId.HasValue)
                query = query.Where(t => t.DestinationId == destinationId.Value);

            // Sort
            query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
            {
                ("name", "desc") => query.OrderByDescending(t => t.Name),
                ("name", _) => query.OrderBy(t => t.Name),
                ("price", "desc") => query.OrderByDescending(t => t.Price),
                ("price", _) => query.OrderBy(t => t.Price),
                ("duration", "desc") => query.OrderByDescending(t => t.Duration),
                ("duration", _) => query.OrderBy(t => t.Duration),
                ("quantity", "desc") => query.OrderByDescending(t => t.Quantity),
                ("quantity", _) => query.OrderBy(t => t.Quantity),
                _ => query.OrderByDescending(t => t.Id)
            };

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Tour>
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
                    ["minPrice"] = minPrice?.ToString(),
                    ["maxPrice"] = maxPrice?.ToString(),
                    ["duration"] = duration?.ToString(),
                    ["destinationId"] = destinationId?.ToString()
                }
            };
        }

        public async Task CreateAsync(Tour tour, IFormFile? imageFile)
        {
            tour.Image = await imageStorageService.SaveAsync(imageFile, "ImageTour") ?? string.Empty;
            db.Tours.Add(tour);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Tour tour, IFormFile? imageFile, string? oldImage)
        {
            var existingTour = await db.Tours.AsNoTracking().FirstOrDefaultAsync(x => x.Id == tour.Id);
            if (existingTour == null) return;

            var newImage = await imageStorageService.SaveAsync(imageFile, "ImageTour");
            if (!string.IsNullOrWhiteSpace(newImage))
            {
                imageStorageService.Delete("ImageTour", oldImage ?? existingTour.Image);
                tour.Image = newImage;
            }
            else
            {
                tour.Image = oldImage ?? existingTour.Image;
            }

            db.Tours.Update(tour);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var tour = await db.Tours.FindAsync(id);
            if (tour == null) return;

            imageStorageService.Delete("ImageTour", tour.Image);
            db.Tours.Remove(tour);
            await db.SaveChangesAsync();
        }
    }
}

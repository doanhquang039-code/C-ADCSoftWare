using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class HotelService : IHotelService
    {
        private readonly ApplicationDbContext db;
        private readonly IImageStorageService imageStorageService;

        public HotelService(ApplicationDbContext db, IImageStorageService imageStorageService)
        {
            this.db = db;
            this.imageStorageService = imageStorageService;
        }

        public async Task<List<Hotel>> GetAllAsync()
        {
            return await db.Hotels.Include(h => h.Tour).ToListAsync();
        }

        public async Task<Hotel?> GetByIdAsync(int id)
        {
            return await db.Hotels.Include(h => h.Tour).FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<PagedResult<Hotel>> GetPagedAsync(string? keyword, decimal? minPrice, decimal? maxPrice,
            int? rating, int? tourId, string? address, string? sortBy, string? sortDir, int page, int pageSize)
        {
            var query = db.Hotels.Include(h => h.Tour).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(h => h.Name.Contains(keyword));

            if (!string.IsNullOrWhiteSpace(address))
                query = query.Where(h => h.Address.Contains(address));

            if (minPrice.HasValue)
                query = query.Where(h => h.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(h => h.Price <= maxPrice.Value);

            if (rating.HasValue)
                query = query.Where(h => h.Rating == rating.Value);

            if (tourId.HasValue)
                query = query.Where(h => h.TourId == tourId.Value);

            query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
            {
                ("name", "desc") => query.OrderByDescending(h => h.Name),
                ("name", _) => query.OrderBy(h => h.Name),
                ("price", "desc") => query.OrderByDescending(h => h.Price),
                ("price", _) => query.OrderBy(h => h.Price),
                ("rating", "desc") => query.OrderByDescending(h => h.Rating),
                ("rating", _) => query.OrderBy(h => h.Rating),
                ("quantity", "desc") => query.OrderByDescending(h => h.Quantity),
                ("quantity", _) => query.OrderBy(h => h.Quantity),
                _ => query.OrderByDescending(h => h.Id)
            };

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Hotel>
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
                    ["rating"] = rating?.ToString(),
                    ["tourId"] = tourId?.ToString(),
                    ["address"] = address
                }
            };
        }

        public async Task CreateAsync(Hotel hotel, IFormFile? imageFile)
        {
            hotel.Image = await imageStorageService.SaveAsync(imageFile, "ImageHotel") ?? string.Empty;
            db.Hotels.Add(hotel);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Hotel hotel, IFormFile? imageFile)
        {
            var existingHotel = await db.Hotels.AsNoTracking().FirstOrDefaultAsync(x => x.Id == hotel.Id);
            if (existingHotel == null) return;

            var newImage = await imageStorageService.SaveAsync(imageFile, "ImageHotel");
            if (!string.IsNullOrWhiteSpace(newImage))
            {
                imageStorageService.Delete("ImageHotel", existingHotel.Image);
                hotel.Image = newImage;
            }
            else
            {
                hotel.Image = existingHotel.Image;
            }

            db.Hotels.Update(hotel);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var hotel = await db.Hotels.FindAsync(id);
            if (hotel == null) return;

            imageStorageService.Delete("ImageHotel", hotel.Image);
            db.Hotels.Remove(hotel);
            await db.SaveChangesAsync();
        }
    }
}

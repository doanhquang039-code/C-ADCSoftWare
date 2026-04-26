using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface ITourService
    {
        Task<PagedResult<Tour>> GetPagedAsync(string? keyword, decimal? minPrice, decimal? maxPrice,
            int? duration, int? destinationId, string? sortBy, string? sortDir, int page, int pageSize);
        Task<Tour?> GetByIdAsync(int id);
        Task<List<Tour>> GetAllAsync();
        Task CreateAsync(Tour tour, IFormFile? imageFile);
        Task UpdateAsync(Tour tour, IFormFile? imageFile, string? oldImage);
        Task DeleteAsync(int id);
    }
}

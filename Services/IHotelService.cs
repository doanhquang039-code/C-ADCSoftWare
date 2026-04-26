using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface IHotelService
    {
        Task<PagedResult<Hotel>> GetPagedAsync(string? keyword, decimal? minPrice, decimal? maxPrice,
            int? rating, int? tourId, string? address, string? sortBy, string? sortDir, int page, int pageSize);
        Task<Hotel?> GetByIdAsync(int id);
        Task<List<Hotel>> GetAllAsync();
        Task CreateAsync(Hotel hotel, IFormFile? imageFile);
        Task UpdateAsync(Hotel hotel, IFormFile? imageFile);
        Task DeleteAsync(int id);
    }
}

using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface IDestinationService
    {
        Task<PagedResult<Destination>> GetPagedAsync(string? keyword, int? categoryId, string? location,
            string? sortBy, string? sortDir, int page, int pageSize);
        Task<Destination?> GetByIdAsync(int id);
        Task<List<Destination>> GetAllAsync();
        Task CreateAsync(Destination destination, IFormFile? imageFile);
        Task UpdateAsync(Destination destination, IFormFile? imageFile);
        Task DeleteAsync(int id);
    }
}

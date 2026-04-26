using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface ICategoryService
    {
        Task<PagedResult<Category>> GetPagedAsync(string? keyword, string? sortBy, string? sortDir, int page, int pageSize);
        Task<Category?> GetByIdAsync(int id);
        Task<List<Category>> GetAllAsync();
        Task<bool> CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(string name, int? excludeId = null);
    }
}

using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface IReviewService
    {
        Task<PagedResult<Review>> GetPagedAsync(string? keyword, int? tourId, string? rating,
            string? sortBy, string? sortDir, int page, int pageSize);
        Task<Review?> GetByIdAsync(int id);
        Task CreateAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(int id);
    }
}

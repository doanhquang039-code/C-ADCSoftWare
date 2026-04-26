using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface IPaymentService
    {
        Task<PagedResult<Payment>> GetPagedAsync(string? keyword, string? paymentStatus,
            string? sortBy, string? sortDir, int page, int pageSize);
        Task<Payment?> GetByIdAsync(int id);
        Task CreateAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task DeleteAsync(int id);
        Task<bool> ConfirmPaymentAsync(int orderId);
    }
}

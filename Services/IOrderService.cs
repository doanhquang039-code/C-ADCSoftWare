using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface IOrderService
    {
        Task<PagedResult<Orders>> GetPagedAsync(string? keyword, string? status, int? userId,
            string? sortBy, string? sortDir, int page, int pageSize);
        Task<Orders?> GetByIdAsync(int id);
        Task<List<Orders>> GetByUserIdAsync(int userId);
        Task<Orders> CreateTourOrderAsync(int userId, string email, int tourId, int quantity, string paymentMethod);
        Task<Orders> CreateHotelOrderAsync(int userId, string email, int hotelId, int quantity, string paymentMethod);
        Task<bool> ConfirmTransferAsync(int orderId, string email, DateTime departureDate);
        Task<List<Orders>> GetPaidOrdersAsync();
        Task<int> GetOrderCountAsync(int userId);
    }
}

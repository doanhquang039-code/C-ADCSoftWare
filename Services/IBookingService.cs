using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(Booking booking);
        Task<Booking> GetBookingByIdAsync(int id);
        Task<List<Booking>> GetUserBookingsAsync(int userId);
        Task<List<Booking>> GetAllBookingsAsync();
        Task<bool> UpdateBookingStatusAsync(int id, string status);
        Task<bool> CancelBookingAsync(int id);
        Task<bool> CheckAvailabilityAsync(string bookingType, int itemId, DateTime startDate, DateTime? endDate);
        Task<decimal> CalculateTotalPriceAsync(string bookingType, int itemId, int adults, int children, int rooms, DateTime startDate, DateTime? endDate);
    }
}

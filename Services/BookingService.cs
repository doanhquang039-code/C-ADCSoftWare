using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            booking.CreatedAt = DateTime.Now;
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Tour)
                .Include(b => b.Hotel)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Booking>> GetUserBookingsAsync(int userId)
        {
            return await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.Hotel)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Tour)
                .Include(b => b.Hotel)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateBookingStatusAsync(int id, string status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return false;

            booking.Status = status;
            booking.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelBookingAsync(int id)
        {
            return await UpdateBookingStatusAsync(id, "Cancelled");
        }

        public async Task<bool> CheckAvailabilityAsync(string bookingType, int itemId, DateTime startDate, DateTime? endDate)
        {
            // Check if tour/hotel has enough capacity
            if (bookingType == "Tour")
            {
                var tour = await _context.Tours.FindAsync(itemId);
                if (tour == null) return false;

                var bookedCount = await _context.Bookings
                    .Where(b => b.TourId == itemId && b.StartDate == startDate && b.Status != "Cancelled")
                    .SumAsync(b => b.Adults + b.Children);

                return bookedCount < tour.Quantity;
            }
            else if (bookingType == "Hotel")
            {
                var hotel = await _context.Hotels.FindAsync(itemId);
                if (hotel == null) return false;

                var bookedRooms = await _context.Bookings
                    .Where(b => b.HotelId == itemId 
                        && b.Status != "Cancelled"
                        && b.StartDate < endDate 
                        && b.EndDate > startDate)
                    .SumAsync(b => b.Rooms);

                return bookedRooms < hotel.Quantity;
            }

            return false;
        }

        public async Task<decimal> CalculateTotalPriceAsync(string bookingType, int itemId, int adults, int children, int rooms, DateTime startDate, DateTime? endDate)
        {
            decimal basePrice = 0;
            int nights = 1;

            if (bookingType == "Tour")
            {
                var tour = await _context.Tours.FindAsync(itemId);
                if (tour == null) return 0;
                basePrice = tour.Price * (adults + children * 0.5m);
            }
            else if (bookingType == "Hotel")
            {
                var hotel = await _context.Hotels.FindAsync(itemId);
                if (hotel == null) return 0;
                
                if (endDate.HasValue)
                {
                    nights = (endDate.Value - startDate).Days;
                    if (nights < 1) nights = 1;
                }
                
                basePrice = hotel.Price * rooms * nights;
            }

            return basePrice;
        }
    }
}

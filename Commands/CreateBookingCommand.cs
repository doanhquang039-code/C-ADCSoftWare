using MediatR;
using WEBDULICH.Models;
using WEBDULICH.Services;
using Microsoft.EntityFrameworkCore;

namespace WEBDULICH.Commands
{
    public class CreateBookingCommand : IRequest<Booking>
    {
        public int TourId { get; set; }
        public int UserId { get; set; }
        public int NumberOfPeople { get; set; }
        public DateTime BookingDate { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string? SpecialRequests { get; set; }
    }

    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Booking>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateBookingCommandHandler> _logger;

        public CreateBookingCommandHandler(ApplicationDbContext context, ILogger<CreateBookingCommandHandler> _logger)
        {
            _context = context;
            this._logger = _logger;
        }

        public async Task<Booking> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Creating booking for tour {request.TourId} by user {request.UserId}");

            var tour = await _context.Tours.FindAsync(new object[] { request.TourId }, cancellationToken);
            if (tour == null)
            {
                throw new Exception("Tour not found");
            }

            var booking = new Booking
            {
                TourId = request.TourId,
                UserId = request.UserId,
                // Map other properties as they exist in the Booking model
                TotalPrice = tour.Price * request.NumberOfPeople,
                CreatedAt = DateTime.Now
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Booking created successfully with ID {booking.Id}");

            return booking;
        }
    }
}

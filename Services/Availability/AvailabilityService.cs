using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services.Availability
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AvailabilityService> _logger;

        public AvailabilityService(
            ApplicationDbContext context,
            ILogger<AvailabilityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Models.Availability> GetAvailabilityAsync(string entityType, int entityId, DateTime date)
        {
            var availability = await _context.Availabilities
                .FirstOrDefaultAsync(a => 
                    a.EntityType == entityType && 
                    (entityType == "Tour" ? a.TourId == entityId : a.HotelId == entityId) &&
                    a.Date.Date == date.Date);

            if (availability == null)
            {
                // Create default availability
                availability = await CreateDefaultAvailabilityAsync(entityType, entityId, date);
            }

            return availability;
        }

        private async Task<Models.Availability> CreateDefaultAvailabilityAsync(string entityType, int entityId, DateTime date)
        {
            int defaultCapacity = 20; // Default
            decimal basePrice = 0;

            if (entityType == "Tour")
            {
                var tour = await _context.Tours.FindAsync(entityId);
                if (tour != null)
                {
                    defaultCapacity = tour.Quantity;
                    basePrice = tour.Price;
                }
            }
            else if (entityType == "Hotel")
            {
                var hotel = await _context.Hotels.FindAsync(entityId);
                if (hotel != null)
                {
                    defaultCapacity = 50; // Default hotel capacity
                    basePrice = hotel.Price;
                }
            }

            var availability = new Models.Availability
            {
                EntityType = entityType,
                TourId = entityType == "Tour" ? entityId : null,
                HotelId = entityType == "Hotel" ? entityId : null,
                Date = date.Date,
                TotalCapacity = defaultCapacity,
                BookedCapacity = 0,
                AvailableCapacity = defaultCapacity,
                HoldCapacity = 0,
                OccupancyRate = 0,
                CurrentPrice = basePrice,
                BasePrice = basePrice,
                Status = "Available",
                DemandLevel = "Low"
            };

            _context.Availabilities.Add(availability);
            await _context.SaveChangesAsync();

            return availability;
        }

        public async Task<List<Models.Availability>> GetAvailabilityRangeAsync(string entityType, int entityId, DateTime startDate, DateTime endDate)
        {
            var availabilities = new List<Models.Availability>();
            
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var availability = await GetAvailabilityAsync(entityType, entityId, date);
                availabilities.Add(availability);
            }

            return availabilities;
        }

        public async Task<Models.Availability> UpdateAvailabilityAsync(Models.Availability availability)
        {
            availability.AvailableCapacity = availability.TotalCapacity - availability.BookedCapacity - availability.HoldCapacity;
            availability.OccupancyRate = availability.TotalCapacity > 0 
                ? (decimal)availability.BookedCapacity / availability.TotalCapacity * 100 
                : 0;

            // Update status
            if (availability.AvailableCapacity <= 0)
                availability.Status = "Full";
            else if (availability.AvailableCapacity <= availability.TotalCapacity * 0.2m)
                availability.Status = "Limited";
            else
                availability.Status = "Available";

            availability.LastUpdated = DateTime.Now;

            _context.Availabilities.Update(availability);
            await _context.SaveChangesAsync();

            return availability;
        }

        public async Task<bool> CheckAvailabilityAsync(string entityType, int entityId, DateTime date, int quantity)
        {
            var availability = await GetAvailabilityAsync(entityType, entityId, date);
            return availability.AvailableCapacity >= quantity;
        }

        public async Task<AvailabilityBlock> CreateBlockAsync(AvailabilityBlock block)
        {
            // Set expiry time (15 minutes default)
            block.ExpiresAt = DateTime.Now.AddMinutes(15);
            block.Status = "Active";

            _context.AvailabilityBlocks.Add(block);

            // Update availability
            var availability = await _context.Availabilities.FindAsync(block.AvailabilityId);
            if (availability != null)
            {
                availability.HoldCapacity += block.Quantity;
                await UpdateAvailabilityAsync(availability);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created availability block: {block.Id} for {block.Quantity} slots");

            return block;
        }

        public async Task<bool> ReleaseBlockAsync(int blockId)
        {
            var block = await _context.AvailabilityBlocks.FindAsync(blockId);
            if (block == null || block.Status != "Active") return false;

            block.Status = "Released";

            // Update availability
            var availability = await _context.Availabilities.FindAsync(block.AvailabilityId);
            if (availability != null)
            {
                availability.HoldCapacity -= block.Quantity;
                await UpdateAvailabilityAsync(availability);
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ConvertBlockToBookingAsync(int blockId, int bookingId)
        {
            var block = await _context.AvailabilityBlocks.FindAsync(blockId);
            if (block == null || block.Status != "Active") return false;

            block.Status = "Converted";
            block.BookingId = bookingId;

            // Update availability
            var availability = await _context.Availabilities.FindAsync(block.AvailabilityId);
            if (availability != null)
            {
                availability.HoldCapacity -= block.Quantity;
                availability.BookedCapacity += block.Quantity;
                await UpdateAvailabilityAsync(availability);
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task CleanupExpiredBlocksAsync()
        {
            var expiredBlocks = await _context.AvailabilityBlocks
                .Where(b => b.Status == "Active" && b.ExpiresAt < DateTime.Now)
                .ToListAsync();

            foreach (var block in expiredBlocks)
            {
                await ReleaseBlockAsync(block.Id);
            }

            _logger.LogInformation($"Cleaned up {expiredBlocks.Count} expired blocks");
        }

        public async Task<Dictionary<DateTime, Models.Availability>> GetCalendarAsync(string entityType, int entityId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var availabilities = await GetAvailabilityRangeAsync(entityType, entityId, startDate, endDate);

            return availabilities.ToDictionary(a => a.Date, a => a);
        }

        public async Task<List<DateTime>> GetAvailableDatesAsync(string entityType, int entityId, DateTime startDate, DateTime endDate)
        {
            var availabilities = await GetAvailabilityRangeAsync(entityType, entityId, startDate, endDate);

            return availabilities
                .Where(a => a.Status == "Available" && a.AvailableCapacity > 0)
                .Select(a => a.Date)
                .ToList();
        }

        public async Task<Dictionary<string, object>> GetOccupancyStatsAsync(string entityType, int entityId, DateTime startDate, DateTime endDate)
        {
            var availabilities = await GetAvailabilityRangeAsync(entityType, entityId, startDate, endDate);

            return new Dictionary<string, object>
            {
                ["totalDays"] = availabilities.Count,
                ["averageOccupancy"] = availabilities.Average(a => a.OccupancyRate),
                ["fullDays"] = availabilities.Count(a => a.Status == "Full"),
                ["limitedDays"] = availabilities.Count(a => a.Status == "Limited"),
                ["availableDays"] = availabilities.Count(a => a.Status == "Available"),
                ["totalCapacity"] = availabilities.Sum(a => a.TotalCapacity),
                ["totalBooked"] = availabilities.Sum(a => a.BookedCapacity),
                ["totalRevenue"] = availabilities.Sum(a => a.BookedCapacity * a.CurrentPrice)
            };
        }

        public async Task<string> GetDemandLevelAsync(string entityType, int entityId, DateTime date)
        {
            var availability = await GetAvailabilityAsync(entityType, entityId, date);

            // Calculate demand based on views and bookings
            var viewsToBookingsRatio = availability.BookingsLast24Hours > 0
                ? (decimal)availability.ViewsLast24Hours / availability.BookingsLast24Hours
                : 0;

            if (availability.OccupancyRate >= 90) return "VeryHigh";
            if (availability.OccupancyRate >= 70) return "High";
            if (availability.OccupancyRate >= 40) return "Medium";
            return "Low";
        }

        public async Task UpdateDemandLevelAsync(int availabilityId)
        {
            var availability = await _context.Availabilities.FindAsync(availabilityId);
            if (availability == null) return;

            availability.DemandLevel = await GetDemandLevelAsync(
                availability.EntityType,
                availability.TourId ?? availability.HotelId ?? 0,
                availability.Date
            );

            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<DateTime, decimal>> ForecastDemandAsync(string entityType, int entityId, int days)
        {
            var forecast = new Dictionary<DateTime, decimal>();
            var startDate = DateTime.Now.Date;

            // Simple forecasting based on historical data
            var historicalData = await _context.Availabilities
                .Where(a => a.EntityType == entityType &&
                           (entityType == "Tour" ? a.TourId == entityId : a.HotelId == entityId) &&
                           a.Date < startDate)
                .OrderByDescending(a => a.Date)
                .Take(30)
                .ToListAsync();

            var avgOccupancy = historicalData.Any() ? historicalData.Average(a => a.OccupancyRate) : 50;

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var dayOfWeek = date.DayOfWeek;

                // Adjust for weekends
                var multiplier = (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday) ? 1.3m : 1.0m;

                forecast[date] = avgOccupancy * multiplier;
            }

            return forecast;
        }

        public async Task<List<DateTime>> GetHighDemandDatesAsync(string entityType, int entityId, DateTime startDate, DateTime endDate)
        {
            var availabilities = await GetAvailabilityRangeAsync(entityType, entityId, startDate, endDate);

            return availabilities
                .Where(a => a.DemandLevel == "High" || a.DemandLevel == "VeryHigh")
                .Select(a => a.Date)
                .ToList();
        }
    }
}

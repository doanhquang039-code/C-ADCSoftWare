using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using System.Text.Json;

namespace WEBDULICH.Services.TourPackage
{
    public class TourPackageService : ITourPackageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TourPackageService> _logger;

        public TourPackageService(
            ApplicationDbContext context,
            ILogger<TourPackageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Models.TourPackage> CreatePackageAsync(Models.TourPackage package)
        {
            package.CreatedAt = DateTime.Now;
            _context.TourPackages.Add(package);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Created tour package: {package.Name}");
            return package;
        }

        public async Task<Models.TourPackage> GetPackageByIdAsync(int id)
        {
            return await _context.TourPackages
                .Include(p => p.Items)
                    .ThenInclude(i => i.Tour)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Hotel)
                .Include(p => p.User)
                .Include(p => p.Bookings)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Models.TourPackage>> GetAllPackagesAsync(string status = null, bool? isPublic = null)
        {
            var query = _context.TourPackages.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (isPublic.HasValue)
                query = query.Where(p => p.IsPublic == isPublic.Value);

            return await query
                .Include(p => p.Items)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Models.TourPackage> UpdatePackageAsync(Models.TourPackage package)
        {
            package.UpdatedAt = DateTime.Now;
            _context.TourPackages.Update(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<bool> DeletePackageAsync(int id)
        {
            var package = await _context.TourPackages.FindAsync(id);
            if (package == null) return false;

            package.Status = "Archived";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TourPackageItem> AddItemToPackageAsync(TourPackageItem item)
        {
            item.CreatedAt = DateTime.Now;
            _context.TourPackageItems.Add(item);
            await _context.SaveChangesAsync();

            // Recalculate package price
            await CalculatePackagePriceAsync(item.TourPackageId);

            return item;
        }

        public async Task<bool> RemoveItemFromPackageAsync(int itemId)
        {
            var item = await _context.TourPackageItems.FindAsync(itemId);
            if (item == null) return false;

            var packageId = item.TourPackageId;
            _context.TourPackageItems.Remove(item);
            await _context.SaveChangesAsync();

            // Recalculate package price
            await CalculatePackagePriceAsync(packageId);

            return true;
        }

        public async Task<List<TourPackageItem>> GetPackageItemsAsync(int packageId)
        {
            return await _context.TourPackageItems
                .Where(i => i.TourPackageId == packageId)
                .Include(i => i.Tour)
                .Include(i => i.Hotel)
                .OrderBy(i => i.DayNumber)
                .ThenBy(i => i.OrderInDay)
                .ToListAsync();
        }

        public async Task<TourPackageItem> UpdatePackageItemAsync(TourPackageItem item)
        {
            _context.TourPackageItems.Update(item);
            await _context.SaveChangesAsync();

            // Recalculate package price
            await CalculatePackagePriceAsync(item.TourPackageId);

            return item;
        }

        public async Task<Models.TourPackage> BuildCustomPackageAsync(int userId, List<TourPackageItem> items, string name, string description)
        {
            var package = new Models.TourPackage
            {
                Name = name,
                Description = description,
                UserId = userId,
                PackageType = "Custom",
                Status = "Draft",
                IsPublic = false,
                CreatedAt = DateTime.Now
            };

            _context.TourPackages.Add(package);
            await _context.SaveChangesAsync();

            // Add items
            foreach (var item in items)
            {
                item.TourPackageId = package.Id;
                await AddItemToPackageAsync(item);
            }

            // Calculate totals
            var days = items.Max(i => i.DayNumber);
            package.TotalDays = days;
            package.TotalNights = days - 1;

            await CalculatePackagePriceAsync(package.Id);

            _logger.LogInformation($"Built custom package for user {userId}: {name}");
            return package;
        }

        public async Task<decimal> CalculatePackagePriceAsync(int packageId)
        {
            var items = await GetPackageItemsAsync(packageId);
            var package = await _context.TourPackages.FindAsync(packageId);

            if (package == null) return 0;

            decimal totalPrice = items.Sum(i => i.Price);
            package.OriginalPrice = totalPrice;

            // Apply discount based on package size
            decimal discount = 0;
            if (items.Count >= 10) discount = 0.15m; // 15% off
            else if (items.Count >= 7) discount = 0.10m; // 10% off
            else if (items.Count >= 5) discount = 0.05m; // 5% off

            package.DiscountPercentage = discount * 100;
            package.FinalPrice = totalPrice * (1 - discount);

            await _context.SaveChangesAsync();

            return package.FinalPrice;
        }

        public async Task<Models.TourPackage> OptimizePackageAsync(int packageId)
        {
            var package = await GetPackageByIdAsync(packageId);
            if (package == null) return null;

            var items = package.Items.ToList();

            // Optimize by day
            var itemsByDay = items.GroupBy(i => i.DayNumber);

            foreach (var dayGroup in itemsByDay)
            {
                var dayItems = dayGroup.OrderBy(i => i.OrderInDay).ToList();

                // Reorder items logically
                for (int i = 0; i < dayItems.Count; i++)
                {
                    dayItems[i].OrderInDay = i + 1;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Optimized package {packageId}");
            return package;
        }

        public async Task<TourPackageBooking> CreateBookingAsync(TourPackageBooking booking)
        {
            booking.CreatedAt = DateTime.Now;
            _context.TourPackageBookings.Add(booking);
            await _context.SaveChangesAsync();

            // Update package booking count
            var package = await _context.TourPackages.FindAsync(booking.TourPackageId);
            if (package != null)
            {
                package.BookingCount++;
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Created package booking for user {booking.UserId}");
            return booking;
        }

        public async Task<TourPackageBooking> GetBookingByIdAsync(int id)
        {
            return await _context.TourPackageBookings
                .Include(b => b.TourPackage)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<TourPackageBooking>> GetUserBookingsAsync(int userId)
        {
            return await _context.TourPackageBookings
                .Where(b => b.UserId == userId)
                .Include(b => b.TourPackage)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<TourPackageBooking> UpdateBookingStatusAsync(int bookingId, string status)
        {
            var booking = await _context.TourPackageBookings.FindAsync(bookingId);
            if (booking == null) return null;

            booking.Status = status;
            booking.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return booking;
        }

        public async Task<List<Models.TourPackage>> GetUserPackagesAsync(int userId)
        {
            return await _context.TourPackages
                .Where(p => p.UserId == userId)
                .Include(p => p.Items)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Models.TourPackage> ClonePackageAsync(int packageId, int userId)
        {
            var original = await GetPackageByIdAsync(packageId);
            if (original == null) return null;

            var clone = new Models.TourPackage
            {
                Name = $"{original.Name} (Copy)",
                Description = original.Description,
                UserId = userId,
                PackageType = "Custom",
                Status = "Draft",
                IsPublic = false,
                TotalDays = original.TotalDays,
                TotalNights = original.TotalNights,
                MinPeople = original.MinPeople,
                MaxPeople = original.MaxPeople,
                Tags = original.Tags,
                CreatedAt = DateTime.Now
            };

            _context.TourPackages.Add(clone);
            await _context.SaveChangesAsync();

            // Clone items
            foreach (var item in original.Items)
            {
                var clonedItem = new TourPackageItem
                {
                    TourPackageId = clone.Id,
                    ItemType = item.ItemType,
                    TourId = item.TourId,
                    HotelId = item.HotelId,
                    DayNumber = item.DayNumber,
                    OrderInDay = item.OrderInDay,
                    Title = item.Title,
                    Description = item.Description,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime,
                    Price = item.Price,
                    Notes = item.Notes,
                    CreatedAt = DateTime.Now
                };
                _context.TourPackageItems.Add(clonedItem);
            }

            await _context.SaveChangesAsync();
            await CalculatePackagePriceAsync(clone.Id);

            _logger.LogInformation($"Cloned package {packageId} for user {userId}");
            return clone;
        }

        public async Task<List<Models.TourPackage>> GetPopularPackagesAsync(int count = 10)
        {
            return await _context.TourPackages
                .Where(p => p.IsPublic && p.Status == "Active")
                .OrderByDescending(p => p.BookingCount)
                .ThenByDescending(p => p.ViewCount)
                .Take(count)
                .Include(p => p.Items)
                .ToListAsync();
        }

        public async Task<List<Models.TourPackage>> GetRecommendedPackagesAsync(int userId, int count = 10)
        {
            // Get user's booking history
            var userBookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Tour)
                .ToListAsync();

            // Simple recommendation: packages with similar tours
            var tourIds = userBookings.Select(b => b.TourId).Distinct().ToList();

            var recommended = await _context.TourPackages
                .Where(p => p.IsPublic && p.Status == "Active" && p.UserId != userId)
                .Include(p => p.Items)
                .ToListAsync();

            // Score packages based on matching tours
            var scored = recommended.Select(p => new
            {
                Package = p,
                Score = p.Items.Count(i => i.TourId.HasValue && tourIds.Contains(i.TourId.Value))
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Package.AverageRating)
            .Take(count)
            .Select(x => x.Package)
            .ToList();

            return scored;
        }

        public async Task<Dictionary<string, object>> GetPackageStatisticsAsync(int packageId)
        {
            var package = await GetPackageByIdAsync(packageId);
            if (package == null) return null;

            var bookings = package.Bookings.ToList();

            return new Dictionary<string, object>
            {
                ["packageName"] = package.Name,
                ["totalViews"] = package.ViewCount,
                ["totalBookings"] = package.BookingCount,
                ["averageRating"] = package.AverageRating,
                ["totalRevenue"] = bookings.Sum(b => b.TotalPrice),
                ["conversionRate"] = package.ViewCount > 0 ? (decimal)package.BookingCount / package.ViewCount * 100 : 0,
                ["totalItems"] = package.Items.Count,
                ["totalDays"] = package.TotalDays,
                ["originalPrice"] = package.OriginalPrice,
                ["finalPrice"] = package.FinalPrice,
                ["discountPercentage"] = package.DiscountPercentage
            };
        }

        public async Task<Dictionary<string, object>> GetOverallPackageStatisticsAsync()
        {
            var packages = await _context.TourPackages.ToListAsync();
            var bookings = await _context.TourPackageBookings.ToListAsync();

            return new Dictionary<string, object>
            {
                ["totalPackages"] = packages.Count,
                ["activePackages"] = packages.Count(p => p.Status == "Active"),
                ["publicPackages"] = packages.Count(p => p.IsPublic),
                ["customPackages"] = packages.Count(p => p.PackageType == "Custom"),
                ["totalBookings"] = bookings.Count,
                ["totalRevenue"] = bookings.Sum(b => b.TotalPrice),
                ["averagePackagePrice"] = packages.Any() ? packages.Average(p => p.FinalPrice) : 0,
                ["averageRating"] = packages.Any() ? packages.Average(p => p.AverageRating) : 0
            };
        }
    }
}

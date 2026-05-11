using WEBDULICH.Models;

namespace WEBDULICH.Services.TourPackage
{
    public interface ITourPackageService
    {
        // Package CRUD
        Task<Models.TourPackage> CreatePackageAsync(Models.TourPackage package);
        Task<Models.TourPackage> GetPackageByIdAsync(int id);
        Task<List<Models.TourPackage>> GetAllPackagesAsync(string status = null, bool? isPublic = null);
        Task<Models.TourPackage> UpdatePackageAsync(Models.TourPackage package);
        Task<bool> DeletePackageAsync(int id);

        // Package Items
        Task<TourPackageItem> AddItemToPackageAsync(TourPackageItem item);
        Task<bool> RemoveItemFromPackageAsync(int itemId);
        Task<List<TourPackageItem>> GetPackageItemsAsync(int packageId);
        Task<TourPackageItem> UpdatePackageItemAsync(TourPackageItem item);

        // Package Builder
        Task<Models.TourPackage> BuildCustomPackageAsync(int userId, List<TourPackageItem> items, string name, string description);
        Task<decimal> CalculatePackagePriceAsync(int packageId);
        Task<Models.TourPackage> OptimizePackageAsync(int packageId);

        // Package Booking
        Task<TourPackageBooking> CreateBookingAsync(TourPackageBooking booking);
        Task<TourPackageBooking> GetBookingByIdAsync(int id);
        Task<List<TourPackageBooking>> GetUserBookingsAsync(int userId);
        Task<TourPackageBooking> UpdateBookingStatusAsync(int bookingId, string status);

        // User Packages
        Task<List<Models.TourPackage>> GetUserPackagesAsync(int userId);
        Task<Models.TourPackage> ClonePackageAsync(int packageId, int userId);

        // Popular & Recommendations
        Task<List<Models.TourPackage>> GetPopularPackagesAsync(int count = 10);
        Task<List<Models.TourPackage>> GetRecommendedPackagesAsync(int userId, int count = 10);

        // Statistics
        Task<Dictionary<string, object>> GetPackageStatisticsAsync(int packageId);
        Task<Dictionary<string, object>> GetOverallPackageStatisticsAsync();
    }
}

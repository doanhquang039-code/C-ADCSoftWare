using WEBDULICH.Models;

namespace WEBDULICH.Services.Availability
{
    public interface IAvailabilityService
    {
        // Availability Management
        Task<Models.Availability> GetAvailabilityAsync(string entityType, int entityId, DateTime date);
        Task<List<Models.Availability>> GetAvailabilityRangeAsync(string entityType, int entityId, DateTime startDate, DateTime endDate);
        Task<Models.Availability> UpdateAvailabilityAsync(Models.Availability availability);
        Task<bool> CheckAvailabilityAsync(string entityType, int entityId, DateTime date, int quantity);
        
        // Blocking
        Task<AvailabilityBlock> CreateBlockAsync(AvailabilityBlock block);
        Task<bool> ReleaseBlockAsync(int blockId);
        Task<bool> ConvertBlockToBookingAsync(int blockId, int bookingId);
        Task CleanupExpiredBlocksAsync();
        
        // Calendar
        Task<Dictionary<DateTime, Models.Availability>> GetCalendarAsync(string entityType, int entityId, int year, int month);
        Task<List<DateTime>> GetAvailableDatesAsync(string entityType, int entityId, DateTime startDate, DateTime endDate);
        
        // Analytics
        Task<Dictionary<string, object>> GetOccupancyStatsAsync(string entityType, int entityId, DateTime startDate, DateTime endDate);
        Task<string> GetDemandLevelAsync(string entityType, int entityId, DateTime date);
        Task UpdateDemandLevelAsync(int availabilityId);
        
        // Forecasting
        Task<Dictionary<DateTime, decimal>> ForecastDemandAsync(string entityType, int entityId, int days);
        Task<List<DateTime>> GetHighDemandDatesAsync(string entityType, int entityId, DateTime startDate, DateTime endDate);
    }
}

using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface IUserService
    {
        Task<PagedResult<User>> GetPagedAsync(string? keyword, string? role, bool? isActive,
            string? sortBy, string? sortDir, int page, int pageSize);
        Task<User?> GetByIdAsync(int id);
        Task<User?> AuthenticateAsync(string email, string password);
        Task<bool> RegisterAsync(User user);
        Task UpdateAsync(User user);
        Task<bool> ChangeRoleAsync(int userId, string newRole);
        Task<bool> ToggleActiveAsync(int userId);
        Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
        Task<DashboardStats> GetDashboardStatsAsync();
        
        // New methods
        Task<List<User>> GetByRoleAsync(string role);
        Task<List<User>> GetAdminsAsync();
        Task<List<User>> GetManagersAsync();
        Task<List<User>> GetHiringStaffAsync();
        Task<bool> UpdateProfileAsync(int userId, User updatedUser);
        Task<bool> UpdateLoyaltyPointsAsync(int userId, int points);
        Task<bool> UpdateMembershipTierAsync(int userId, string tier);
        Task<bool> VerifyEmailAsync(int userId);
        Task<bool> VerifyPhoneAsync(int userId);
        Task<bool> RecordLoginAsync(int userId);
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveUsersCountAsync();
        Task<Dictionary<string, int>> GetUsersByRoleStatsAsync();
        Task<Dictionary<string, int>> GetUsersByMembershipStatsAsync();
        Task<bool> UpdateUserAsync(User user);
    }

    public class DashboardStats
    {
        public int TotalUsers { get; set; }
        public int TotalTours { get; set; }
        public int TotalHotels { get; set; }
        public int TotalOrders { get; set; }
        public int PaidOrders { get; set; }
        public long TotalRevenue { get; set; }
        public int TotalDestinations { get; set; }
        public int TotalReviews { get; set; }
    }
}

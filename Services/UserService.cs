using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext db;

        public UserService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await db.Users.FindAsync(id);
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            if (user != null && !user.IsActive)
                return null; // Tài khoản bị khóa
            
            // Record login
            if (user != null)
            {
                await RecordLoginAsync(user.Id);
            }
            
            return user;
        }

        public async Task<bool> RegisterAsync(User user)
        {
            if (await EmailExistsAsync(user.Email))
                return false;

            user.Role = "User";
            user.CreatedAt = DateTime.Now;
            user.IsActive = true;
            user.MembershipTier = "Bronze";
            user.LoyaltyPoints = 0;
            user.LoginCount = 0;
            
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task UpdateAsync(User user)
        {
            var existingUser = await db.Users.FindAsync(user.Id);
            if (existingUser == null) return;

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Password = user.Password;
            existingUser.Gender = user.Gender;
            existingUser.Age = user.Age;
            existingUser.UpdatedAt = DateTime.Now;

            db.Users.Update(existingUser);
            await db.SaveChangesAsync();
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
        {
            var query = db.Users.Where(u => u.Email == email);
            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);
            return await query.AnyAsync();
        }

        public async Task<PagedResult<User>> GetPagedAsync(string? keyword, string? role, bool? isActive,
            string? sortBy, string? sortDir, int page, int pageSize)
        {
            var query = db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(u => u.Name.Contains(keyword) || u.Email.Contains(keyword));

            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role == role);

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
            {
                ("name", "desc") => query.OrderByDescending(u => u.Name),
                ("name", _) => query.OrderBy(u => u.Name),
                ("email", "desc") => query.OrderByDescending(u => u.Email),
                ("email", _) => query.OrderBy(u => u.Email),
                ("role", "desc") => query.OrderByDescending(u => u.Role),
                ("role", _) => query.OrderBy(u => u.Role),
                ("created", "asc") => query.OrderBy(u => u.CreatedAt),
                ("created", _) => query.OrderByDescending(u => u.CreatedAt),
                _ => query.OrderByDescending(u => u.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<User>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Keyword = keyword,
                SortBy = sortBy,
                SortDir = sortDir,
                Filters = new Dictionary<string, string?>
                {
                    ["role"] = role,
                    ["isActive"] = isActive?.ToString()
                }
            };
        }

        public async Task<bool> ChangeRoleAsync(int userId, string newRole)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null) return false;

            var validRoles = new[] { "Admin", "Manager", "Hiring", "User" };
            if (!validRoles.Contains(newRole)) return false;

            user.Role = newRole;
            user.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveAsync(int userId)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            return new DashboardStats
            {
                TotalUsers = await db.Users.CountAsync(),
                TotalTours = await db.Tours.CountAsync(),
                TotalHotels = await db.Hotels.CountAsync(),
                TotalOrders = await db.Orders.CountAsync(),
                PaidOrders = await db.Orders.CountAsync(o => o.Status == "Đã thanh toán"),
                TotalRevenue = await db.Orders.Where(o => o.Status == "Đã thanh toán").SumAsync(o => (long)o.TotalPrice),
                TotalDestinations = await db.Destinations.CountAsync(),
                TotalReviews = await db.Reviews.CountAsync()
            };
        }

        // New methods implementation
        public async Task<List<User>> GetByRoleAsync(string role)
        {
            return await db.Users
                .Where(u => u.Role == role && u.IsActive)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<List<User>> GetAdminsAsync()
        {
            return await GetByRoleAsync("Admin");
        }

        public async Task<List<User>> GetManagersAsync()
        {
            return await GetByRoleAsync("Manager");
        }

        public async Task<List<User>> GetHiringStaffAsync()
        {
            return await GetByRoleAsync("Hiring");
        }

        public async Task<bool> UpdateProfileAsync(int userId, User updatedUser)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null) return false;

            user.Name = updatedUser.Name;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.Address = updatedUser.Address;
            user.City = updatedUser.City;
            user.Country = updatedUser.Country;
            user.PostalCode = updatedUser.PostalCode;
            user.DateOfBirth = updatedUser.DateOfBirth;
            user.IdentityNumber = updatedUser.IdentityNumber;
            user.PassportNumber = updatedUser.PassportNumber;
            user.Nationality = updatedUser.Nationality;
            user.Occupation = updatedUser.Occupation;
            user.Company = updatedUser.Company;
            user.Notes = updatedUser.Notes;
            user.PreferredLanguage = updatedUser.PreferredLanguage;
            user.UpdatedAt = DateTime.Now;

            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLoyaltyPointsAsync(int userId, int points)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null) return false;

            user.LoyaltyPoints += points;
            
            // Auto update membership tier based on points
            user.MembershipTier = user.LoyaltyPoints switch
            {
                >= 10000 => "Platinum",
                >= 5000 => "Gold",
                >= 1000 => "Silver",
                _ => "Bronze"
            };

            user.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMembershipTierAsync(int userId, string tier)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null) return false;

            var validTiers = new[] { "Bronze", "Silver", "Gold", "Platinum" };
            if (!validTiers.Contains(tier)) return false;

            user.MembershipTier = tier;
            user.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifyEmailAsync(int userId)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null) return false;

            user.EmailVerified = true;
            user.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifyPhoneAsync(int userId)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null) return false;

            user.PhoneVerified = true;
            user.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RecordLoginAsync(int userId)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null) return false;

            user.LastLoginAt = DateTime.Now;
            user.LoginCount++;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await db.Users.CountAsync();
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            return await db.Users.CountAsync(u => u.IsActive);
        }

        public async Task<Dictionary<string, int>> GetUsersByRoleStatsAsync()
        {
            return await db.Users
                .GroupBy(u => u.Role)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Role, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetUsersByMembershipStatsAsync()
        {
            return await db.Users
                .GroupBy(u => u.MembershipTier)
                .Select(g => new { Tier = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Tier, x => x.Count);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            user.UpdatedAt = DateTime.Now;
            db.Users.Update(user);
            await db.SaveChangesAsync();
            return true;
        }
    }
}

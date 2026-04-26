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
            return user;
        }

        public async Task<bool> RegisterAsync(User user)
        {
            if (await EmailExistsAsync(user.Email))
                return false;

            user.Role = "User";
            user.CreatedAt = DateTime.Now;
            user.IsActive = true;
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

            var validRoles = new[] { "Admin", "Staff", "User" };
            if (!validRoles.Contains(newRole)) return false;

            user.Role = newRole;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveAsync(int userId)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = !user.IsActive;
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
    }
}

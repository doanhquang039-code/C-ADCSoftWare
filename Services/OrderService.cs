using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext db;

        public OrderService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<Orders?> GetByIdAsync(int id)
        {
            return await db.Orders
                .Include(o => o.Tour)
                .Include(o => o.Hotel)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Orders>> GetByUserIdAsync(int userId)
        {
            return await db.Orders
                .Include(o => o.Tour)
                .Include(o => o.Hotel)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<PagedResult<Orders>> GetPagedAsync(string? keyword, string? status, int? userId,
            string? sortBy, string? sortDir, int page, int pageSize)
        {
            var query = db.Orders
                .Include(o => o.Tour)
                .Include(o => o.Hotel)
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(o =>
                    (o.Tour != null && o.Tour.Name.Contains(keyword)) ||
                    (o.Hotel != null && o.Hotel.Name.Contains(keyword)) ||
                    (o.User != null && o.User.Name.Contains(keyword)) ||
                    (o.ConfirmedEmail != null && o.ConfirmedEmail.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(o => o.Status == status);

            if (userId.HasValue)
                query = query.Where(o => o.UserId == userId.Value);

            query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
            {
                ("date", "asc") => query.OrderBy(o => o.OrderDate),
                ("date", _) => query.OrderByDescending(o => o.OrderDate),
                ("price", "desc") => query.OrderByDescending(o => o.TotalPrice),
                ("price", _) => query.OrderBy(o => o.TotalPrice),
                ("status", "desc") => query.OrderByDescending(o => o.Status),
                ("status", _) => query.OrderBy(o => o.Status),
                _ => query.OrderByDescending(o => o.OrderDate)
            };

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Orders>
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
                    ["status"] = status,
                    ["userId"] = userId?.ToString()
                }
            };
        }

        public async Task<Orders> CreateTourOrderAsync(int userId, string email, int tourId, int quantity, string paymentMethod)
        {
            var tour = await db.Tours.FirstOrDefaultAsync(t => t.Id == tourId);
            if (tour == null || quantity <= 0 || tour.Quantity < quantity)
                throw new InvalidOperationException("Không đủ chỗ hoặc dữ liệu không hợp lệ!");

            var order = new Orders
            {
                UserId = userId,
                TourId = tour.Id,
                Quantity = quantity,
                OrderDate = DateTime.Now,
                TotalPrice = quantity * tour.Price,
                Status = "Chưa thanh toán",
                PaymentMethod = paymentMethod,
                ConfirmedEmail = email
            };

            tour.Quantity -= quantity;
            db.Orders.Add(order);
            await db.SaveChangesAsync();
            return order;
        }

        public async Task<Orders> CreateHotelOrderAsync(int userId, string email, int hotelId, int quantity, string paymentMethod)
        {
            var hotel = await db.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId);
            if (hotel == null || quantity <= 0 || hotel.Quantity < quantity)
                throw new InvalidOperationException("Không đủ phòng hoặc dữ liệu không hợp lệ!");

            var order = new Orders
            {
                UserId = userId,
                HotelId = hotel.Id,
                TourId = hotel.TourId,
                Quantity = quantity,
                OrderDate = DateTime.Now,
                TotalPrice = quantity * hotel.Price,
                Status = "Chưa thanh toán",
                PaymentMethod = paymentMethod,
                ConfirmedEmail = email
            };

            hotel.Quantity -= quantity;
            db.Orders.Add(order);
            await db.SaveChangesAsync();
            return order;
        }

        public async Task<bool> ConfirmTransferAsync(int orderId, string email, DateTime departureDate)
        {
            var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return false;

            order.Status = "Đã thanh toán";
            order.OrderDate = DateTime.Now;
            order.ConfirmedEmail = email;
            order.DepartureDate = departureDate;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<List<Orders>> GetPaidOrdersAsync()
        {
            return await db.Orders
                .Include(o => o.Tour)
                .Include(o => o.Hotel)
                .Include(o => o.User)
                .Where(o => o.Status == "Đã thanh toán")
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<int> GetOrderCountAsync(int userId)
        {
            return await db.Orders.CountAsync(o => o.UserId == userId);
        }
    }
}

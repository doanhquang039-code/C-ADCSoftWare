using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext db;

        public PaymentService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await db.Payments.Include(p => p.Orders).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PagedResult<Payment>> GetPagedAsync(string? keyword, string? paymentStatus,
            string? sortBy, string? sortDir, int page, int pageSize)
        {
            var query = db.Payments.Include(p => p.Orders).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => p.PaymentMethod.Contains(keyword) || p.PaymentStatus.Contains(keyword));

            if (!string.IsNullOrWhiteSpace(paymentStatus))
                query = query.Where(p => p.PaymentStatus == paymentStatus);

            query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
            {
                ("date", "asc") => query.OrderBy(p => p.PaymentDate),
                ("date", _) => query.OrderByDescending(p => p.PaymentDate),
                ("amount", "desc") => query.OrderByDescending(p => p.Amount),
                ("amount", _) => query.OrderBy(p => p.Amount),
                ("method", "desc") => query.OrderByDescending(p => p.PaymentMethod),
                ("method", _) => query.OrderBy(p => p.PaymentMethod),
                _ => query.OrderByDescending(p => p.PaymentDate)
            };

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Payment>
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
                    ["paymentStatus"] = paymentStatus
                }
            };
        }

        public async Task CreateAsync(Payment payment)
        {
            db.Payments.Add(payment);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payment payment)
        {
            db.Payments.Update(payment);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var payment = await db.Payments.FindAsync(id);
            if (payment == null) return;

            db.Payments.Remove(payment);
            await db.SaveChangesAsync();
        }

        public async Task<bool> ConfirmPaymentAsync(int orderId)
        {
            var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return false;

            order.Status = "Đã thanh toán";
            await db.SaveChangesAsync();
            return true;
        }
    }
}

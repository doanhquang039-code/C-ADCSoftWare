using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class CouponService : ICouponService
    {
        private readonly ApplicationDbContext _context;

        public CouponService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Coupon> GetCouponByCodeAsync(string code)
        {
            return await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == code);
        }

        public async Task<bool> ValidateCouponAsync(string code, decimal orderAmount)
        {
            var coupon = await GetCouponByCodeAsync(code);
            if (coupon == null) return false;

            return coupon.IsValid && orderAmount >= coupon.MinOrderAmount;
        }

        public async Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount)
        {
            var coupon = await GetCouponByCodeAsync(code);
            if (coupon == null || !coupon.IsValid) return 0;

            if (orderAmount < coupon.MinOrderAmount) return 0;

            if (coupon.DiscountType == "Percent")
            {
                return orderAmount * coupon.DiscountValue / 100;
            }
            else // Fixed
            {
                return coupon.DiscountValue;
            }
        }

        public async Task<bool> ApplyCouponAsync(string code)
        {
            var coupon = await GetCouponByCodeAsync(code);
            if (coupon == null || !coupon.IsValid) return false;

            coupon.UsedCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Coupon>> GetActiveCouponsAsync()
        {
            return await _context.Coupons
                .Where(c => c.IsActive && c.EndDate >= DateTime.Now)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Coupon> CreateCouponAsync(Coupon coupon)
        {
            coupon.CreatedAt = DateTime.Now;
            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();
            return coupon;
        }

        public async Task<bool> UpdateCouponAsync(Coupon coupon)
        {
            var existing = await _context.Coupons.FindAsync(coupon.Id);
            if (existing == null) return false;

            existing.Code = coupon.Code;
            existing.DiscountType = coupon.DiscountType;
            existing.DiscountValue = coupon.DiscountValue;
            existing.MinOrderAmount = coupon.MinOrderAmount;
            existing.MaxUsage = coupon.MaxUsage;
            existing.StartDate = coupon.StartDate;
            existing.EndDate = coupon.EndDate;
            existing.IsActive = coupon.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCouponAsync(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return false;

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

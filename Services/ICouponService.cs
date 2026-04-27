using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface ICouponService
    {
        Task<Coupon> GetCouponByCodeAsync(string code);
        Task<bool> ValidateCouponAsync(string code, decimal orderAmount);
        Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount);
        Task<bool> ApplyCouponAsync(string code);
        Task<List<Coupon>> GetActiveCouponsAsync();
        Task<Coupon> CreateCouponAsync(Coupon coupon);
        Task<bool> UpdateCouponAsync(Coupon coupon);
        Task<bool> DeleteCouponAsync(int id);
    }
}

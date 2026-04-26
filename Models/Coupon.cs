using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// "Percent" hoặc "Fixed"
        /// </summary>
        [Required]
        public string DiscountType { get; set; } = "Percent";

        /// <summary>
        /// Giá trị giảm (% hoặc số tiền cố định)
        /// </summary>
        public int DiscountValue { get; set; }

        /// <summary>
        /// Số tiền đơn hàng tối thiểu để áp dụng
        /// </summary>
        public int MinOrderAmount { get; set; }

        /// <summary>
        /// Số lần sử dụng tối đa (0 = không giới hạn)
        /// </summary>
        public int MaxUsage { get; set; }

        /// <summary>
        /// Số lần đã sử dụng
        /// </summary>
        public int UsedCount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Computed
        public bool IsValid => IsActive
            && DateTime.Now >= StartDate
            && DateTime.Now <= EndDate
            && (MaxUsage == 0 || UsedCount < MaxUsage);
    }
}

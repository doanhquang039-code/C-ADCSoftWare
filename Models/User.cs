using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int Age { get; set; }

        public string Gender { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        /// <summary>
        /// "Admin" | "Manager" | "Hiring" | "User"
        /// </summary>
        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ngôn ngữ ưa thích: "vi" | "en"
        /// </summary>
        public string PreferredLanguage { get; set; } = "vi";

        // Extended user information
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; } = "Vietnam";
        public string PostalCode { get; set; }
        
        /// <summary>
        /// Avatar URL
        /// </summary>
        public string ProfileImage { get; set; }
        
        /// <summary>
        /// Ngày sinh
        /// </summary>
        public DateTime? DateOfBirth { get; set; }
        
        /// <summary>
        /// CCCD/CMND
        /// </summary>
        public string IdentityNumber { get; set; }
        
        /// <summary>
        /// Passport
        /// </summary>
        public string PassportNumber { get; set; }
        
        /// <summary>
        /// Quốc tịch
        /// </summary>
        public string Nationality { get; set; } = "Vietnamese";
        
        /// <summary>
        /// Nghề nghiệp
        /// </summary>
        public string Occupation { get; set; }
        
        /// <summary>
        /// Công ty
        /// </summary>
        public string Company { get; set; }
        
        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Notes { get; set; }
        
        /// <summary>
        /// Điểm tích lũy
        /// </summary>
        public int LoyaltyPoints { get; set; } = 0;
        
        /// <summary>
        /// Hạng thành viên: "Bronze", "Silver", "Gold", "Platinum"
        /// </summary>
        public string MembershipTier { get; set; } = "Bronze";
        
        /// <summary>
        /// Email verified
        /// </summary>
        public bool EmailVerified { get; set; } = false;
        
        /// <summary>
        /// Phone verified
        /// </summary>
        public bool PhoneVerified { get; set; } = false;
        
        /// <summary>
        /// Last login
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
        
        /// <summary>
        /// Login count
        /// </summary>
        public int LoginCount { get; set; } = 0;
        
        // Social Auth Properties
        /// <summary>
        /// Google ID for social login
        /// </summary>
        public string? GoogleId { get; set; }
        
        /// <summary>
        /// Facebook ID for social login
        /// </summary>
        public string? FacebookId { get; set; }
        
        /// <summary>
        /// Apple ID for social login
        /// </summary>
        public string? AppleId { get; set; }
        
        /// <summary>
        /// Profile picture URL
        /// </summary>
        public string? ProfilePicture { get; set; }
        
        /// <summary>
        /// Full name (for display)
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// Email verified status
        /// </summary>
        public bool IsEmailVerified { get; set; } = false;

        // Navigation properties
        public ICollection<Orders> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Wishlist> Wishlists { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<BlogPost> BlogPosts { get; set; }
        
        // Helper methods
        public int GetAge()
        {
            if (!DateOfBirth.HasValue) return Age;
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Value.Year;
            if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
            return age;
        }
        
        public bool IsAdmin() => Role == "Admin";
        public bool IsManager() => Role == "Manager";
        public bool IsHiring() => Role == "Hiring";
        public bool IsStaff() => Role == "Manager" || Role == "Hiring";
        public bool IsStaffOrAdmin() => Role == "Admin" || Role == "Manager" || Role == "Hiring";
    }
}


using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
// ánh xạ cơ sở dữ liệu 
namespace WEBDULICH.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        
        // New tables
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<ReviewImage> ReviewImages { get; set; }
        public DbSet<SecurityLog> SecurityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tour>().ToTable("Tour");
            modelBuilder.Entity<User>().ToTable("User"); 
            modelBuilder.Entity<Hotel>().ToTable("Hotel");
            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<Destination>().ToTable("Destination");
            modelBuilder.Entity<Orders>().ToTable("Orders");
            modelBuilder.Entity<Review>().ToTable("Review");
            modelBuilder.Entity<Payment>().ToTable("Payment");
            modelBuilder.Entity<Coupon>().ToTable("Coupon");
            
            // New tables
            modelBuilder.Entity<Booking>().ToTable("Booking");
            modelBuilder.Entity<Wishlist>().ToTable("Wishlist");
            modelBuilder.Entity<Notification>().ToTable("Notification");
            modelBuilder.Entity<ChatMessage>().ToTable("ChatMessage");
            modelBuilder.Entity<BlogPost>().ToTable("BlogPost");
            modelBuilder.Entity<ReviewImage>().ToTable("ReviewImage");
            modelBuilder.Entity<SecurityLog>().ToTable("SecurityLog");

            // Default values for User
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasDefaultValue("User");

            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<User>()
                .Property(u => u.PreferredLanguage)
                .HasDefaultValue("vi");

            modelBuilder.Entity<User>()
                .Property(u => u.Country)
                .HasDefaultValue("Vietnam");

            modelBuilder.Entity<User>()
                .Property(u => u.Nationality)
                .HasDefaultValue("Vietnamese");

            modelBuilder.Entity<User>()
                .Property(u => u.MembershipTier)
                .HasDefaultValue("Bronze");

            modelBuilder.Entity<User>()
                .Property(u => u.LoyaltyPoints)
                .HasDefaultValue(0);

            modelBuilder.Entity<User>()
                .Property(u => u.LoginCount)
                .HasDefaultValue(0);

            // Indexes for User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Role);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.MembershipTier);

            // Indexes for performance
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.UserId, b.Status });

            modelBuilder.Entity<Wishlist>()
                .HasIndex(w => new { w.UserId, w.ItemType, w.ItemId })
                .IsUnique();

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });

            modelBuilder.Entity<ChatMessage>()
                .HasIndex(c => c.SessionId);

            modelBuilder.Entity<BlogPost>()
                .HasIndex(b => b.Slug)
                .IsUnique();

            // Security indexes
            modelBuilder.Entity<SecurityLog>()
                .HasIndex(s => new { s.Email, s.EventType });

            modelBuilder.Entity<SecurityLog>()
                .HasIndex(s => s.IpAddress);

            modelBuilder.Entity<SecurityLog>()
                .HasIndex(s => s.CreatedAt);

            base.OnModelCreating(modelBuilder);
        }
    }
}

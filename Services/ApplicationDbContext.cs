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

        // New features DbSets
        public DbSet<Report> Reports { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<EmailCampaign> EmailCampaigns { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<EmailSubscriber> EmailSubscribers { get; set; }
        public DbSet<LoyaltyAccount> LoyaltyAccounts { get; set; }
        public DbSet<PointTransaction> PointTransactions { get; set; }
        public DbSet<LoyaltyTier> LoyaltyTiers { get; set; }
        public DbSet<Reward> Rewards { get; set; }
        public DbSet<RewardRedemption> RewardRedemptions { get; set; }
        public DbSet<PointsRule> PointsRules { get; set; }
        
        // E-Ticket DbSet
        public DbSet<Models.Ticket> Tickets { get; set; }
        
        // Order Details DbSet
        public DbSet<OrderDetail> OrderDetails { get; set; }

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

            // New features tables
            modelBuilder.Entity<Report>().ToTable("Report");
            modelBuilder.Entity<Location>().ToTable("Location");
            modelBuilder.Entity<EmailCampaign>().ToTable("EmailCampaign");
            modelBuilder.Entity<EmailLog>().ToTable("EmailLog");
            modelBuilder.Entity<EmailTemplate>().ToTable("EmailTemplate");
            modelBuilder.Entity<EmailSubscriber>().ToTable("EmailSubscriber");
            modelBuilder.Entity<LoyaltyAccount>().ToTable("LoyaltyAccount");
            modelBuilder.Entity<PointTransaction>().ToTable("PointTransaction");
            modelBuilder.Entity<LoyaltyTier>().ToTable("LoyaltyTier");
            modelBuilder.Entity<Reward>().ToTable("Reward");
            modelBuilder.Entity<RewardRedemption>().ToTable("RewardRedemption");
            modelBuilder.Entity<PointsRule>().ToTable("PointsRule");
            
            // E-Ticket table
            modelBuilder.Entity<Models.Ticket>().ToTable("Ticket");
            
            // Order Details table
            modelBuilder.Entity<OrderDetail>().ToTable("OrderDetail");

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

            // New features indexes
            modelBuilder.Entity<Report>()
                .HasIndex(r => new { r.ReportType, r.CreatedAt });

            modelBuilder.Entity<Location>()
                .HasIndex(l => new { l.LocationType, l.IsVisible });

            modelBuilder.Entity<Location>()
                .HasIndex(l => new { l.Latitude, l.Longitude });

            modelBuilder.Entity<EmailCampaign>()
                .HasIndex(c => new { c.Status, c.ScheduledDate });

            modelBuilder.Entity<EmailLog>()
                .HasIndex(l => new { l.CampaignId, l.Status });

            modelBuilder.Entity<EmailLog>()
                .HasIndex(l => l.TrackingToken)
                .IsUnique();

            modelBuilder.Entity<EmailSubscriber>()
                .HasIndex(s => s.Email)
                .IsUnique();

            modelBuilder.Entity<LoyaltyAccount>()
                .HasIndex(l => l.UserId)
                .IsUnique();

            modelBuilder.Entity<PointTransaction>()
                .HasIndex(p => new { p.LoyaltyAccountId, p.CreatedAt });

            modelBuilder.Entity<RewardRedemption>()
                .HasIndex(r => new { r.LoyaltyAccountId, r.Status });

            modelBuilder.Entity<RewardRedemption>()
                .HasIndex(r => r.RedemptionCode)
                .IsUnique();
            
            // E-Ticket indexes
            modelBuilder.Entity<Models.Ticket>()
                .HasIndex(t => t.TicketCode)
                .IsUnique();
            
            modelBuilder.Entity<Models.Ticket>()
                .HasIndex(t => new { t.BookingId, t.Status });
            
            modelBuilder.Entity<Models.Ticket>()
                .HasIndex(t => t.ValidUntil);

            base.OnModelCreating(modelBuilder);
        }
    }
}

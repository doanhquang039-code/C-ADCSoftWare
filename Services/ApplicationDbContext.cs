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

            // Default values for User
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasDefaultValue("User");

            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

            base.OnModelCreating(modelBuilder);
        }
    }
}

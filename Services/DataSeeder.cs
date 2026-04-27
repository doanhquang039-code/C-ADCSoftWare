using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext _context;

        public DataSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Seed Categories
            if (!await _context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Du lịch biển" },
                    new Category { Name = "Du lịch núi" },
                    new Category { Name = "Du lịch thành phố" },
                    new Category { Name = "Du lịch văn hóa" },
                    new Category { Name = "Du lịch sinh thái" }
                };
                _context.Categories.AddRange(categories);
                await _context.SaveChangesAsync();
            }

            // Seed Destinations
            if (!await _context.Destinations.AnyAsync())
            {
                var destinations = new List<Destination>
                {
                    new Destination 
                    { 
                        Name = "Hạ Long", 
                        Description = "Vịnh Hạ Long - Di sản thiên nhiên thế giới", 
                        Image = "/images/halong.jpg", 
                        Location = "Quảng Ninh", 
                        CategoryId = 1 
                    },
                    new Destination 
                    { 
                        Name = "Sapa", 
                        Description = "Thị trấn miền núi với ruộng bậc thang tuyệt đẹp", 
                        Image = "/images/sapa.jpg", 
                        Location = "Lào Cai", 
                        CategoryId = 2 
                    },
                    new Destination 
                    { 
                        Name = "Hội An", 
                        Description = "Phố cổ Hội An - Di sản văn hóa thế giới", 
                        Image = "/images/hoian.jpg", 
                        Location = "Quảng Nam", 
                        CategoryId = 4 
                    },
                    new Destination 
                    { 
                        Name = "Phú Quốc", 
                        Description = "Đảo ngọc Phú Quốc với bãi biển tuyệt đẹp", 
                        Image = "/images/phuquoc.jpg", 
                        Location = "Kiên Giang", 
                        CategoryId = 1 
                    },
                    new Destination 
                    { 
                        Name = "Đà Lạt", 
                        Description = "Thành phố ngàn hoa với khí hậu mát mẻ", 
                        Image = "/images/dalat.jpg", 
                        Location = "Lâm Đồng", 
                        CategoryId = 2 
                    }
                };
                _context.Destinations.AddRange(destinations);
                await _context.SaveChangesAsync();
            }

            // Seed Tours
            if (!await _context.Tours.AnyAsync())
            {
                var tours = new List<Tour>
                {
                    new Tour 
                    { 
                        Name = "Tour Hạ Long 2N1Đ", 
                        Description = "Khám phá vịnh Hạ Long với du thuyền sang trọng", 
                        Price = 2500000, 
                        Duration = 2, 
                        Image = "/images/tour-halong.jpg", 
                        DestinationId = 1, 
                        Quantity = 50 
                    },
                    new Tour 
                    { 
                        Name = "Tour Sapa 3N2Đ", 
                        Description = "Trekking Sapa và khám phá văn hóa dân tộc", 
                        Price = 3200000, 
                        Duration = 3, 
                        Image = "/images/tour-sapa.jpg", 
                        DestinationId = 2, 
                        Quantity = 30 
                    },
                    new Tour 
                    { 
                        Name = "Tour Hội An - Đà Nẵng 4N3Đ", 
                        Description = "Khám phá phố cổ Hội An và thành phố Đà Nẵng", 
                        Price = 4500000, 
                        Duration = 4, 
                        Image = "/images/tour-hoian.jpg", 
                        DestinationId = 3, 
                        Quantity = 40 
                    },
                    new Tour 
                    { 
                        Name = "Tour Phú Quốc 5N4Đ", 
                        Description = "Nghỉ dưỡng tại đảo ngọc Phú Quốc", 
                        Price = 6800000, 
                        Duration = 5, 
                        Image = "/images/tour-phuquoc.jpg", 
                        DestinationId = 4, 
                        Quantity = 25 
                    },
                    new Tour 
                    { 
                        Name = "Tour Đà Lạt 3N2Đ", 
                        Description = "Khám phá thành phố ngàn hoa Đà Lạt", 
                        Price = 2800000, 
                        Duration = 3, 
                        Image = "/images/tour-dalat.jpg", 
                        DestinationId = 5, 
                        Quantity = 35 
                    }
                };
                _context.Tours.AddRange(tours);
                await _context.SaveChangesAsync();
            }

            // Seed Hotels
            if (!await _context.Hotels.AnyAsync())
            {
                var hotels = new List<Hotel>
                {
                    new Hotel 
                    { 
                        Name = "Halong Bay Hotel", 
                        Address = "Bãi Cháy, Hạ Long, Quảng Ninh", 
                        Price = 1200000, 
                        Image = "/images/hotel-halong.jpg", 
                        Rating = 4, 
                        TourId = 1, 
                        Quantity = 100 
                    },
                    new Hotel 
                    { 
                        Name = "Sapa Mountain Resort", 
                        Address = "Thị trấn Sapa, Lào Cai", 
                        Price = 1500000, 
                        Image = "/images/hotel-sapa.jpg", 
                        Rating = 5, 
                        TourId = 2, 
                        Quantity = 80 
                    },
                    new Hotel 
                    { 
                        Name = "Hoi An Ancient House", 
                        Address = "Phố cổ Hội An, Quảng Nam", 
                        Price = 1800000, 
                        Image = "/images/hotel-hoian.jpg", 
                        Rating = 4, 
                        TourId = 3, 
                        Quantity = 60 
                    },
                    new Hotel 
                    { 
                        Name = "Phu Quoc Beach Resort", 
                        Address = "Bãi Trường, Phú Quốc, Kiên Giang", 
                        Price = 2500000, 
                        Image = "/images/hotel-phuquoc.jpg", 
                        Rating = 5, 
                        TourId = 4, 
                        Quantity = 120 
                    },
                    new Hotel 
                    { 
                        Name = "Dalat Palace Hotel", 
                        Address = "Trung tâm Đà Lạt, Lâm Đồng", 
                        Price = 1600000, 
                        Image = "/images/hotel-dalat.jpg", 
                        Rating = 4, 
                        TourId = 5, 
                        Quantity = 90 
                    }
                };
                _context.Hotels.AddRange(hotels);
                await _context.SaveChangesAsync();
            }

            // Seed Users
            if (!await _context.Users.AnyAsync())
            {
                var users = new List<User>
                {
                    new User 
                    { 
                        Name = "Admin User", 
                        Age = 30, 
                        Gender = "Nam", 
                        Email = "admin@webdulich.local", 
                        Password = "admin123", 
                        Role = "Admin",
                        PhoneNumber = "0901234567",
                        Address = "123 Nguyễn Huệ, Q1, TP.HCM",
                        City = "TP.HCM",
                        Country = "Vietnam",
                        CreatedAt = DateTime.Now,
                        IsActive = true,
                        LoyaltyPoints = 1000,
                        MembershipTier = "Gold"
                    },
                    new User 
                    { 
                        Name = "Nguyễn Văn A", 
                        Age = 25, 
                        Gender = "Nam", 
                        Email = "nguyenvana@gmail.com", 
                        Password = "123456", 
                        Role = "User",
                        PhoneNumber = "0987654321",
                        Address = "456 Lê Lợi, Q1, TP.HCM",
                        City = "TP.HCM",
                        Country = "Vietnam",
                        CreatedAt = DateTime.Now,
                        IsActive = true,
                        LoyaltyPoints = 500,
                        MembershipTier = "Silver"
                    },
                    new User 
                    { 
                        Name = "Trần Thị B", 
                        Age = 28, 
                        Gender = "Nữ", 
                        Email = "tranthib@gmail.com", 
                        Password = "123456", 
                        Role = "User",
                        PhoneNumber = "0912345678",
                        Address = "789 Trần Hưng Đạo, Q5, TP.HCM",
                        City = "TP.HCM",
                        Country = "Vietnam",
                        CreatedAt = DateTime.Now,
                        IsActive = true,
                        LoyaltyPoints = 250,
                        MembershipTier = "Bronze"
                    }
                };
                _context.Users.AddRange(users);
                await _context.SaveChangesAsync();
            }

            // Seed Orders
            if (!await _context.Orders.AnyAsync())
            {
                var orders = new List<Orders>
                {
                    new Orders 
                    { 
                        Status = "Paid", 
                        Quantity = 2, 
                        TotalPrice = 5000000, 
                        OrderDate = DateTime.Now.AddDays(-10), 
                        PaymentMethod = "Credit Card", 
                        UserId = 2, 
                        TourId = 1,
                        ConfirmedEmail = "nguyenvana@gmail.com",
                        DepartureDate = DateTime.Now.AddDays(15)
                    },
                    new Orders 
                    { 
                        Status = "Pending", 
                        Quantity = 1, 
                        TotalPrice = 3200000, 
                        OrderDate = DateTime.Now.AddDays(-5), 
                        PaymentMethod = "Bank Transfer", 
                        UserId = 3, 
                        TourId = 2,
                        ConfirmedEmail = "tranthib@gmail.com",
                        DepartureDate = DateTime.Now.AddDays(20)
                    },
                    new Orders 
                    { 
                        Status = "Paid", 
                        Quantity = 3, 
                        TotalPrice = 13500000, 
                        OrderDate = DateTime.Now.AddDays(-3), 
                        PaymentMethod = "Credit Card", 
                        UserId = 2, 
                        TourId = 3,
                        ConfirmedEmail = "nguyenvana@gmail.com",
                        DepartureDate = DateTime.Now.AddDays(25)
                    }
                };
                _context.Orders.AddRange(orders);
                await _context.SaveChangesAsync();
            }

            // Seed Reviews
            if (!await _context.Reviews.AnyAsync())
            {
                var reviews = new List<Review>
                {
                    new Review 
                    { 
                        Rating = "5", 
                        Comment = "Tour rất tuyệt vời, hướng dẫn viên nhiệt tình!", 
                        ReviewDate = DateTime.Now.AddDays(-5), 
                        UserId = 2, 
                        TourId = 1 
                    },
                    new Review 
                    { 
                        Rating = "4", 
                        Comment = "Cảnh đẹp, dịch vụ tốt nhưng thời tiết không thuận lợi", 
                        ReviewDate = DateTime.Now.AddDays(-3), 
                        UserId = 3, 
                        TourId = 2 
                    },
                    new Review 
                    { 
                        Rating = "5", 
                        Comment = "Phố cổ Hội An thật sự rất đẹp và lãng mạn", 
                        ReviewDate = DateTime.Now.AddDays(-1), 
                        UserId = 2, 
                        TourId = 3 
                    }
                };
                _context.Reviews.AddRange(reviews);
                await _context.SaveChangesAsync();
            }

            // Seed Coupons
            if (!await _context.Coupons.AnyAsync())
            {
                var coupons = new List<Coupon>
                {
                    new Coupon 
                    { 
                        Code = "SUMMER2024", 
                        DiscountType = "Percentage", 
                        DiscountValue = 15, 
                        MinOrderAmount = 2000000, 
                        MaxUsage = 100, 
                        UsedCount = 25, 
                        StartDate = DateTime.Now.AddDays(-30), 
                        EndDate = DateTime.Now.AddDays(30), 
                        IsActive = true, 
                        CreatedAt = DateTime.Now.AddDays(-30) 
                    },
                    new Coupon 
                    { 
                        Code = "NEWUSER", 
                        DiscountType = "Fixed", 
                        DiscountValue = 500000, 
                        MinOrderAmount = 3000000, 
                        MaxUsage = 50, 
                        UsedCount = 12, 
                        StartDate = DateTime.Now.AddDays(-15), 
                        EndDate = DateTime.Now.AddDays(45), 
                        IsActive = true, 
                        CreatedAt = DateTime.Now.AddDays(-15) 
                    }
                };
                _context.Coupons.AddRange(coupons);
                await _context.SaveChangesAsync();
            }

            // Seed Loyalty Tiers
            if (!await _context.LoyaltyTiers.AnyAsync())
            {
                var loyaltyTiers = new List<LoyaltyTier>
                {
                    new LoyaltyTier 
                    { 
                        Name = "Bronze", 
                        Level = 1, 
                        MinPoints = 0, 
                        PointsMultiplier = 1.0m, 
                        DiscountPercentage = 0m, 
                        Benefits = "Tích điểm cơ bản", 
                        Color = "#CD7F32", 
                        Icon = "bronze-medal", 
                        IsActive = true, 
                        CreatedAt = DateTime.Now 
                    },
                    new LoyaltyTier 
                    { 
                        Name = "Silver", 
                        Level = 2, 
                        MinPoints = 500, 
                        PointsMultiplier = 1.2m, 
                        DiscountPercentage = 5m, 
                        Benefits = "Giảm giá 5%, tích điểm x1.2", 
                        Color = "#C0C0C0", 
                        Icon = "silver-medal", 
                        IsActive = true, 
                        CreatedAt = DateTime.Now 
                    },
                    new LoyaltyTier 
                    { 
                        Name = "Gold", 
                        Level = 3, 
                        MinPoints = 1000, 
                        PointsMultiplier = 1.5m, 
                        DiscountPercentage = 10m, 
                        Benefits = "Giảm giá 10%, tích điểm x1.5, ưu tiên hỗ trợ", 
                        Color = "#FFD700", 
                        Icon = "gold-medal", 
                        IsActive = true, 
                        CreatedAt = DateTime.Now 
                    },
                    new LoyaltyTier 
                    { 
                        Name = "Platinum", 
                        Level = 4, 
                        MinPoints = 2000, 
                        PointsMultiplier = 2.0m, 
                        DiscountPercentage = 15m, 
                        Benefits = "Giảm giá 15%, tích điểm x2, ưu tiên cao nhất", 
                        Color = "#E5E4E2", 
                        Icon = "platinum-medal", 
                        IsActive = true, 
                        CreatedAt = DateTime.Now 
                    }
                };
                _context.LoyaltyTiers.AddRange(loyaltyTiers);
                await _context.SaveChangesAsync();
            }

            // Seed Rewards
            if (!await _context.Rewards.AnyAsync())
            {
                var rewards = new List<Reward>
                {
                    new Reward 
                    { 
                        Name = "Voucher giảm giá 200K", 
                        Description = "Voucher giảm giá 200,000 VND cho đơn hàng từ 2 triệu", 
                        PointsCost = 200, 
                        RewardType = "Discount", 
                        Value = 200000m, 
                        RewardData = "{\"discountAmount\": 200000, \"minOrder\": 2000000}", 
                        Image = "/images/voucher-200k.jpg", 
                        Quantity = 100, 
                        RedeemedCount = 15, 
                        MinTierLevel = 1, 
                        ExpiryDate = DateTime.Now.AddMonths(6), 
                        IsActive = true, 
                        CreatedAt = DateTime.Now 
                    },
                    new Reward 
                    { 
                        Name = "Tour miễn phí Hạ Long", 
                        Description = "Tour Hạ Long 2N1Đ hoàn toàn miễn phí", 
                        PointsCost = 1000, 
                        RewardType = "FreeTour", 
                        Value = 2500000m, 
                        RewardData = "{\"tourId\": 1}", 
                        Image = "/images/free-tour-halong.jpg", 
                        Quantity = 10, 
                        RedeemedCount = 2, 
                        MinTierLevel = 3, 
                        ExpiryDate = DateTime.Now.AddMonths(12), 
                        IsActive = true, 
                        CreatedAt = DateTime.Now 
                    }
                };
                _context.Rewards.AddRange(rewards);
                await _context.SaveChangesAsync();
            }

            // Seed Locations for Map
            if (!await _context.Locations.AnyAsync())
            {
                var locations = new List<Location>
                {
                    new Location 
                    { 
                        Name = "Vịnh Hạ Long", 
                        Address = "Hạ Long, Quảng Ninh", 
                        Latitude = 20.9101, 
                        Longitude = 107.1839, 
                        LocationType = "Destination", 
                        RelatedId = 1, 
                        Description = "Di sản thiên nhiên thế giới", 
                        Image = "/images/halong-map.jpg", 
                        ContactInfo = "Tel: 0203-384-6888", 
                        OpeningHours = "24/7", 
                        Website = "https://halongbay.gov.vn", 
                        Rating = 4.8, 
                        CreatedAt = DateTime.Now, 
                        IsVisible = true 
                    },
                    new Location 
                    { 
                        Name = "Thị trấn Sapa", 
                        Address = "Sapa, Lào Cai", 
                        Latitude = 22.3364, 
                        Longitude = 103.8438, 
                        LocationType = "Destination", 
                        RelatedId = 2, 
                        Description = "Thị trấn miền núi nổi tiếng", 
                        Image = "/images/sapa-map.jpg", 
                        ContactInfo = "Tel: 0214-387-1975", 
                        OpeningHours = "24/7", 
                        Website = "https://sapa.gov.vn", 
                        Rating = 4.7, 
                        CreatedAt = DateTime.Now, 
                        IsVisible = true 
                    }
                };
                _context.Locations.AddRange(locations);
                await _context.SaveChangesAsync();
            }

            // Seed Email Templates
            if (!await _context.EmailTemplates.AnyAsync())
            {
                var emailTemplates = new List<EmailTemplate>
                {
                    new EmailTemplate 
                    { 
                        Name = "Welcome Email", 
                        Description = "Email chào mừng thành viên mới", 
                        TemplateType = "Welcome", 
                        Subject = "Chào mừng bạn đến với WEBDULICH!", 
                        HtmlContent = "<h1>Chào mừng {{Name}}!</h1><p>Cảm ơn bạn đã đăng ký tài khoản tại WEBDULICH.</p>", 
                        TextContent = "Chào mừng {{Name}}! Cảm ơn bạn đã đăng ký tài khoản tại WEBDULICH.", 
                        Variables = "Name,Email", 
                        IsActive = true, 
                        CreatedAt = DateTime.Now 
                    },
                    new EmailTemplate 
                    { 
                        Name = "Booking Confirmation", 
                        Description = "Email xác nhận đặt tour", 
                        TemplateType = "BookingConfirmation", 
                        Subject = "Xác nhận đặt tour - {{TourName}}", 
                        HtmlContent = "<h1>Xác nhận đặt tour</h1><p>Chào {{Name}},</p><p>Tour: {{TourName}}</p><p>Ngày khởi hành: {{DepartureDate}}</p>", 
                        TextContent = "Xác nhận đặt tour - {{TourName}} cho {{Name}} vào ngày {{DepartureDate}}", 
                        Variables = "Name,TourName,DepartureDate,TotalPrice", 
                        IsActive = true, 
                        CreatedAt = DateTime.Now 
                    }
                };
                _context.EmailTemplates.AddRange(emailTemplates);
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("Seed data completed successfully!");
        }
    }
}
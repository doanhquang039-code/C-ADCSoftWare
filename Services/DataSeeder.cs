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
                var categories = new[]
                {
                    new Category { Name = "Biển đảo" },
                    new Category { Name = "Núi rừng" },
                    new Category { Name = "Thành phố" },
                    new Category { Name = "Văn hóa lịch sử" },
                    new Category { Name = "Nghỉ dưỡng" },
                    new Category { Name = "Phiêu lưu" },
                    new Category { Name = "Ẩm thực" },
                    new Category { Name = "Tâm linh" }
                };
                _context.Categories.AddRange(categories);
                await _context.SaveChangesAsync();
            }

            // Seed Destinations
            if (!await _context.Destinations.AnyAsync())
            {
                var categories = await _context.Categories.ToListAsync();
                var destinations = new[]
                {
                    new Destination { Name = "Hạ Long", Description = "Vịnh Hạ Long - Di sản thế giới", Image = "/images/destinations/halong.jpg", Location = "Quảng Ninh", CategoryId = categories.First(c => c.Name == "Biển đảo").Id },
                    new Destination { Name = "Sapa", Description = "Thị trấn trong mây", Image = "/images/destinations/sapa.jpg", Location = "Lào Cai", CategoryId = categories.First(c => c.Name == "Núi rừng").Id },
                    new Destination { Name = "Hội An", Description = "Phố cổ Hội An", Image = "/images/destinations/hoian.jpg", Location = "Quảng Nam", CategoryId = categories.First(c => c.Name == "Văn hóa lịch sử").Id },
                    new Destination { Name = "Phú Quốc", Description = "Đảo ngọc Phú Quốc", Image = "/images/destinations/phuquoc.jpg", Location = "Kiên Giang", CategoryId = categories.First(c => c.Name == "Biển đảo").Id },
                    new Destination { Name = "Đà Lạt", Description = "Thành phố ngàn hoa", Image = "/images/destinations/dalat.jpg", Location = "Lâm Đồng", CategoryId = categories.First(c => c.Name == "Nghỉ dưỡng").Id },
                    new Destination { Name = "Nha Trang", Description = "Vịnh biển đẹp nhất", Image = "/images/destinations/nhatrang.jpg", Location = "Khánh Hòa", CategoryId = categories.First(c => c.Name == "Biển đảo").Id },
                    new Destination { Name = "Huế", Description = "Cố đô Huế", Image = "/images/destinations/hue.jpg", Location = "Thừa Thiên Huế", CategoryId = categories.First(c => c.Name == "Văn hóa lịch sử").Id },
                    new Destination { Name = "Hồ Chí Minh", Description = "Thành phố năng động", Image = "/images/destinations/hcm.jpg", Location = "TP.HCM", CategoryId = categories.First(c => c.Name == "Thành phố").Id }
                };
                _context.Destinations.AddRange(destinations);
                await _context.SaveChangesAsync();
            }

            // Seed Users
            if (!await _context.Users.AnyAsync())
            {
                var users = new[]
                {
                    new User 
                    { 
                        Name = "Admin System", 
                        Email = "admin@webdulich.com", 
                        Password = "admin123", 
                        Role = "Admin", 
                        Age = 30, 
                        Gender = "Nam",
                        PhoneNumber = "0901234567",
                        Address = "123 Nguyễn Huệ",
                        City = "TP.HCM",
                        Country = "Vietnam",
                        Nationality = "Vietnamese",
                        MembershipTier = "Platinum",
                        LoyaltyPoints = 15000,
                        EmailVerified = true,
                        PhoneVerified = true,
                        IsActive = true,
                        DateOfBirth = new DateTime(1994, 1, 15),
                        IdentityNumber = "079094001234",
                        Occupation = "System Administrator",
                        Company = "WebDuLich Corp"
                    },
                    new User 
                    { 
                        Name = "Nguyễn Văn Manager", 
                        Email = "manager@webdulich.com", 
                        Password = "manager123", 
                        Role = "Manager", 
                        Age = 28, 
                        Gender = "Nam",
                        PhoneNumber = "0902345678",
                        Address = "456 Lê Lợi",
                        City = "Hà Nội",
                        Country = "Vietnam",
                        Nationality = "Vietnamese",
                        MembershipTier = "Gold",
                        LoyaltyPoints = 8000,
                        EmailVerified = true,
                        IsActive = true,
                        DateOfBirth = new DateTime(1996, 3, 20),
                        IdentityNumber = "001096002345",
                        Occupation = "Tour Manager",
                        Company = "WebDuLich Corp"
                    },
                    new User 
                    { 
                        Name = "Trần Thị Hiring", 
                        Email = "hiring@webdulich.com", 
                        Password = "hiring123", 
                        Role = "Hiring", 
                        Age = 26, 
                        Gender = "Nữ",
                        PhoneNumber = "0903456789",
                        Address = "789 Trần Hưng Đạo",
                        City = "Đà Nẵng",
                        Country = "Vietnam",
                        Nationality = "Vietnamese",
                        MembershipTier = "Silver",
                        LoyaltyPoints = 3000,
                        EmailVerified = true,
                        IsActive = true,
                        DateOfBirth = new DateTime(1998, 7, 10),
                        IdentityNumber = "043098003456",
                        Occupation = "HR Specialist",
                        Company = "WebDuLich Corp"
                    },
                    new User 
                    { 
                        Name = "Lê Văn User", 
                        Email = "user@webdulich.com", 
                        Password = "user123", 
                        Role = "User", 
                        Age = 25, 
                        Gender = "Nam",
                        PhoneNumber = "0904567890",
                        Address = "321 Hai Bà Trưng",
                        City = "TP.HCM",
                        Country = "Vietnam",
                        Nationality = "Vietnamese",
                        MembershipTier = "Bronze",
                        LoyaltyPoints = 500,
                        IsActive = true,
                        DateOfBirth = new DateTime(1999, 12, 5),
                        IdentityNumber = "079099004567",
                        Occupation = "Software Developer"
                    },
                    new User 
                    { 
                        Name = "Phạm Thị Lan", 
                        Email = "lan.pham@gmail.com", 
                        Password = "user123", 
                        Role = "User", 
                        Age = 32, 
                        Gender = "Nữ",
                        PhoneNumber = "0905678901",
                        Address = "654 Võ Văn Tần",
                        City = "TP.HCM",
                        Country = "Vietnam",
                        Nationality = "Vietnamese",
                        MembershipTier = "Gold",
                        LoyaltyPoints = 7500,
                        IsActive = true,
                        DateOfBirth = new DateTime(1992, 5, 18),
                        IdentityNumber = "079092005678",
                        Occupation = "Marketing Manager",
                        Company = "ABC Company"
                    },
                    new User 
                    { 
                        Name = "Hoàng Minh Tuấn", 
                        Email = "tuan.hoang@yahoo.com", 
                        Password = "user123", 
                        Role = "User", 
                        Age = 29, 
                        Gender = "Nam",
                        PhoneNumber = "0906789012",
                        Address = "987 Cách Mạng Tháng 8",
                        City = "Hà Nội",
                        Country = "Vietnam",
                        Nationality = "Vietnamese",
                        MembershipTier = "Silver",
                        LoyaltyPoints = 4200,
                        IsActive = true,
                        DateOfBirth = new DateTime(1995, 9, 25),
                        IdentityNumber = "001095006789",
                        Occupation = "Teacher"
                    },
                    new User 
                    { 
                        Name = "Ngô Thị Mai", 
                        Email = "mai.ngo@hotmail.com", 
                        Password = "user123", 
                        Role = "User", 
                        Age = 27, 
                        Gender = "Nữ",
                        PhoneNumber = "0907890123",
                        Address = "147 Nguyễn Thái Học",
                        City = "Đà Nẵng",
                        Country = "Vietnam",
                        Nationality = "Vietnamese",
                        MembershipTier = "Bronze",
                        LoyaltyPoints = 1800,
                        IsActive = true,
                        DateOfBirth = new DateTime(1997, 11, 8),
                        IdentityNumber = "043097007890",
                        Occupation = "Nurse"
                    }
                };
                _context.Users.AddRange(users);
                await _context.SaveChangesAsync();
            }

            // Seed Tours
            if (!await _context.Tours.AnyAsync())
            {
                var destinations = await _context.Destinations.ToListAsync();
                var tours = new[]
                {
                    new Tour { Name = "Tour Hạ Long 2N1Đ", Description = "Khám phá vịnh Hạ Long với du thuyền sang trọng", Price = 2500000, Duration = 2, Image = "/images/tours/halong-tour.jpg", DestinationId = destinations.First(d => d.Name == "Hạ Long").Id, Quantity = 50 },
                    new Tour { Name = "Sapa Trekking 3N2Đ", Description = "Chinh phục đỉnh Fansipan và khám phá văn hóa dân tộc", Price = 3200000, Duration = 3, Image = "/images/tours/sapa-tour.jpg", DestinationId = destinations.First(d => d.Name == "Sapa").Id, Quantity = 30 },
                    new Tour { Name = "Hội An - Huế 4N3Đ", Description = "Khám phá di sản văn hóa miền Trung", Price = 4500000, Duration = 4, Image = "/images/tours/hoian-hue-tour.jpg", DestinationId = destinations.First(d => d.Name == "Hội An").Id, Quantity = 40 },
                    new Tour { Name = "Phú Quốc Resort 5N4Đ", Description = "Nghỉ dưỡng tại đảo ngọc Phú Quốc", Price = 6800000, Duration = 5, Image = "/images/tours/phuquoc-tour.jpg", DestinationId = destinations.First(d => d.Name == "Phú Quốc").Id, Quantity = 25 },
                    new Tour { Name = "Đà Lạt Romantic 3N2Đ", Description = "Thành phố tình yêu với khí hậu mát mẻ", Price = 2800000, Duration = 3, Image = "/images/tours/dalat-tour.jpg", DestinationId = destinations.First(d => d.Name == "Đà Lạt").Id, Quantity = 35 },
                    new Tour { Name = "Nha Trang Beach 4N3Đ", Description = "Tắm biển và thể thao dưới nước", Price = 3800000, Duration = 4, Image = "/images/tours/nhatrang-tour.jpg", DestinationId = destinations.First(d => d.Name == "Nha Trang").Id, Quantity = 45 }
                };
                _context.Tours.AddRange(tours);
                await _context.SaveChangesAsync();
            }

            // Seed Hotels
            if (!await _context.Hotels.AnyAsync())
            {
                var tours = await _context.Tours.ToListAsync();
                var hotels = new[]
                {
                    new Hotel { Name = "Hạ Long Bay Hotel", Address = "Bãi Cháy, Hạ Long", Price = 1200000, Image = "/images/hotels/halong-hotel.jpg", Rating = 4, TourId = tours.First(t => t.Name.Contains("Hạ Long")).Id, Quantity = 100 },
                    new Hotel { Name = "Sapa Mountain Resort", Address = "Thị trấn Sapa", Price = 1500000, Image = "/images/hotels/sapa-hotel.jpg", Rating = 5, TourId = tours.First(t => t.Name.Contains("Sapa")).Id, Quantity = 80 },
                    new Hotel { Name = "Hội An Ancient House", Address = "Phố cổ Hội An", Price = 1800000, Image = "/images/hotels/hoian-hotel.jpg", Rating = 4, TourId = tours.First(t => t.Name.Contains("Hội An")).Id, Quantity = 60 },
                    new Hotel { Name = "Phú Quốc Beach Resort", Address = "Bãi Trường, Phú Quốc", Price = 2500000, Image = "/images/hotels/phuquoc-hotel.jpg", Rating = 5, TourId = tours.First(t => t.Name.Contains("Phú Quốc")).Id, Quantity = 120 },
                    new Hotel { Name = "Đà Lạt Palace Hotel", Address = "Trung tâm Đà Lạt", Price = 1600000, Image = "/images/hotels/dalat-hotel.jpg", Rating = 4, TourId = tours.First(t => t.Name.Contains("Đà Lạt")).Id, Quantity = 90 },
                    new Hotel { Name = "Nha Trang Seaside Hotel", Address = "Bãi biển Nha Trang", Price = 1400000, Image = "/images/hotels/nhatrang-hotel.jpg", Rating = 4, TourId = tours.First(t => t.Name.Contains("Nha Trang")).Id, Quantity = 110 }
                };
                _context.Hotels.AddRange(hotels);
                await _context.SaveChangesAsync();
            }

            // Seed Coupons
            if (!await _context.Coupons.AnyAsync())
            {
                var coupons = new[]
                {
                    new Coupon { Code = "WELCOME2024", DiscountType = "Percent", DiscountValue = 10, MinOrderAmount = 1000000, MaxUsage = 100, StartDate = DateTime.Now.AddDays(-30), EndDate = DateTime.Now.AddDays(30), IsActive = true },
                    new Coupon { Code = "SUMMER50", DiscountType = "Fixed", DiscountValue = 500000, MinOrderAmount = 3000000, MaxUsage = 50, StartDate = DateTime.Now.AddDays(-15), EndDate = DateTime.Now.AddDays(45), IsActive = true },
                    new Coupon { Code = "NEWUSER", DiscountType = "Percent", DiscountValue = 15, MinOrderAmount = 2000000, MaxUsage = 200, StartDate = DateTime.Now.AddDays(-10), EndDate = DateTime.Now.AddDays(60), IsActive = true },
                    new Coupon { Code = "LOYALTY20", DiscountType = "Percent", DiscountValue = 20, MinOrderAmount = 5000000, MaxUsage = 30, StartDate = DateTime.Now.AddDays(-5), EndDate = DateTime.Now.AddDays(90), IsActive = true }
                };
                _context.Coupons.AddRange(coupons);
                await _context.SaveChangesAsync();
            }

            // Seed Reviews
            if (!await _context.Reviews.AnyAsync())
            {
                var users = await _context.Users.Where(u => u.Role == "User").ToListAsync();
                var tours = await _context.Tours.ToListAsync();
                
                if (users.Any() && tours.Any())
                {
                    var reviews = new[]
                    {
                        new Review { Rating = "5", Comment = "Tour rất tuyệt vời! Hướng dẫn viên nhiệt tình, cảnh đẹp", ReviewDate = DateTime.Now.AddDays(-10), UserId = users[0].Id, TourId = tours[0].Id },
                        new Review { Rating = "4", Comment = "Khách sạn sạch sẽ, view đẹp. Sẽ quay lại lần sau", ReviewDate = DateTime.Now.AddDays(-8), UserId = users[0].Id, TourId = tours[1].Id },
                        new Review { Rating = "5", Comment = "Phong cảnh Sapa thật sự choáng ngợp. Đáng đồng tiền", ReviewDate = DateTime.Now.AddDays(-5), UserId = users[0].Id, TourId = tours[1].Id },
                        new Review { Rating = "4", Comment = "Dịch vụ tốt, ăn uống ngon. Chỉ có điều thời tiết hơi nóng", ReviewDate = DateTime.Now.AddDays(-3), UserId = users[0].Id, TourId = tours[2].Id }
                    };
                    _context.Reviews.AddRange(reviews);
                    await _context.SaveChangesAsync();
                }
            }

            // Seed Blog Posts
            if (!await _context.BlogPosts.AnyAsync())
            {
                var authors = await _context.Users.Where(u => u.Role != "User").ToListAsync();
                var categories = await _context.Categories.ToListAsync();
                var destinations = await _context.Destinations.ToListAsync();

                if (authors.Any())
                {
                    var blogPosts = new[]
                    {
                        new BlogPost 
                        { 
                            Title = "10 Điểm đến không thể bỏ qua khi du lịch Việt Nam", 
                            Content = "Việt Nam là một đất nước với nhiều cảnh đẹp tuyệt vời từ Bắc vào Nam. Từ vịnh Hạ Long hùng vĩ đến đồng bằng sông Cửu Long phù sa, mỗi vùng miền đều có những nét đẹp riêng biệt...", 
                            Summary = "Khám phá 10 điểm đến hấp dẫn nhất Việt Nam",
                            Slug = "10-diem-den-khong-the-bo-qua-khi-du-lich-viet-nam",
                            FeaturedImage = "/images/blog/top-10-destinations.jpg",
                            AuthorId = authors[0].Id,
                            CategoryId = categories[0].Id,
                            DestinationId = destinations[0].Id,
                            Tags = "du lịch,việt nam,điểm đến",
                            IsPublished = true,
                            PublishedAt = DateTime.Now.AddDays(-7),
                            ViewCount = 1250
                        },
                        new BlogPost 
                        { 
                            Title = "Kinh nghiệm du lịch Sapa tự túc", 
                            Content = "Sapa là một trong những điểm đến yêu thích của du khách trong và ngoài nước. Với khí hậu mát mẻ quanh năm, cảnh quan núi non hùng vĩ và văn hóa dân tộc đặc sắc...", 
                            Summary = "Hướng dẫn chi tiết du lịch Sapa tự túc tiết kiệm",
                            Slug = "kinh-nghiem-du-lich-sapa-tu-tuc",
                            FeaturedImage = "/images/blog/sapa-guide.jpg",
                            AuthorId = authors[1].Id,
                            CategoryId = categories[1].Id,
                            DestinationId = destinations[1].Id,
                            Tags = "sapa,tự túc,kinh nghiệm",
                            IsPublished = true,
                            PublishedAt = DateTime.Now.AddDays(-5),
                            ViewCount = 890
                        },
                        new BlogPost 
                        { 
                            Title = "Ẩm thực đường phố Hội An không thể bỏ qua", 
                            Content = "Hội An không chỉ nổi tiếng với kiến trúc cổ kính mà còn là thiên đường ẩm thực với những món ăn đặc trưng...", 
                            Summary = "Khám phá những món ăn đặc sản tại phố cổ Hội An",
                            Slug = "am-thuc-duong-pho-hoi-an-khong-the-bo-qua",
                            FeaturedImage = "/images/blog/hoian-food.jpg",
                            AuthorId = authors[0].Id,
                            CategoryId = categories.First(c => c.Name == "Ẩm thực").Id,
                            DestinationId = destinations.First(d => d.Name == "Hội An").Id,
                            Tags = "hội an,ẩm thực,món ngon",
                            IsPublished = true,
                            PublishedAt = DateTime.Now.AddDays(-3),
                            ViewCount = 650
                        }
                    };
                    _context.BlogPosts.AddRange(blogPosts);
                    await _context.SaveChangesAsync();
                }
            }

            // Seed Bookings
            if (!await _context.Bookings.AnyAsync())
            {
                var users = await _context.Users.Where(u => u.Role == "User").ToListAsync();
                var tours = await _context.Tours.ToListAsync();
                var hotels = await _context.Hotels.ToListAsync();

                if (users.Any() && tours.Any())
                {
                    var bookings = new[]
                    {
                        new Booking 
                        { 
                            UserId = users[0].Id, 
                            BookingType = "Tour", 
                            TourId = tours[0].Id, 
                            StartDate = DateTime.Now.AddDays(15), 
                            EndDate = DateTime.Now.AddDays(17),
                            Adults = 2, 
                            Children = 0,
                            TotalPrice = tours[0].Price * 2, 
                            Status = "Confirmed",
                            SpecialRequests = "Yêu cầu phòng view biển"
                        },
                        new Booking 
                        { 
                            UserId = users[1].Id, 
                            BookingType = "Hotel", 
                            HotelId = hotels[0].Id, 
                            StartDate = DateTime.Now.AddDays(20), 
                            EndDate = DateTime.Now.AddDays(23),
                            Adults = 1, 
                            Children = 0,
                            Rooms = 1,
                            TotalPrice = hotels[0].Price * 3, 
                            Status = "Pending",
                            SpecialRequests = "Check-in muộn"
                        },
                        new Booking 
                        { 
                            UserId = users[2].Id, 
                            BookingType = "Tour", 
                            TourId = tours[1].Id, 
                            StartDate = DateTime.Now.AddDays(10), 
                            EndDate = DateTime.Now.AddDays(13),
                            Adults = 4, 
                            Children = 0,
                            TotalPrice = tours[1].Price * 4, 
                            Status = "Confirmed",
                            SpecialRequests = ""
                        }
                    };
                    _context.Bookings.AddRange(bookings);
                    await _context.SaveChangesAsync();
                }
            }

            // Seed Wishlists
            if (!await _context.Wishlists.AnyAsync())
            {
                var users = await _context.Users.Where(u => u.Role == "User").ToListAsync();
                var tours = await _context.Tours.ToListAsync();
                var hotels = await _context.Hotels.ToListAsync();

                if (users.Any() && tours.Any())
                {
                    var wishlists = new[]
                    {
                        new Wishlist { UserId = users[0].Id, ItemType = "Tour", ItemId = tours[1].Id },
                        new Wishlist { UserId = users[0].Id, ItemType = "Tour", ItemId = tours[2].Id },
                        new Wishlist { UserId = users[1].Id, ItemType = "Hotel", ItemId = hotels[1].Id },
                        new Wishlist { UserId = users[2].Id, ItemType = "Tour", ItemId = tours[0].Id }
                    };
                    _context.Wishlists.AddRange(wishlists);
                    await _context.SaveChangesAsync();
                }
            }

            // Seed Notifications
            if (!await _context.Notifications.AnyAsync())
            {
                var users = await _context.Users.Where(u => u.Role == "User").ToListAsync();

                if (users.Any())
                {
                    var notifications = new[]
                    {
                        new Notification 
                        { 
                            UserId = users[0].Id, 
                            Title = "Xác nhận đặt tour", 
                            Message = "Tour Hạ Long 2N1Đ của bạn đã được xác nhận. Mã booking: BK001", 
                            Type = "Booking",
                            IsRead = false
                        },
                        new Notification 
                        { 
                            UserId = users[0].Id, 
                            Title = "Khuyến mãi đặc biệt", 
                            Message = "Giảm 20% cho tour Sapa - Áp dụng đến hết tháng này!", 
                            Type = "Promotion",
                            IsRead = true
                        },
                        new Notification 
                        { 
                            UserId = users[1].Id, 
                            Title = "Thanh toán thành công", 
                            Message = "Thanh toán cho đơn hàng #12345 đã được xử lý thành công", 
                            Type = "Payment",
                            IsRead = false
                        }
                    };
                    _context.Notifications.AddRange(notifications);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
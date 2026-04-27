using System.Globalization;

namespace WEBDULICH.Services
{
    public class LocalizationService : ILocalizationService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _resources;
        private string _currentCulture = "vi";

        public LocalizationService()
        {
            _resources = new Dictionary<string, Dictionary<string, string>>();
            LoadResources();
        }

        private void LoadResources()
        {
            // Vietnamese resources
            _resources["vi"] = new Dictionary<string, string>
            {
                // Common
                {"Home", "Trang chủ"},
                {"About", "Giới thiệu"},
                {"Contact", "Liên hệ"},
                {"Login", "Đăng nhập"},
                {"Register", "Đăng ký"},
                {"Logout", "Đăng xuất"},
                {"Search", "Tìm kiếm"},
                {"Book", "Đặt tour"},
                {"BookNow", "Đặt ngay"},
                {"ViewDetails", "Xem chi tiết"},
                {"Price", "Giá"},
                {"Duration", "Thời gian"},
                {"Location", "Địa điểm"},
                {"Rating", "Đánh giá"},
                {"Reviews", "Nhận xét"},
                {"Gallery", "Thư viện ảnh"},
                {"Description", "Mô tả"},
                
                // Navigation
                {"Tours", "Tours"},
                {"Hotels", "Khách sạn"},
                {"Destinations", "Điểm đến"},
                {"Blog", "Blog"},
                {"MyAccount", "Tài khoản"},
                {"Dashboard", "Bảng điều khiển"},
                {"Bookings", "Đặt chỗ"},
                {"Wishlist", "Yêu thích"},
                {"Notifications", "Thông báo"},
                
                // Forms
                {"Name", "Tên"},
                {"Email", "Email"},
                {"Password", "Mật khẩu"},
                {"ConfirmPassword", "Xác nhận mật khẩu"},
                {"PhoneNumber", "Số điện thoại"},
                {"Address", "Địa chỉ"},
                {"City", "Thành phố"},
                {"Country", "Quốc gia"},
                {"DateOfBirth", "Ngày sinh"},
                {"Gender", "Giới tính"},
                {"Male", "Nam"},
                {"Female", "Nữ"},
                {"Submit", "Gửi"},
                {"Cancel", "Hủy"},
                {"Save", "Lưu"},
                {"Edit", "Sửa"},
                {"Delete", "Xóa"},
                {"Update", "Cập nhật"},
                
                // Messages
                {"Welcome", "Chào mừng"},
                {"LoginSuccess", "Đăng nhập thành công"},
                {"LoginFailed", "Đăng nhập thất bại"},
                {"RegisterSuccess", "Đăng ký thành công"},
                {"BookingSuccess", "Đặt tour thành công"},
                {"BookingFailed", "Đặt tour thất bại"},
                {"InvalidCredentials", "Thông tin đăng nhập không chính xác"},
                {"AccountLocked", "Tài khoản đã bị khóa"},
                {"PasswordTooWeak", "Mật khẩu quá yếu"},
                
                // Tour related
                {"PopularTours", "Tours phổ biến"},
                {"FeaturedDestinations", "Điểm đến nổi bật"},
                {"TourPackages", "Gói tour"},
                {"BookingDetails", "Chi tiết đặt chỗ"},
                {"TravelDate", "Ngày đi"},
                {"NumberOfPeople", "Số người"},
                {"TotalPrice", "Tổng giá"},
                {"PaymentMethod", "Phương thức thanh toán"},
                
                // Status
                {"Confirmed", "Đã xác nhận"},
                {"Pending", "Đang chờ"},
                {"Cancelled", "Đã hủy"},
                {"Completed", "Hoàn thành"},
                {"Active", "Hoạt động"},
                {"Inactive", "Không hoạt động"}
            };

            // English resources
            _resources["en"] = new Dictionary<string, string>
            {
                // Common
                {"Home", "Home"},
                {"About", "About"},
                {"Contact", "Contact"},
                {"Login", "Login"},
                {"Register", "Register"},
                {"Logout", "Logout"},
                {"Search", "Search"},
                {"Book", "Book"},
                {"BookNow", "Book Now"},
                {"ViewDetails", "View Details"},
                {"Price", "Price"},
                {"Duration", "Duration"},
                {"Location", "Location"},
                {"Rating", "Rating"},
                {"Reviews", "Reviews"},
                {"Gallery", "Gallery"},
                {"Description", "Description"},
                
                // Navigation
                {"Tours", "Tours"},
                {"Hotels", "Hotels"},
                {"Destinations", "Destinations"},
                {"Blog", "Blog"},
                {"MyAccount", "My Account"},
                {"Dashboard", "Dashboard"},
                {"Bookings", "Bookings"},
                {"Wishlist", "Wishlist"},
                {"Notifications", "Notifications"},
                
                // Forms
                {"Name", "Name"},
                {"Email", "Email"},
                {"Password", "Password"},
                {"ConfirmPassword", "Confirm Password"},
                {"PhoneNumber", "Phone Number"},
                {"Address", "Address"},
                {"City", "City"},
                {"Country", "Country"},
                {"DateOfBirth", "Date of Birth"},
                {"Gender", "Gender"},
                {"Male", "Male"},
                {"Female", "Female"},
                {"Submit", "Submit"},
                {"Cancel", "Cancel"},
                {"Save", "Save"},
                {"Edit", "Edit"},
                {"Delete", "Delete"},
                {"Update", "Update"},
                
                // Messages
                {"Welcome", "Welcome"},
                {"LoginSuccess", "Login successful"},
                {"LoginFailed", "Login failed"},
                {"RegisterSuccess", "Registration successful"},
                {"BookingSuccess", "Booking successful"},
                {"BookingFailed", "Booking failed"},
                {"InvalidCredentials", "Invalid credentials"},
                {"AccountLocked", "Account is locked"},
                {"PasswordTooWeak", "Password is too weak"},
                
                // Tour related
                {"PopularTours", "Popular Tours"},
                {"FeaturedDestinations", "Featured Destinations"},
                {"TourPackages", "Tour Packages"},
                {"BookingDetails", "Booking Details"},
                {"TravelDate", "Travel Date"},
                {"NumberOfPeople", "Number of People"},
                {"TotalPrice", "Total Price"},
                {"PaymentMethod", "Payment Method"},
                
                // Status
                {"Confirmed", "Confirmed"},
                {"Pending", "Pending"},
                {"Cancelled", "Cancelled"},
                {"Completed", "Completed"},
                {"Active", "Active"},
                {"Inactive", "Inactive"}
            };
        }

        public string GetString(string key, string? culture = null)
        {
            culture ??= _currentCulture;
            
            if (_resources.ContainsKey(culture) && _resources[culture].ContainsKey(key))
            {
                return _resources[culture][key];
            }
            
            // Fallback to Vietnamese if key not found in requested culture
            if (culture != "vi" && _resources["vi"].ContainsKey(key))
            {
                return _resources["vi"][key];
            }
            
            return key; // Return key if translation not found
        }

        public string GetString(string key, string culture, params object[] args)
        {
            var format = GetString(key, culture);
            return string.Format(format, args);
        }

        public void SetCulture(string culture)
        {
            if (_resources.ContainsKey(culture))
            {
                _currentCulture = culture;
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
            }
        }

        public string GetCurrentCulture()
        {
            return _currentCulture;
        }

        public List<string> GetSupportedCultures()
        {
            return _resources.Keys.ToList();
        }

        public Dictionary<string, string> GetAllStrings(string culture)
        {
            return _resources.ContainsKey(culture) ? _resources[culture] : new Dictionary<string, string>();
        }
    }
}
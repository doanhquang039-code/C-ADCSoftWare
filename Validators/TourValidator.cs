using FluentValidation;
using WEBDULICH.ViewModels;

namespace WEBDULICH.Validators
{
    public class TourValidator : AbstractValidator<TourViewModel>
    {
        public TourValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên tour không được để trống")
                .MaximumLength(200).WithMessage("Tên tour không được vượt quá 200 ký tự");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Mô tả không được để trống")
                .MaximumLength(2000).WithMessage("Mô tả không được vượt quá 2000 ký tự");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Giá phải lớn hơn 0");

            RuleFor(x => x.Duration)
                .GreaterThan(0).WithMessage("Thời gian phải lớn hơn 0");

            RuleFor(x => x.MaxParticipants)
                .GreaterThan(0).WithMessage("Số lượng người tham gia phải lớn hơn 0");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Vui lòng chọn danh mục");

            RuleFor(x => x.DestinationId)
                .GreaterThan(0).WithMessage("Vui lòng chọn điểm đến");
        }
    }

    public class HotelValidator : AbstractValidator<HotelViewModel>
    {
        public HotelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên khách sạn không được để trống")
                .MaximumLength(200).WithMessage("Tên khách sạn không được vượt quá 200 ký tự");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Mô tả không được để trống")
                .MaximumLength(2000).WithMessage("Mô tả không được vượt quá 2000 ký tự");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Địa chỉ không được để trống")
                .MaximumLength(500).WithMessage("Địa chỉ không được vượt quá 500 ký tự");

            RuleFor(x => x.PricePerNight)
                .GreaterThan(0).WithMessage("Giá phải lớn hơn 0");

            RuleFor(x => x.Rating)
                .InclusiveBetween(0, 5).WithMessage("Đánh giá phải từ 0 đến 5 sao");

            RuleFor(x => x.DestinationId)
                .GreaterThan(0).WithMessage("Vui lòng chọn điểm đến");
        }
    }

    public class BookingValidator : AbstractValidator<BookingViewModel>
    {
        public BookingValidator()
        {
            RuleFor(x => x.TourId)
                .GreaterThan(0).WithMessage("Vui lòng chọn tour");

            RuleFor(x => x.NumberOfPeople)
                .GreaterThan(0).WithMessage("Số lượng người phải lớn hơn 0");

            RuleFor(x => x.BookingDate)
                .NotEmpty().WithMessage("Ngày đặt không được để trống")
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Ngày đặt phải từ hôm nay trở đi");

            RuleFor(x => x.ContactName)
                .NotEmpty().WithMessage("Tên liên hệ không được để trống")
                .MaximumLength(100).WithMessage("Tên liên hệ không được vượt quá 100 ký tự");

            RuleFor(x => x.ContactEmail)
                .NotEmpty().WithMessage("Email không được để trống")
                .EmailAddress().WithMessage("Email không hợp lệ");

            RuleFor(x => x.ContactPhone)
                .NotEmpty().WithMessage("Số điện thoại không được để trống")
                .Matches(@"^[0-9]{10,11}$").WithMessage("Số điện thoại không hợp lệ");
        }
    }
}

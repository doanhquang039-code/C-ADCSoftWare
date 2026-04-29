using AutoMapper;
using WEBDULICH.Models;
using WEBDULICH.ViewModels;

namespace WEBDULICH.Mappings
{
    public class TourProfile : Profile
    {
        public TourProfile()
        {
            // Tour mappings - simplified to avoid errors with missing properties
            CreateMap<Tour, TourViewModel>();
            CreateMap<TourViewModel, Tour>();

            // Hotel mappings
            CreateMap<Hotel, HotelViewModel>();
            CreateMap<HotelViewModel, Hotel>();

            // Booking mappings
            CreateMap<Booking, BookingViewModel>();
            CreateMap<BookingViewModel, Booking>();
        }
    }
}

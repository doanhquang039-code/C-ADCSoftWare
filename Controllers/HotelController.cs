using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class HotelController : Controller
    {
        private readonly IHotelService hotelService;
        private readonly ITourService tourService;

        public HotelController(IHotelService hotelService, ITourService tourService)
        {
            this.hotelService = hotelService;
            this.tourService = tourService;
        }

        public async Task<IActionResult> Index(string? keyword, decimal? minPrice, decimal? maxPrice,
            int? rating, int? tourId, string? address, string? sortBy, string? sortDir, int page = 1, int pageSize = 10)
        {
            var result = await hotelService.GetPagedAsync(keyword, minPrice, maxPrice,
                rating, tourId, address, sortBy, sortDir, page, pageSize);

            ViewBag.Tours = await tourService.GetAllAsync();
            return View(result);
        }

        [AdminOnly]
        public async Task<IActionResult> Create()
        {
            await PopulateTours();
            return View();
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Create(Hotel hotel, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTours(hotel.TourId);
                return View(hotel);
            }

            await hotelService.CreateAsync(hotel, imageFile);
            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var hotel = await hotelService.GetByIdAsync(id);
            if (hotel == null) return NotFound();

            await PopulateTours(hotel.TourId);
            return View(hotel);
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Edit(Hotel hotel, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTours(hotel.TourId);
                return View(hotel);
            }

            await hotelService.UpdateAsync(hotel, imageFile);
            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public async Task<IActionResult> Delete(int id)
        {
            await hotelService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateTours(int? selectedTourId = null)
        {
            var tours = await tourService.GetAllAsync();
            ViewBag.Tours = new SelectList(tours, "Id", "Name", selectedTourId);
        }
    }
}

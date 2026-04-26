using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class TourController : Controller
    {
        private readonly ITourService tourService;
        private readonly IDestinationService destinationService;

        public TourController(ITourService tourService, IDestinationService destinationService)
        {
            this.tourService = tourService;
            this.destinationService = destinationService;
        }

        public async Task<IActionResult> Index(string? keyword, decimal? minPrice, decimal? maxPrice,
            int? duration, int? destinationId, string? sortBy, string? sortDir, int page = 1, int pageSize = 10)
        {
            var result = await tourService.GetPagedAsync(keyword, minPrice, maxPrice,
                duration, destinationId, sortBy, sortDir, page, pageSize);

            ViewBag.Destinations = await destinationService.GetAllAsync();
            return View(result);
        }

        [AdminOnly]
        public async Task<IActionResult> Create()
        {
            await PopulateDestinations();
            return View();
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Create(Tour tour, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDestinations(tour.DestinationId);
                return View(tour);
            }

            await tourService.CreateAsync(tour, imageFile);
            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var tour = await tourService.GetByIdAsync(id);
            if (tour == null) return NotFound();

            await PopulateDestinations(tour.DestinationId);
            return View(tour);
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Edit(Tour tour, IFormFile? imageFile, string? oldImage)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDestinations(tour.DestinationId);
                return View(tour);
            }

            await tourService.UpdateAsync(tour, imageFile, oldImage);
            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public async Task<IActionResult> Delete(int id)
        {
            await tourService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDestinations(int? selectedDestinationId = null)
        {
            var destinations = await destinationService.GetAllAsync();
            ViewBag.Destinations = new SelectList(destinations, "Id", "Name", selectedDestinationId);
        }
    }
}

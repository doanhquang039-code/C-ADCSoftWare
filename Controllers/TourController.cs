using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class TourController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IImageStorageService imageStorageService;

        public TourController(ApplicationDbContext db, IImageStorageService imageStorageService)
        {
            this.db = db;
            this.imageStorageService = imageStorageService;
        }

        public IActionResult Index()
        {
            var list = db.Tours.Include(t => t.Destination)
                .Include(t => t.Orders)
                .Include(t => t.Reviews)
                .Include(t => t.Hotels)
                .ToList();
            return View(list);
        }

        [AdminOnly]
        public IActionResult Create()
        {
            PopulateDestinations();
            return View();
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Create(Tour tour, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                PopulateDestinations(tour.DestinationId);
                return View(tour);
            }

            tour.Image = await imageStorageService.SaveAsync(imageFile, "ImageTour") ?? string.Empty;
            db.Tours.Add(tour);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public IActionResult Edit(int id)
        {
            var tour = db.Tours.Find(id);
            if (tour == null)
            {
                return NotFound();
            }

            PopulateDestinations(tour.DestinationId);
            return View(tour);
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Edit(Tour tour, IFormFile? imageFile, string? oldImage)
        {
            if (!ModelState.IsValid)
            {
                PopulateDestinations(tour.DestinationId);
                return View(tour);
            }

            var existingTour = db.Tours.AsNoTracking().FirstOrDefault(x => x.Id == tour.Id);
            if (existingTour == null)
            {
                return NotFound();
            }

            var newImage = await imageStorageService.SaveAsync(imageFile, "ImageTour");
            if (!string.IsNullOrWhiteSpace(newImage))
            {
                imageStorageService.Delete("ImageTour", oldImage ?? existingTour.Image);
                tour.Image = newImage;
            }
            else
            {
                tour.Image = oldImage ?? existingTour.Image;
            }

            db.Tours.Update(tour);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public IActionResult Delete(int id)
        {
            var tour = db.Tours.Find(id);
            if (tour == null)
            {
                return NotFound();
            }

            imageStorageService.Delete("ImageTour", tour.Image);
            db.Tours.Remove(tour);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Filter(string keyword, decimal? minPrice, decimal? maxPrice, int? duration)
        {
            var tours = db.Tours.Include(t => t.Destination).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                tours = tours.Where(t => t.Name.Contains(keyword));
            }

            if (minPrice.HasValue)
            {
                tours = tours.Where(t => t.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                tours = tours.Where(t => t.Price <= maxPrice.Value);
            }

            if (duration.HasValue)
            {
                tours = tours.Where(t => t.Duration == duration.Value);
            }

            ViewBag.Keyword = keyword;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Duration = duration;

            return View("Index", tours.ToList());
        }

        public IActionResult Demo1(string sortOrder, int? destinationId)
        {
            var tours = db.Tours.AsQueryable();

            ViewBag.SortOrder = sortOrder;
            ViewBag.DestinationId = destinationId;
            ViewBag.Destinations = db.Destinations.ToList();

            if (destinationId.HasValue)
            {
                tours = tours.Where(t => t.DestinationId == destinationId.Value);
            }

            tours = sortOrder switch
            {
                "price_asc" => tours.OrderBy(t => t.Price),
                "price_desc" => tours.OrderByDescending(t => t.Price),
                "quantity_asc" => tours.OrderBy(t => t.Quantity),
                "quantity_desc" => tours.OrderByDescending(t => t.Quantity),
                _ => tours.OrderBy(t => t.Id)
            };

            return View("Index", tours.ToList());
        }

        private void PopulateDestinations(int? selectedDestinationId = null)
        {
            ViewBag.Destinations = new SelectList(db.Destinations, "Id", "Name", selectedDestinationId);
        }
    }
}

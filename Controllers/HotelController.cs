using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class HotelController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IImageStorageService imageStorageService;

        public HotelController(ApplicationDbContext db, IImageStorageService imageStorageService)
        {
            this.db = db;
            this.imageStorageService = imageStorageService;
        }

        public IActionResult Index()
        {
            var list = db.Hotels.Include(h => h.Tour).ToList();
            return View(list);
        }

        [AdminOnly]
        public IActionResult Create()
        {
            PopulateTours();
            return View();
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Create(Hotel hotel, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                PopulateTours(hotel.TourId);
                return View(hotel);
            }

            hotel.Image = await imageStorageService.SaveAsync(imageFile, "ImageHotel") ?? string.Empty;
            db.Hotels.Add(hotel);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public IActionResult Edit(int id)
        {
            var hotel = db.Hotels.Find(id);
            if (hotel == null)
            {
                return NotFound();
            }

            PopulateTours(hotel.TourId);
            return View(hotel);
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Edit(Hotel hotel, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                PopulateTours(hotel.TourId);
                return View(hotel);
            }

            var existingHotel = db.Hotels.AsNoTracking().FirstOrDefault(x => x.Id == hotel.Id);
            if (existingHotel == null)
            {
                return NotFound();
            }

            var newImage = await imageStorageService.SaveAsync(imageFile, "ImageHotel");
            if (!string.IsNullOrWhiteSpace(newImage))
            {
                imageStorageService.Delete("ImageHotel", existingHotel.Image);
                hotel.Image = newImage;
            }
            else
            {
                hotel.Image = existingHotel.Image;
            }

            db.Hotels.Update(hotel);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public IActionResult Delete(int id)
        {
            var hotel = db.Hotels.Find(id);
            if (hotel == null)
            {
                return NotFound();
            }

            imageStorageService.Delete("ImageHotel", hotel.Image);
            db.Hotels.Remove(hotel);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Filter(string keyword, decimal? minPrice, decimal? maxPrice, int? rating)
        {
            var hotels = db.Hotels.Include(h => h.Tour).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                hotels = hotels.Where(h => h.Name.Contains(keyword));
            }

            if (minPrice.HasValue)
            {
                hotels = hotels.Where(h => h.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                hotels = hotels.Where(h => h.Price <= maxPrice.Value);
            }

            if (rating.HasValue)
            {
                hotels = hotels.Where(h => h.Rating == rating.Value);
            }

            ViewBag.KeyWord = keyword;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Rating = rating;

            return View("Index", hotels.ToList());
        }

        public IActionResult Demo1(string sortOrder, string address1)
        {
            var hotels = db.Hotels.AsQueryable();
            ViewBag.SortOrder = sortOrder;
            ViewBag.Address = address1;

            if (!string.IsNullOrWhiteSpace(address1))
            {
                hotels = hotels.Where(h => h.Address.Contains(address1));
            }

            hotels = sortOrder switch
            {
                "price_asc" => hotels.OrderBy(t => t.Price),
                "price_desc" => hotels.OrderByDescending(t => t.Price),
                "quantity_asc" => hotels.OrderBy(t => t.Quantity),
                "quantity_desc" => hotels.OrderByDescending(t => t.Quantity),
                _ => hotels.OrderBy(t => t.Id)
            };

            return View("Index", hotels.ToList());
        }

        private void PopulateTours(int? selectedTourId = null)
        {
            ViewBag.Tours = new SelectList(db.Tours, "Id", "Name", selectedTourId);
        }
    }
}

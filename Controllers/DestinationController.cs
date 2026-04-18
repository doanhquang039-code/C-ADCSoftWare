using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class DestinationController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IImageStorageService imageStorageService;

        public DestinationController(ApplicationDbContext db, IImageStorageService imageStorageService)
        {
            this.db = db;
            this.imageStorageService = imageStorageService;
        }

        public IActionResult Index()
        {
            var list = db.Destinations
                .Include(d => d.Category)
                .Include(d => d.Tours)
                .ToList();
            return View(list);
        }

        [AdminOnly]
        public IActionResult Create()
        {
            PopulateCategories();
            return View();
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Create(Destination destination, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                PopulateCategories(destination.CategoryId);
                return View(destination);
            }

            destination.Image = await imageStorageService.SaveAsync(imageFile, "ImageDestination") ?? string.Empty;
            db.Destinations.Add(destination);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public IActionResult Edit(int id)
        {
            var destination = db.Destinations.Find(id);
            if (destination == null)
            {
                return NotFound();
            }

            PopulateCategories(destination.CategoryId);
            return View(destination);
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Edit(Destination destination, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                PopulateCategories(destination.CategoryId);
                return View(destination);
            }

            var existingDestination = db.Destinations.AsNoTracking().FirstOrDefault(x => x.Id == destination.Id);
            if (existingDestination == null)
            {
                return NotFound();
            }

            var newImage = await imageStorageService.SaveAsync(imageFile, "ImageDestination");
            if (!string.IsNullOrWhiteSpace(newImage))
            {
                imageStorageService.Delete("ImageDestination", existingDestination.Image);
                destination.Image = newImage;
            }
            else
            {
                destination.Image = existingDestination.Image;
            }

            db.Destinations.Update(destination);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public IActionResult Delete(int id)
        {
            var destination = db.Destinations.Find(id);
            if (destination == null)
            {
                return NotFound();
            }

            imageStorageService.Delete("ImageDestination", destination.Image);
            db.Destinations.Remove(destination);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var destination = db.Destinations
                .Include(d => d.Category)
                .Include(d => d.Tours)
                .FirstOrDefault(x => x.Id == id);
            if (destination == null)
            {
                return NotFound();
            }

            return View(destination);
        }

        public IActionResult SearchByRegion(string region)
        {
            var query = db.Destinations
                .Include(d => d.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(region))
            {
                query = query.Where(d => d.Category != null && d.Category.Name == region);
            }

            ViewBag.SelectedRegion = region;
            return View("Index", query.ToList());
        }

        private void PopulateCategories(int? selectedCategoryId = null)
        {
            ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", selectedCategoryId);
        }
    }
}

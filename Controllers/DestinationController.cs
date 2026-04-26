using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class DestinationController : Controller
    {
        private readonly IDestinationService destinationService;
        private readonly ICategoryService categoryService;

        public DestinationController(IDestinationService destinationService, ICategoryService categoryService)
        {
            this.destinationService = destinationService;
            this.categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string? keyword, int? categoryId, string? location,
            string? sortBy, string? sortDir, int page = 1, int pageSize = 10)
        {
            var result = await destinationService.GetPagedAsync(keyword, categoryId, location,
                sortBy, sortDir, page, pageSize);

            ViewBag.Categories = await categoryService.GetAllAsync();
            return View(result);
        }

        [AdminOnly]
        public async Task<IActionResult> Create()
        {
            await PopulateCategories();
            return View();
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Create(Destination destination, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategories(destination.CategoryId);
                return View(destination);
            }

            await destinationService.CreateAsync(destination, imageFile);
            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var destination = await destinationService.GetByIdAsync(id);
            if (destination == null) return NotFound();

            await PopulateCategories(destination.CategoryId);
            return View(destination);
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> Edit(Destination destination, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategories(destination.CategoryId);
                return View(destination);
            }

            await destinationService.UpdateAsync(destination, imageFile);
            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public async Task<IActionResult> Delete(int id)
        {
            await destinationService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var destination = await destinationService.GetByIdAsync(id);
            if (destination == null) return NotFound();

            return View(destination);
        }

        private async Task PopulateCategories(int? selectedCategoryId = null)
        {
            var categories = await categoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategoryId);
        }
    }
}

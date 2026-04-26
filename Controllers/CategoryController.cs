using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    [AdminOnly]
    public class CategoryController : Controller
    {
        private readonly ICategoryService categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            this.categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string? keyword, string? sortBy, string? sortDir, int page = 1, int pageSize = 10)
        {
            var result = await categoryService.GetPagedAsync(keyword, sortBy, sortDir, page, pageSize);
            return View(result);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var success = await categoryService.CreateAsync(category);
            if (!success)
            {
                ModelState.AddModelError("Name", "Tên danh mục đã tồn tại!");
                return View(category);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            await categoryService.UpdateAsync(category);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            await categoryService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

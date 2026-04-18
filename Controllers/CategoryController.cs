using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    [AdminOnly]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext db;

        public CategoryController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            var categories = db.Categories.ToList();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            var normalizedName = category.Name?.Trim().ToLower();
            var isExists = !string.IsNullOrWhiteSpace(normalizedName)
                && db.Categories.Any(c => c.Name.ToLower() == normalizedName);

            if (isExists)
            {
                ModelState.AddModelError("Name", "Tên danh mục đã tồn tại!");
            }

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            db.Categories.Add(category);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var category = db.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            db.Categories.Update(category);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var category = db.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

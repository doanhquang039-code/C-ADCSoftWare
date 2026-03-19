using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
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
        public IActionResult Create(Category c)
        {
            var isExists = db.Categories.Any(cl => cl.Name.ToLower() == c.Name.ToLower());

            if (isExists == true)
            {
                ModelState.AddModelError("Name", "Tên lớp đã tồn tại!");
            }
            if (!ModelState.IsValid)
            {
                db.Categories.Add(c);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(c);
        }

        public IActionResult Edit(int id)
        {
            var c = db.Categories.Find(id);
            return View(c);
        }

        [HttpPost]
        public IActionResult Edit(Category c)
        {
            db.Categories.Update(c);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var c = db.Categories.Find(id);
            db.Categories.Remove(c);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}


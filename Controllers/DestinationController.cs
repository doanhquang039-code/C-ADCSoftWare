

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class DestinationController : Controller
    {
        private readonly ApplicationDbContext db;

        public DestinationController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            var list = db.Destinations
                         .Include(d => d.Category)
                         .Include(d => d.Tours)
                         .ToList();
            return View(list);
        }
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(db.Categories, "Id", "Name");
            return View();
        }

        
        [HttpPost]
        public IActionResult Create(Destination d, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ImageDestination");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var filePath = Path.Combine(path, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                d.Image = fileName;
            }

            if (ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", d.CategoryId);
                return View(d);
            }

            db.Destinations.Add(d);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var d = db.Destinations.Find(id);
            if (d == null) return NotFound();
            ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", d.CategoryId);
            return View(d);
        }

        [HttpPost]
        public IActionResult Edit(Destination d, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ImageDestination");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                d.Image = fileName; // Cập nhật ảnh mới
            }

            if (string.IsNullOrEmpty(d.Image))
            {
                var old = db.Destinations.AsNoTracking().FirstOrDefault(x => x.Id == d.Id);
                d.Image = old?.Image;
            }

            if (ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", d.CategoryId);
                return View(d);
            }

            db.Destinations.Update(d);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public IActionResult Delete(int id)
        {
            var d = db.Destinations.Find(id);
            if (d == null) return NotFound();

            db.Destinations.Remove(d);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

      
        public IActionResult Details(int id)
        {
            var d = db.Destinations
                      .Include(d => d.Category)
                      .Include(d => d.Tours)
                      .FirstOrDefault(x => x.Id == id);
            if (d == null) return NotFound();

            return View(d);
        }
        public IActionResult SearchByRegion(string region)
        {
            var query = db.Destinations
                          .Include(d => d.Category)
                          .AsQueryable();

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(d => d.Category.Name == region);
            }

            ViewBag.SelectedRegion = region;
            return View("Index", query.ToList()); 
        }

    }
}

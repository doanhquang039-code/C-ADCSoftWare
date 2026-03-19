using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WEBDULICH.Models;
using WEBDULICH.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WEBDULICH.Controllers
{
    public class TourController : Controller
    {
        private readonly ApplicationDbContext db1;
        public TourController(ApplicationDbContext db1)
        {
            this.db1 = db1;
        }

        public IActionResult Index()
        {
            var list = db1.Tours.Include(t => t.Destination)
                               .Include(t => t.Orders)
                               .Include(t => t.Reviews)
                               .Include(t => t.Hotels).ToList();
            return View(list);
        }
        public IActionResult Create()
        {
       
            ViewBag.DestinationNames = db1.Destinations.Select(d => d.Name).ToList();

            return View();
        }
        [HttpPost]
        public IActionResult Create(Tour t, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ImageTour");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);

                }
                t.Image = fileName;
            }
            if (ModelState.IsValid)
            {
                ViewBag.Destinations = new SelectList(db1.Destinations, "Id", "Name", t.DestinationId);
                    return View(t);
            }
            db1.Tours.Add(t);
            db1.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            var t = db1.Tours.Find(id);
            if (t == null)

                return NotFound();
            ViewBag.Destinations = new SelectList(db1.Destinations, "Id", "Name", t.DestinationId);

            return View(t);
        }
        [HttpPost]
        public IActionResult Edit(Tour t, IFormFile imageFile, string OldImage)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ImageTour");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                t.Image = fileName;

                // Xóa ảnh cũ nếu muốn
                var oldPath = Path.Combine(folderPath, OldImage ?? "");
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }
            else
            {
                t.Image = OldImage; // giữ ảnh cũ
            }

            if (ModelState.IsValid)
            {
                ViewBag.Destinations = new SelectList(db1.Destinations, "Id", "Name", t.DestinationId);
                return View(t);
            }

            db1.Tours.Update(t);
            db1.SaveChanges();
            return RedirectToAction("Index");
        }


        public IActionResult Delete(int id)
        {
            var abc = db1.Tours.Find(id);
            db1.Tours.Remove(abc);
            db1.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Filter(string keyword, decimal? minPrice, decimal? maxPrice, int? duration)
        {
            var tours = db1.Tours.Include(t => t.Destination).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                tours = tours.Where(t => t.Name.Contains(keyword));
            }
            // nếu em chuyển thể 
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
            var tours = db1.Tours.AsQueryable();

            ViewBag.SortOrder = sortOrder;
            ViewBag.DestinationId = destinationId;
            ViewBag.Destinations = db1.Destinations.ToList(); 

            if (destinationId.HasValue)
            {
                tours = tours.Where(t => t.DestinationId == destinationId.Value);
            }

            switch (sortOrder)
            {
                case "price_asc":
                    tours = tours.OrderBy(t => t.Price);
                    break;
                case "price_desc":
                    tours = tours.OrderByDescending(t => t.Price);
                    break;
                case "quantity_asc":
                    tours = tours.OrderBy(t => t.Quantity);
                    break;
                case "quantity_desc":
                    tours = tours.OrderByDescending(t => t.Quantity);
                    break;
                default:
                    tours = tours.OrderBy(t => t.Id);
                    break;
            }

            return View("Index", tours.ToList());
        }

       

    }
}

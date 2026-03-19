using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{

    public class HotelController : Controller
    {
        private readonly ApplicationDbContext db3;

        public HotelController(ApplicationDbContext db3)
        {
            this.db3 = db3;
        }

        public IActionResult Index()
        {
            var list = db3.Hotels.Include(h => h.Tour).ToList();
            return View(list);
        }


        public IActionResult Create()
        {
            ViewBag.Tours = new SelectList(db3.Tours, "Id", "Name");
            return View();
        }


        [HttpPost]
        public IActionResult Create(Hotel h, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ImageHotel");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var filePath = Path.Combine(path, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                h.Image = fileName;
            }

            if (ModelState.IsValid)
            {
                ViewBag.Tours = new SelectList(db3.Tours, "Id", "Name");
                return View(h);
            }

            db3.Hotels.Add(h);
            db3.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var h = db3.Hotels.Find(id);
            if (h == null) return NotFound();

            ViewBag.Tours = new SelectList(db3.Tours, "Id", "Name", h.TourId);
            return View(h);
        }

        [HttpPost]
        public IActionResult Edit(Hotel h, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ImageHotel");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var filePath = Path.Combine(path, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                h.Image = fileName;
            }
            else
            {
                var old = db3.Hotels.AsNoTracking().FirstOrDefault(x => x.Id == h.Id);
                h.Image = old?.Image;
            }

            if (ModelState.IsValid)
            {
                ViewBag.Tours = new SelectList(db3.Tours, "Id", "Name", h.TourId);
                return View(h);
            }

            db3.Hotels.Update(h);
            db3.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var h = db3.Hotels.Find(id);
            if (h == null) return NotFound();

            db3.Hotels.Remove(h);
            db3.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Filter(string keyword, decimal? minPrice, decimal? maxPrice, int? rating)
        {
            var hotels = db3.Hotels.Include(h => h.Tour).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
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
        public IActionResult Demo1(string sortOrder ,string address1)
        {
            var hotels = db3.Hotels.AsQueryable();
            ViewBag.SortOrder = sortOrder;
            ViewBag.Address= address1;
            if (!string.IsNullOrEmpty(address1))
            {
                hotels = hotels.Where(h => h.Address.Contains(address1));
            }
            switch (sortOrder)
            {
                case "price_asc":
                    hotels = hotels.OrderBy(t => t.Price);
                    break;
                case "price_desc":
                    hotels = hotels.OrderByDescending(t => t.Price);
                    break;
                case "quantity_asc":
                    hotels = hotels.OrderBy(t => t.Quantity);
                    break;
                case "quantity_desc":
                    hotels = hotels.OrderByDescending(t => t.Quantity);
                    break;
                default:
                    hotels = hotels.OrderBy(t => t.Id);
                    break;
            }

            return View("Index", hotels.ToList());
        }
       

    }
}

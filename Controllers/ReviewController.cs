using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using WEBDULICH.Services;
using WEBDULICH.Helpers;

namespace WEBDULICH.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext db;

        public ReviewController(ApplicationDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            var reviews = db.Reviews
                 .Include(r => r.User)
                 .Include(r => r.Tour)
                 .ToList();
            return View(reviews);

        }

        public IActionResult Create()
        {
            var user = HttpContext.Session.GetObject<User>("userLogin");
            if (user == null)
                return RedirectToAction("Login", "User");

            ViewBag.Tours = new SelectList(db.Tours, "Id", "Name");
            return View();
        }

        [HttpPost]
        public IActionResult Create(Review r)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Users = new SelectList(db.Users, "Id", "Name", r.UserId);
                ViewBag.Tours = new SelectList(db.Tours, "Id", "Name", r.TourId);
                return View(r);
            }

            if (string.IsNullOrEmpty(r.Rating))
            {
                ModelState.AddModelError("Rating", "Vui lòng chọn số sao.");
                ViewBag.Users = new SelectList(db.Users, "Id", "Name", r.UserId);
                ViewBag.Tours = new SelectList(db.Tours, "Id", "Name", r.TourId);
                return View(r);
            }
            if (r.ReviewDate == DateTime.MinValue)  
            {
                r.ReviewDate = DateTime.Now;
            }

            db.Reviews.Add(r);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public IActionResult Edit(int id)
        {
            var review = db.Reviews.Find(id);
            if (review == null) return NotFound();

            ViewBag.Tours = new SelectList(db.Tours, "Id", "Name", review.TourId);
            return View(review);
        }

        [HttpPost]
        public IActionResult Edit(Review review)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Tours = new SelectList(db.Tours, "Id", "Name", review.TourId);
                return View(review);
            }

            db.Reviews.Update(review);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var review = db.Reviews.Find(id);
            if (review == null) return NotFound();

            db.Reviews.Remove(review);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}


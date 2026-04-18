using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ICurrentUserService currentUserService;

        public ReviewController(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            db = context;
            this.currentUserService = currentUserService;
        }

        public IActionResult Index()
        {
            var reviews = db.Reviews
                .Include(r => r.User)
                .Include(r => r.Tour)
                .ToList();
            return View(reviews);
        }

        [AuthenticatedOnly]
        public IActionResult Create()
        {
            ViewBag.Tours = new SelectList(db.Tours, "Id", "Name");
            return View();
        }

        [HttpPost]
        [AuthenticatedOnly]
        public IActionResult Create(Review review)
        {
            var currentUser = currentUserService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "User");
            }

            if (string.IsNullOrWhiteSpace(review.Rating))
            {
                ModelState.AddModelError("Rating", "Vui lòng chọn số sao.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Tours = new SelectList(db.Tours, "Id", "Name", review.TourId);
                return View(review);
            }

            review.UserId = currentUser.Id;
            if (review.ReviewDate == DateTime.MinValue)
            {
                review.ReviewDate = DateTime.Now;
            }

            db.Reviews.Add(review);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [AuthenticatedOnly]
        public IActionResult Edit(int id)
        {
            var review = db.Reviews.Find(id);
            if (review == null)
            {
                return NotFound();
            }

            ViewBag.Tours = new SelectList(db.Tours, "Id", "Name", review.TourId);
            return View(review);
        }

        [HttpPost]
        [AuthenticatedOnly]
        public IActionResult Edit(Review review)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Tours = new SelectList(db.Tours, "Id", "Name", review.TourId);
                return View(review);
            }

            db.Reviews.Update(review);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [AuthenticatedOnly]
        public IActionResult Delete(int id)
        {
            var review = db.Reviews.Find(id);
            if (review == null)
            {
                return NotFound();
            }

            db.Reviews.Remove(review);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

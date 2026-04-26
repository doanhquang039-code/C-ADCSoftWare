using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewService reviewService;
        private readonly ITourService tourService;
        private readonly ICurrentUserService currentUserService;

        public ReviewController(IReviewService reviewService, ITourService tourService, ICurrentUserService currentUserService)
        {
            this.reviewService = reviewService;
            this.tourService = tourService;
            this.currentUserService = currentUserService;
        }

        public async Task<IActionResult> Index(string? keyword, int? tourId, string? rating,
            string? sortBy, string? sortDir, int page = 1, int pageSize = 10)
        {
            var result = await reviewService.GetPagedAsync(keyword, tourId, rating,
                sortBy, sortDir, page, pageSize);

            ViewBag.Tours = await tourService.GetAllAsync();
            return View(result);
        }

        [AuthenticatedOnly]
        public async Task<IActionResult> Create()
        {
            var tours = await tourService.GetAllAsync();
            ViewBag.Tours = new SelectList(tours, "Id", "Name");
            return View();
        }

        [HttpPost]
        [AuthenticatedOnly]
        public async Task<IActionResult> Create(Review review)
        {
            var currentUser = currentUserService.GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "User");

            if (string.IsNullOrWhiteSpace(review.Rating))
            {
                ModelState.AddModelError("Rating", "Vui lòng chọn số sao.");
            }

            if (!ModelState.IsValid)
            {
                var tours = await tourService.GetAllAsync();
                ViewBag.Tours = new SelectList(tours, "Id", "Name", review.TourId);
                return View(review);
            }

            review.UserId = currentUser.Id;
            await reviewService.CreateAsync(review);
            return RedirectToAction(nameof(Index));
        }

        [AuthenticatedOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var review = await reviewService.GetByIdAsync(id);
            if (review == null) return NotFound();

            var tours = await tourService.GetAllAsync();
            ViewBag.Tours = new SelectList(tours, "Id", "Name", review.TourId);
            return View(review);
        }

        [HttpPost]
        [AuthenticatedOnly]
        public async Task<IActionResult> Edit(Review review)
        {
            if (!ModelState.IsValid)
            {
                var tours = await tourService.GetAllAsync();
                ViewBag.Tours = new SelectList(tours, "Id", "Name", review.TourId);
                return View(review);
            }

            await reviewService.UpdateAsync(review);
            return RedirectToAction(nameof(Index));
        }

        [AuthenticatedOnly]
        public async Task<IActionResult> Delete(int id)
        {
            await reviewService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

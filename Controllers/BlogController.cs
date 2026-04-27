using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly ICategoryService _categoryService;
        private readonly IDestinationService _destinationService;
        private readonly ICurrentUserService _currentUserService;

        public BlogController(
            IBlogService blogService,
            ICategoryService categoryService,
            IDestinationService destinationService,
            ICurrentUserService currentUserService)
        {
            _blogService = blogService;
            _categoryService = categoryService;
            _destinationService = destinationService;
            _currentUserService = currentUserService;
        }

        // GET: Blog
        public async Task<IActionResult> Index(int page = 1)
        {
            var posts = await _blogService.GetPublishedPostsAsync(page, 12);
            var categories = await _categoryService.GetAllAsync();
            
            ViewBag.Categories = categories;
            ViewBag.CurrentPage = page;
            return View(posts);
        }

        // GET: Blog/Post/slug
        public async Task<IActionResult> Post(string slug)
        {
            var post = await _blogService.GetBySlugAsync(slug);
            if (post == null || !post.IsPublished) return NotFound();

            await _blogService.IncrementViewCountAsync(post.Id);
            return View(post);
        }

        // GET: Blog/Category/5
        public async Task<IActionResult> Category(int id)
        {
            var posts = await _blogService.GetPostsByCategoryAsync(id);
            var category = await _categoryService.GetByIdAsync(id);
            
            ViewBag.Category = category;
            return View("Index", posts);
        }

        // GET: Blog/Destination/5
        public async Task<IActionResult> Destination(int id)
        {
            var posts = await _blogService.GetPostsByDestinationAsync(id);
            var destination = await _destinationService.GetByIdAsync(id);
            
            ViewBag.Destination = destination;
            return View("Index", posts);
        }

        // GET: Blog/Search
        public async Task<IActionResult> Search(string q)
        {
            var posts = await _blogService.SearchPostsAsync(q);
            ViewBag.Keyword = q;
            return View("Index", posts);
        }

        // GET: Blog/Create (Admin/Manager/Hiring)
        public async Task<IActionResult> Create()
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || !user.IsStaffOrAdmin()) return Forbid();

            ViewBag.Categories = await _categoryService.GetAllAsync();
            ViewBag.Destinations = await _destinationService.GetAllAsync();
            return View();
        }

        // POST: Blog/Create
        [HttpPost]
        public async Task<IActionResult> Create(BlogPost post, IFormFile featuredImage)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || !user.IsStaffOrAdmin()) return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllAsync();
                ViewBag.Destinations = await _destinationService.GetAllAsync();
                return View(post);
            }

            // Handle image upload (implement IImageStorageService)
            if (featuredImage != null)
            {
                // post.FeaturedImage = await _imageStorageService.UploadAsync(featuredImage);
            }

            post.AuthorId = user.Id;
            post.Slug = GenerateSlug(post.Title);
            
            await _blogService.CreatePostAsync(post);
            TempData["Success"] = "Tạo bài viết thành công!";
            return RedirectToAction("Index");
        }

        // GET: Blog/Edit/5 (Admin/Manager/Hiring)
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || !user.IsStaffOrAdmin()) return Forbid();

            var post = await _blogService.GetByIdAsync(id);
            if (post == null) return NotFound();

            // Only author or admin can edit
            if (post.AuthorId != user.Id && !user.IsAdmin()) return Forbid();

            ViewBag.Categories = await _categoryService.GetAllAsync();
            ViewBag.Destinations = await _destinationService.GetAllAsync();
            return View(post);
        }

        // POST: Blog/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, BlogPost post, IFormFile featuredImage)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || !user.IsStaffOrAdmin()) return Forbid();

            var existingPost = await _blogService.GetByIdAsync(id);
            if (existingPost == null) return NotFound();

            // Only author or admin can edit
            if (existingPost.AuthorId != user.Id && !user.IsAdmin()) return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllAsync();
                ViewBag.Destinations = await _destinationService.GetAllAsync();
                return View(post);
            }

            if (featuredImage != null)
            {
                // post.FeaturedImage = await _imageStorageService.UploadAsync(featuredImage);
            }

            post.Id = id;
            post.Slug = GenerateSlug(post.Title);
            
            await _blogService.UpdatePostAsync(post);
            TempData["Success"] = "Cập nhật bài viết thành công!";
            return RedirectToAction("Post", new { slug = post.Slug });
        }

        // POST: Blog/Publish/5 (Admin/Manager)
        [HttpPost]
        public async Task<IActionResult> Publish(int id)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || (!user.IsAdmin() && !user.IsManager())) return Forbid();

            await _blogService.PublishPostAsync(id);
            TempData["Success"] = "Xuất bản bài viết thành công!";
            return RedirectToAction("Index");
        }

        // POST: Blog/Delete/5 (Admin only)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null || !user.IsAdmin()) return Forbid();

            await _blogService.DeletePostAsync(id);
            TempData["Success"] = "Xóa bài viết thành công!";
            return RedirectToAction("Index");
        }

        private string GenerateSlug(string title)
        {
            // Simple slug generation - you can use a library like Slugify
            return title.ToLower()
                .Replace(" ", "-")
                .Replace("đ", "d")
                .Replace("á", "a")
                .Replace("à", "a")
                .Replace("ả", "a")
                .Replace("ã", "a")
                .Replace("ạ", "a");
        }
    }
}

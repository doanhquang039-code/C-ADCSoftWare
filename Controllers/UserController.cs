using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

public class UserController : Controller
{
    private readonly ApplicationDbContext db;
    private readonly ICurrentUserService currentUserService;

    public UserController(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        db = context;
        this.currentUserService = currentUserService;
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(User user)
    {
        if (db.Users.Any(u => u.Email == user.Email))
        {
            ModelState.AddModelError("Email", "Email đã được sử dụng!");
        }

        if (!ModelState.IsValid)
        {
            return View(user);
        }

        db.Users.Add(user);
        db.SaveChanges();
        return RedirectToAction(nameof(Login));
    }

    [AuthenticatedOnly]
    public IActionResult Logout()
    {
        currentUserService.SignOut();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        var user = db.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
        if (user == null)
        {
            ViewBag.Error = "Email hoặc mật khẩu không đúng!";
            return View();
        }

        currentUserService.SignIn(user);
        return RedirectToAction("Index", "Home");
    }

    [AuthenticatedOnly]
    public IActionResult Edit(int id)
    {
        var user = db.Users.Find(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    [AuthenticatedOnly]
    public IActionResult Edit(User user)
    {
        if (!ModelState.IsValid)
        {
            return View(user);
        }

        var existingUser = db.Users.Find(user.Id);
        if (existingUser == null)
        {
            return NotFound();
        }

        existingUser.Name = user.Name;
        existingUser.Email = user.Email;
        existingUser.Password = user.Password;
        existingUser.Gender = user.Gender;
        existingUser.Age = user.Age;

        db.Users.Update(existingUser);
        db.SaveChanges();

        currentUserService.SignIn(existingUser);
        return RedirectToAction("Index", "Home");
    }

    [AuthenticatedOnly]
    public IActionResult Details(int? id)
    {
        var currentUser = currentUserService.GetCurrentUser();
        var targetId = id ?? currentUser?.Id;
        if (targetId == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var user = db.Users.Find(targetId.Value);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }
}

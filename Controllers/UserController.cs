using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

public class UserController : Controller
{
    private readonly ApplicationDbContext db;

    public UserController(ApplicationDbContext context)
    {
        db = context;
    }

    // GET: /User/Create
    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    public IActionResult Create(User user)
    {
        if (!ModelState.IsValid)
        {

            var exists = db.Users.Any(u => u.Email == user.Email);
            if (exists)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng!");
                return View(user);
            }

            db.Users.Add(user);
            db.SaveChanges();

            return RedirectToAction("Login", "User");
        }

        return View(user);
    }
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("userLogin");
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
        if (user != null)
        {
            HttpContext.Session.SetObject("userLogin", user);      
            HttpContext.Session.SetInt32("userId", user.Id);      

            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Email hoặc mật khẩu không đúng!";
        return View();
    }
    public IActionResult Edit(int id)
    {
        var user = db.Users.Find(id);
        return View();
    }
    [HttpPost]
    public IActionResult Edit(User user)
    {
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

        return RedirectToAction("Index");
    }



    public IActionResult Details(int id)
    {
        var user = db.Users.Find(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

}
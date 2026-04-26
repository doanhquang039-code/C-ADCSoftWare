using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

public class UserController : Controller
{
    private readonly IUserService userService;
    private readonly ICurrentUserService currentUserService;

    public UserController(IUserService userService, ICurrentUserService currentUserService)
    {
        this.userService = userService;
        this.currentUserService = currentUserService;
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        if (!ModelState.IsValid)
        {
            return View(user);
        }

        var success = await userService.RegisterAsync(user);
        if (!success)
        {
            ModelState.AddModelError("Email", "Email đã được sử dụng!");
            return View(user);
        }

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
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await userService.AuthenticateAsync(email, password);
        if (user == null)
        {
            ViewBag.Error = "Email hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa!";
            return View();
        }

        currentUserService.SignIn(user);
        return RedirectToAction("Index", "Home");
    }

    [AuthenticatedOnly]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        return View(user);
    }

    [HttpPost]
    [AuthenticatedOnly]
    public async Task<IActionResult> Edit(User user)
    {
        if (!ModelState.IsValid)
        {
            return View(user);
        }

        await userService.UpdateAsync(user);

        // Re-fetch user from DB and re-sign in to update session
        var updatedUser = await userService.GetByIdAsync(user.Id);
        if (updatedUser != null)
        {
            currentUserService.SignIn(updatedUser);
        }

        return RedirectToAction("Index", "Home");
    }

    [AuthenticatedOnly]
    public async Task<IActionResult> Details(int? id)
    {
        var currentUser = currentUserService.GetCurrentUser();
        var targetId = id ?? currentUser?.Id;
        if (targetId == null) return RedirectToAction(nameof(Login));

        var user = await userService.GetByIdAsync(targetId.Value);
        if (user == null) return NotFound();

        return View(user);
    }
}

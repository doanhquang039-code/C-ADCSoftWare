using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Helpers;
using WEBDULICH.Models;
using WEBDULICH.Services;

public class UserController : Controller
{
    private readonly IUserService userService;
    private readonly ICurrentUserService currentUserService;
    private readonly ISecurityService _securityService;

    public UserController(IUserService userService, ICurrentUserService currentUserService, ISecurityService securityService)
    {
        this.userService = userService;
        this.currentUserService = currentUserService;
        _securityService = securityService;
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

    [AuthenticatedOnly]
    public async Task<IActionResult> Dashboard()
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser == null) return RedirectToAction(nameof(Login));

        var user = await userService.GetByIdAsync(currentUser.Id);
        return View(user);
    }

    [AuthenticatedOnly]
    [HttpPost]
    public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser == null) return RedirectToAction(nameof(Login));

        var user = await userService.GetByIdAsync(currentUser.Id);
        if (user == null) return NotFound();

        // Kiểm tra mật khẩu cũ
        bool isCorrect = false;
        if (user.Password.Contains(":"))
        {
            isCorrect = _securityService.VerifyPassword(oldPassword, user.Password);
        }
        else
        {
            isCorrect = (user.Password == oldPassword);
        }

        if (!isCorrect)
        {
            TempData["Error"] = "Mật khẩu cũ không chính xác!";
            return RedirectToAction(nameof(Dashboard));
        }

        // Cập nhật mật khẩu mới
        user.Password = _securityService.HashPassword(newPassword);
        await userService.UpdateAsync(user);

        TempData["Success"] = "Đổi mật khẩu thành công!";
        return RedirectToAction(nameof(Dashboard));
    }
}

using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Services;
using WEBDULICH.Models;

namespace WEBDULICH.Controllers
{
    public class SettingsController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILocalizationService _localizationService;
        private readonly ISecurityService _securityService;
        private readonly ICurrentUserService _currentUserService;

        public SettingsController(
            IUserService userService,
            ILocalizationService localizationService,
            ISecurityService securityService,
            ICurrentUserService currentUserService)
        {
            _userService = userService;
            _localizationService = localizationService;
            _securityService = securityService;
            _currentUserService = currentUserService;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.SupportedLanguages = _localizationService.GetSupportedCultures();
            ViewBag.CurrentLanguage = currentUser.PreferredLanguage;
            
            return View(currentUser);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLanguage(string language)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            if (_localizationService.GetSupportedCultures().Contains(language))
            {
                currentUser.PreferredLanguage = language;
                await _userService.UpdateUserAsync(currentUser);
                _localizationService.SetCulture(language);

                return Json(new { success = true, message = "Language updated successfully" });
            }

            return Json(new { success = false, message = "Unsupported language" });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Verify current password
            if (!_securityService.VerifyPassword(currentPassword, currentUser.Password))
            {
                return Json(new { success = false, message = "Current password is incorrect" });
            }

            // Validate new password
            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "New passwords do not match" });
            }

            if (!_securityService.ValidatePasswordStrength(newPassword))
            {
                return Json(new { success = false, message = "Password must be at least 8 characters with uppercase, lowercase, number and special character" });
            }

            // Update password
            currentUser.Password = _securityService.HashPassword(newPassword);
            currentUser.UpdatedAt = DateTime.Now;
            await _userService.UpdateUserAsync(currentUser);

            // Log security event
            await _securityService.LogSecurityEventAsync(
                "PasswordChanged", 
                currentUser.Id, 
                "User changed password", 
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            );

            return Json(new { success = true, message = "Password changed successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(User model)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Update allowed fields
            currentUser.Name = model.Name;
            currentUser.PhoneNumber = model.PhoneNumber;
            currentUser.Address = model.Address;
            currentUser.City = model.City;
            currentUser.Country = model.Country;
            currentUser.DateOfBirth = model.DateOfBirth;
            currentUser.Gender = model.Gender;
            currentUser.Occupation = model.Occupation;
            currentUser.Company = model.Company;
            currentUser.UpdatedAt = DateTime.Now;

            await _userService.UpdateUserAsync(currentUser);

            return Json(new { success = true, message = "Profile updated successfully" });
        }

        public async Task<IActionResult> Security()
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(currentUser);
        }

        [HttpPost]
        public async Task<IActionResult> EnableTwoFactor()
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var code = await _securityService.GenerateTwoFactorCodeAsync(currentUser.Id);
            
            // In a real application, you would send this code via SMS or email
            // For demo purposes, we'll return it in the response
            return Json(new { 
                success = true, 
                message = "Two-factor authentication code generated", 
                code = code // Remove this in production
            });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyTwoFactor(string code)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var isValid = await _securityService.ValidateTwoFactorCodeAsync(currentUser.Id, code);
            
            if (isValid)
            {
                await _securityService.LogSecurityEventAsync(
                    "TwoFactorEnabled", 
                    currentUser.Id, 
                    "Two-factor authentication enabled", 
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
                );

                return Json(new { success = true, message = "Two-factor authentication enabled successfully" });
            }

            return Json(new { success = false, message = "Invalid verification code" });
        }

        public async Task<IActionResult> GetSecurityLogs()
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // This would typically be restricted to admin users
            if (!currentUser.IsStaffOrAdmin())
            {
                return Json(new { success = false, message = "Access denied" });
            }

            // Return recent security logs for the user
            return Json(new { success = true, logs = "Security logs would be displayed here" });
        }
    }
}
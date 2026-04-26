using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WEBDULICH.Services;

namespace WEBDULICH.Helpers
{
    /// <summary>
    /// Cho phép Staff hoặc Admin truy cập
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class StaffOrAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var currentUserService = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
            if (!currentUserService.IsAuthenticated())
            {
                context.Result = new RedirectToActionResult("Login", "User", null);
                return;
            }

            if (!currentUserService.IsStaffOrAdmin())
            {
                context.Result = new RedirectToActionResult("Index", "Home", new { area = "" });
            }
        }
    }
}

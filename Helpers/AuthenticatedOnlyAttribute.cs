using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WEBDULICH.Services;

namespace WEBDULICH.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthenticatedOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var currentUserService = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
            if (!currentUserService.IsAuthenticated())
            {
                context.Result = new RedirectToActionResult("Login", "User", null);
            }
        }
    }
}

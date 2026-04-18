using Microsoft.Extensions.Options;
using WEBDULICH.Helpers;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly AdminAccessOptions adminAccessOptions;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, IOptions<AdminAccessOptions> adminAccessOptions)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.adminAccessOptions = adminAccessOptions.Value;
        }

        public User? GetCurrentUser()
        {
            return httpContextAccessor.HttpContext?.Session.GetObject<User>("userLogin");
        }

        public bool IsAuthenticated()
        {
            return GetCurrentUser() != null;
        }

        public bool IsAdmin()
        {
            var user = GetCurrentUser();
            if (user?.Email == null)
            {
                return false;
            }

            return adminAccessOptions.Emails.Any(email =>
                string.Equals(email, user.Email, StringComparison.OrdinalIgnoreCase));
        }

        public void SignIn(User user)
        {
            var session = httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                return;
            }

            session.SetObject("userLogin", user);
            session.SetInt32("userId", user.Id);
        }

        public void SignOut()
        {
            var session = httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                return;
            }

            session.Remove("userLogin");
            session.Remove("userId");
        }
    }
}

using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface ICurrentUserService
    {
        User? GetCurrentUser();
        bool IsAuthenticated();
        bool IsAdmin();
        void SignIn(User user);
        void SignOut();
    }
}

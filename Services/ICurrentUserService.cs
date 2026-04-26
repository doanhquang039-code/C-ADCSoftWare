using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface ICurrentUserService
    {
        User? GetCurrentUser();
        bool IsAuthenticated();
        bool IsAdmin();
        bool IsStaff();
        bool IsStaffOrAdmin();
        bool HasRole(string role);
        void SignIn(User user);
        void SignOut();
    }
}

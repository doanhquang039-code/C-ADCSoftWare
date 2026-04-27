using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface ICurrentUserService
    {
        User? GetCurrentUser();
        Task<User?> GetCurrentUserAsync();
        bool IsAuthenticated();
        bool IsAdmin();
        bool IsManager();
        bool IsHiring();
        bool IsStaff();
        bool IsStaffOrAdmin();
        bool HasRole(string role);
        void SignIn(User user);
        void SignOut();
    }
}

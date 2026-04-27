using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface IWishlistService
    {
        Task<bool> AddToWishlistAsync(int userId, string itemType, int itemId);
        Task<bool> RemoveFromWishlistAsync(int userId, string itemType, int itemId);
        Task<bool> IsInWishlistAsync(int userId, string itemType, int itemId);
        Task<List<Wishlist>> GetUserWishlistAsync(int userId);
        Task<List<Tour>> GetWishlistToursAsync(int userId);
        Task<List<Hotel>> GetWishlistHotelsAsync(int userId);
    }
}

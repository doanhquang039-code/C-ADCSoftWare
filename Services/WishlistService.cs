using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly ApplicationDbContext _context;

        public WishlistService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddToWishlistAsync(int userId, string itemType, int itemId)
        {
            var exists = await IsInWishlistAsync(userId, itemType, itemId);
            if (exists) return false;

            var wishlist = new Wishlist
            {
                UserId = userId,
                ItemType = itemType,
                ItemId = itemId,
                CreatedAt = DateTime.Now
            };

            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromWishlistAsync(int userId, string itemType, int itemId)
        {
            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ItemType == itemType && w.ItemId == itemId);

            if (wishlist == null) return false;

            _context.Wishlists.Remove(wishlist);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsInWishlistAsync(int userId, string itemType, int itemId)
        {
            return await _context.Wishlists
                .AnyAsync(w => w.UserId == userId && w.ItemType == itemType && w.ItemId == itemId);
        }

        public async Task<List<Wishlist>> GetUserWishlistAsync(int userId)
        {
            return await _context.Wishlists
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Tour>> GetWishlistToursAsync(int userId)
        {
            var tourIds = await _context.Wishlists
                .Where(w => w.UserId == userId && w.ItemType == "Tour")
                .Select(w => w.ItemId)
                .ToListAsync();

            return await _context.Tours
                .Where(t => tourIds.Contains(t.Id))
                .Include(t => t.Destination)
                .ToListAsync();
        }

        public async Task<List<Hotel>> GetWishlistHotelsAsync(int userId)
        {
            var hotelIds = await _context.Wishlists
                .Where(w => w.UserId == userId && w.ItemType == "Hotel")
                .Select(w => w.ItemId)
                .ToListAsync();

            return await _context.Hotels
                .Where(h => hotelIds.Contains(h.Id))
                .ToListAsync();
        }
    }
}

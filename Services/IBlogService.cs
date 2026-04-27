using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface IBlogService
    {
        Task<BlogPost> GetByIdAsync(int id);
        Task<BlogPost> GetBySlugAsync(string slug);
        Task<List<BlogPost>> GetPublishedPostsAsync(int page = 1, int pageSize = 10);
        Task<List<BlogPost>> GetPostsByCategoryAsync(int categoryId);
        Task<List<BlogPost>> GetPostsByDestinationAsync(int destinationId);
        Task<List<BlogPost>> SearchPostsAsync(string keyword);
        Task<BlogPost> CreatePostAsync(BlogPost post);
        Task<bool> UpdatePostAsync(BlogPost post);
        Task<bool> DeletePostAsync(int id);
        Task<bool> PublishPostAsync(int id);
        Task<bool> UnpublishPostAsync(int id);
        Task<bool> IncrementViewCountAsync(int id);
    }
}

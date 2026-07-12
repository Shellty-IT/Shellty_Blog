using Shellty_Blog.Models;

namespace Shellty_Blog.Services
{
    public interface IBlogPostService
    {
        Task<PagedResult<BlogPost>> GetPostsAsync(BlogPostQuery query);
        Task<List<BlogPost>> GetRecentPostsAsync(int count);
        Task<int> GetPostCountAsync();
        Task<BlogPost?> GetByIdAsync(int id);
        Task<List<string>> GetCategoriesAsync();
        Task CreateAsync(BlogPost post);
        Task UpdateAsync(BlogPost post);
        Task<bool> DeleteAsync(int id);
        Task<(byte[]? Data, string? ContentType)> GetImageAsync(int id);
        string? ValidateImage(IFormFile file);
        Task ApplyImageAsync(BlogPost post, IFormFile file);
        void RemoveImage(BlogPost post);
    }
}

using Microsoft.EntityFrameworkCore;
using Shellty_Blog.Data;
using Shellty_Blog.Models;

namespace Shellty_Blog.Services
{
    public class BlogPostService : IBlogPostService
    {
        private readonly BlogContext _context;
        private readonly long _maxFileSize = 5 * 1024 * 1024;
        private readonly string[] _allowedTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };

        public BlogPostService(BlogContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<BlogPost>> GetPostsAsync(BlogPostQuery options)
        {
            var query = _context.BlogPosts.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(options.SearchTerm))
            {
                var searchTerm = options.SearchTerm.Trim().ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(searchTerm) ||
                    p.Content.ToLower().Contains(searchTerm) ||
                    (p.Category != null && p.Category.ToLower().Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(options.Category))
            {
                query = query.Where(p => p.Category == options.Category);
            }

            query = options.Sort switch
            {
                "oldest" => query.OrderBy(p => p.CreatedDate),
                "title" => query.OrderBy(p => p.Title),
                _ => query.OrderByDescending(p => p.CreatedDate)
            };

            var pageSize = Math.Clamp(options.PageSize, 1, 24);
            var totalCount = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
            var currentPage = Math.Clamp(options.Page, 1, totalPages);

            var posts = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new BlogPost
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    Category = p.Category,
                    CreatedDate = p.CreatedDate,
                    ModifiedDate = p.ModifiedDate,
                    ImageFileName = p.ImageFileName,
                    ImageContentType = p.ImageContentType
                })
                .ToListAsync();

            return new PagedResult<BlogPost>(posts, totalCount, currentPage, pageSize);
        }

        public async Task<List<BlogPost>> GetRecentPostsAsync(int count)
        {
            return await _context.BlogPosts
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedDate)
                .Take(Math.Clamp(count, 1, 12))
                .Select(p => new BlogPost
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    Category = p.Category,
                    CreatedDate = p.CreatedDate,
                    ModifiedDate = p.ModifiedDate,
                    ImageFileName = p.ImageFileName,
                    ImageContentType = p.ImageContentType
                })
                .ToListAsync();
        }

        public Task<int> GetPostCountAsync()
        {
            return _context.BlogPosts.AsNoTracking().CountAsync();
        }

        public async Task<BlogPost?> GetByIdAsync(int id)
        {
            return await _context.BlogPosts.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.BlogPosts
                .AsNoTracking()
                .Where(p => !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task CreateAsync(BlogPost post)
        {
            post.CreatedDate = DateTime.UtcNow;
            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BlogPost post)
        {
            post.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return false;

            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(byte[]? Data, string? ContentType)> GetImageAsync(int id)
        {
            var post = await _context.BlogPosts
                .Where(p => p.Id == id)
                .Select(p => new { p.ImageData, p.ImageContentType })
                .FirstOrDefaultAsync();

            return (post?.ImageData, post?.ImageContentType);
        }

        public string? ValidateImage(IFormFile file)
        {
            if (file.Length > _maxFileSize)
                return "File size cannot exceed 5 MB.";

            if (!_allowedTypes.Contains(file.ContentType.ToLower()))
                return "Only JPEG, PNG, GIF and WebP files are allowed.";

            return null;
        }

        public async Task ApplyImageAsync(BlogPost post, IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            post.ImageData = memoryStream.ToArray();
            post.ImageContentType = file.ContentType;
            post.ImageFileName = file.FileName;
        }

        public void RemoveImage(BlogPost post)
        {
            post.ImageData = null;
            post.ImageContentType = null;
            post.ImageFileName = null;
        }
    }
}

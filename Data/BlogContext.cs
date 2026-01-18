using Microsoft.EntityFrameworkCore;
using PurrfectBlog.Models;

namespace PurrfectBlog.Data
{
    public class BlogContext : DbContext
    {
        public BlogContext(DbContextOptions<BlogContext> options)
            : base(options)
        {
        }
        
        public DbSet<BlogPost> BlogPosts { get; set; }
    }
}
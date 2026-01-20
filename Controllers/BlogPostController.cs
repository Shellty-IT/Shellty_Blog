using Microsoft.AspNetCore.Mvc;
using PurrfectBlog.Data;
using PurrfectBlog.Models;
using Microsoft.EntityFrameworkCore;

namespace PurrfectBlog.Controllers
{
    public class BlogPostController : Controller
    {
        private readonly BlogContext _context;

        public BlogPostController(BlogContext context)
        {
            _context = context;
        }

        // GET: BlogPost/CreatePost
        public IActionResult CreatePost()
        {
            return View();
        }
        
        // POST: BlogPost/CreatePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(BlogPost blogPost)
        {
            if (ModelState.IsValid)
            {
                blogPost.CreatedDate = DateTime.Now;
                _context.BlogPosts.Add(blogPost);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return View(blogPost);
        }
        public async Task<IActionResult> Posts()
        {
            var posts = await _context.BlogPosts
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
    
            return View(posts);
        }
        
        public async Task<IActionResult> Post(int id)
        {
            var post = await _context.BlogPosts
                .FirstOrDefaultAsync(p => p.Id == id);
    
            if (post == null)
            {
                return NotFound();
            }
    
            return View(post);
        }
    }
}
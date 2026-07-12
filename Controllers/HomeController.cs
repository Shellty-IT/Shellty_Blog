using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shellty_Blog.Models;
using Shellty_Blog.Models.ViewModels;
using Shellty_Blog.Services;

namespace Shellty_Blog.Controllers;

public class HomeController : Controller
{
    private readonly IBlogPostService _blogPostService;

    public HomeController(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    public async Task<IActionResult> Index()
    {
        var posts = await _blogPostService.GetRecentPostsAsync(3);
        var totalPostCount = await _blogPostService.GetPostCountAsync();
        var categories = await _blogPostService.GetCategoriesAsync();

        var model = new HomeViewModel
        {
            RecentPosts = posts.Select(post => PostListItemViewModel.FromPost(post, 140)).ToList(),
            TotalPostCount = totalPostCount,
            CategoryCount = categories.Count
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

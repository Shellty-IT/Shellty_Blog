using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shellty_Blog.Controllers;
using Shellty_Blog.Data;
using Shellty_Blog.Models;
using Shellty_Blog.Services;

namespace Shellty_Blog.Tests;

public class SeoTests
{
    private static BlogContext CreateContext() => new(
        new DbContextOptionsBuilder<BlogContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Sitemap_ContainsPublicPostsAndRealModificationDates()
    {
        await using var db = CreateContext();
        db.BlogPosts.Add(new BlogPost
        {
            Id = 42, Title = "Existing post", Content = "Existing content",
            CreatedDate = new DateTime(2026, 5, 1), ModifiedDate = new DateTime(2026, 5, 3),
        });
        await db.SaveChangesAsync();
        var result = Assert.IsType<ContentResult>(await new SeoController(db).Sitemap(default));
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urls = XDocument.Parse(result.Content!).Root!.Elements(ns + "url").ToList();
        Assert.Equal(4, urls.Count);
        Assert.All(urls, url => Assert.StartsWith("https://blog.shellty.pl/", url.Element(ns + "loc")!.Value));
        var post = urls.Single(url => url.Element(ns + "loc")!.Value.EndsWith("/BlogPost/Post/42"));
        Assert.Equal("2026-05-03", post.Element(ns + "lastmod")!.Value);
        Assert.DoesNotContain(urls, url => url.Element(ns + "loc")!.Value.Contains("Account"));
    }

    [Fact]
    public void Robots_AllowsPublicPagesAndDeclaresSitemap()
    {
        using var db = CreateContext();
        var result = Assert.IsType<ContentResult>(new SeoController(db).Robots());
        Assert.Contains("Allow: /", result.Content);
        Assert.DoesNotContain("Disallow: /\n", result.Content);
        Assert.Contains("Sitemap: https://blog.shellty.pl/sitemap.xml", result.Content);
    }

    [Fact]
    public async Task MissingPost_KeepsExistingViewButReturns404()
    {
        await using var db = CreateContext();
        var controller = new BlogPostController(new BlogPostService(db))
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
        var result = Assert.IsType<ViewResult>(await controller.Post(999));
        Assert.Equal("PostNotFound", result.ViewName);
        Assert.Equal(404, controller.Response.StatusCode);
    }
}

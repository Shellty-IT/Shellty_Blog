using System.Globalization;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shellty_Blog.Data;

namespace Shellty_Blog.Controllers;

public class SeoController(BlogContext db) : Controller
{
    private const string SiteUrl = "https://blog.shellty.pl";

    [HttpGet("/robots.txt")]
    public IActionResult Robots() => Content(
        $"User-agent: *\nAllow: /\nDisallow: /health\n\nSitemap: {SiteUrl}/sitemap.xml\n",
        "text/plain; charset=utf-8");

    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> Sitemap(CancellationToken cancellationToken)
    {
        var posts = await db.BlogPosts.AsNoTracking()
            .OrderBy(post => post.Id)
            .Select(post => new { post.Id, post.CreatedDate, post.ModifiedDate })
            .ToListAsync(cancellationToken);

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urls = new List<XElement>
        {
            new(ns + "url", new XElement(ns + "loc", SiteUrl + "/")),
            new(ns + "url", new XElement(ns + "loc", SiteUrl + "/BlogPost/Posts")),
            new(ns + "url", new XElement(ns + "loc", SiteUrl + "/Home/Privacy")),
        };

        foreach (var post in posts)
        {
            var url = new XElement(ns + "url",
                new XElement(ns + "loc", $"{SiteUrl}/BlogPost/Post/{post.Id}"));
            var modified = post.ModifiedDate ?? post.CreatedDate;
            if (modified != default)
                url.Add(new XElement(ns + "lastmod", modified.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
            urls.Add(url);
        }

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null), new XElement(ns + "urlset", urls));
        return Content(document.ToString(), "application/xml; charset=utf-8");
    }
}

using Microsoft.EntityFrameworkCore;
using Shellty_Blog.Data;
using Shellty_Blog.Models;
using Shellty_Blog.Services;

namespace Shellty_Blog.Tests;

public class BlogPostServiceTests
{
    [Fact]
    public async Task GetPostsAsync_FiltersSortsAndPaginatesPosts()
    {
        await using var context = CreateContext();
        context.BlogPosts.AddRange(
            CreatePost("Zebra shell", "A shell field guide", "Guides", 1),
            CreatePost("Amber shell", "Another shell guide", "Guides", 2),
            CreatePost("Ocean notes", "A story about waves", "Stories", 3),
            CreatePost("Blue shell", "Shell care basics", "Guides", 4));
        await context.SaveChangesAsync();
        var service = new BlogPostService(context);

        var result = await service.GetPostsAsync(
            new BlogPostQuery("shell", "Guides", "title", Page: 1, PageSize: 2));

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(["Amber shell", "Blue shell"], result.Items.Select(post => post.Title));
        Assert.All(result.Items, post => Assert.Null(post.ImageData));
    }

    [Fact]
    public async Task GetPostsAsync_ClampsPageToAvailableRange()
    {
        await using var context = CreateContext();
        context.BlogPosts.AddRange(
            CreatePost("First", "Content", "News", 1),
            CreatePost("Second", "Content", "News", 2),
            CreatePost("Third", "Content", "News", 3));
        await context.SaveChangesAsync();
        var service = new BlogPostService(context);

        var result = await service.GetPostsAsync(
            new BlogPostQuery(Page: 99, PageSize: 2));

        Assert.Equal(2, result.CurrentPage);
        Assert.Single(result.Items);
        Assert.Equal("First", result.Items[0].Title);
    }

    [Fact]
    public async Task GetRecentPostsAsync_ReturnsNewestPostsWithoutImageData()
    {
        await using var context = CreateContext();
        var oldPost = CreatePost("Old", "Content", "News", 1);
        oldPost.ImageData = [1, 2, 3];
        oldPost.ImageContentType = "image/png";
        context.BlogPosts.AddRange(
            oldPost,
            CreatePost("New", "Content", "News", 2));
        await context.SaveChangesAsync();
        var service = new BlogPostService(context);

        var result = await service.GetRecentPostsAsync(1);

        var post = Assert.Single(result);
        Assert.Equal("New", post.Title);
        Assert.Null(post.ImageData);
    }

    private static BlogContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BlogContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BlogContext(options);
    }

    private static BlogPost CreatePost(string title, string content, string category, int day)
    {
        return new BlogPost
        {
            Title = title,
            Content = content,
            Category = category,
            CreatedDate = new DateTime(2026, 7, day, 12, 0, 0, DateTimeKind.Utc)
        };
    }
}

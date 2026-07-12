using Shellty_Blog.Models;
using Shellty_Blog.Models.ViewModels;

namespace Shellty_Blog.Tests;

public class PostListItemViewModelTests
{
    [Fact]
    public void FromPost_CreatesReadableSummary()
    {
        var content = string.Join(' ', Enumerable.Repeat("shell", 210));
        var post = new BlogPost
        {
            Id = 7,
            Title = "Shell guide",
            Content = content,
            Category = "Guides",
            CreatedDate = new DateTime(2026, 7, 13, 10, 0, 0, DateTimeKind.Utc),
            ImageContentType = "image/webp"
        };

        var result = PostListItemViewModel.FromPost(post, 40);

        Assert.Equal(7, result.Id);
        Assert.Equal("Shell guide", result.Title);
        Assert.Equal("Guides", result.Category);
        Assert.True(result.HasImage);
        Assert.Equal(2, result.ReadingTimeMinutes);
        Assert.EndsWith("…", result.Excerpt);
        Assert.True(result.Excerpt.Length <= 41);
    }

    [Fact]
    public void FromPost_NormalizesWhitespaceAndUsesOneMinuteMinimum()
    {
        var post = new BlogPost
        {
            Content = "A short\n\npost\twith spacing."
        };

        var result = PostListItemViewModel.FromPost(post);

        Assert.Equal("A short post with spacing.", result.Excerpt);
        Assert.Equal(1, result.ReadingTimeMinutes);
        Assert.False(result.HasImage);
    }
}

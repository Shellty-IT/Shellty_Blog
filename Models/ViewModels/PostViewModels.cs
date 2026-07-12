namespace Shellty_Blog.Models.ViewModels;

public sealed class PostListItemViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Excerpt { get; init; } = string.Empty;
    public string? Category { get; init; }
    public DateTime CreatedDate { get; init; }
    public bool HasImage { get; init; }
    public int ReadingTimeMinutes { get; init; }

    public static PostListItemViewModel FromPost(BlogPost post, int excerptLength = 180)
    {
        var normalizedContent = string.Join(
            " ",
            post.Content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        var excerpt = normalizedContent.Length > excerptLength
            ? $"{normalizedContent[..excerptLength].TrimEnd()}…"
            : normalizedContent;

        var wordCount = post.Content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

        return new PostListItemViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Excerpt = excerpt,
            Category = post.Category,
            CreatedDate = post.CreatedDate,
            HasImage = !string.IsNullOrEmpty(post.ImageContentType),
            ReadingTimeMinutes = Math.Max(1, (int)Math.Ceiling(wordCount / 200d))
        };
    }
}

public sealed class PostsViewModel
{
    public IReadOnlyList<PostListItemViewModel> Posts { get; init; } = [];
    public IReadOnlyList<string> Categories { get; init; } = [];
    public string? SearchTerm { get; init; }
    public string? Category { get; init; }
    public string Sort { get; init; } = "newest";
    public int CurrentPage { get; init; } = 1;
    public int TotalPages { get; init; } = 1;
    public int TotalCount { get; init; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasActiveFilters => !string.IsNullOrWhiteSpace(SearchTerm) || !string.IsNullOrWhiteSpace(Category);
}

public sealed class HomeViewModel
{
    public IReadOnlyList<PostListItemViewModel> RecentPosts { get; init; } = [];
    public int TotalPostCount { get; init; }
    public int CategoryCount { get; init; }
}

public sealed class PostDetailsViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? Category { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? ModifiedDate { get; init; }
    public bool HasImage { get; init; }
    public int ReadingTimeMinutes { get; init; }

    public static PostDetailsViewModel FromPost(BlogPost post)
    {
        var wordCount = post.Content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

        return new PostDetailsViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Category = post.Category,
            CreatedDate = post.CreatedDate,
            ModifiedDate = post.ModifiedDate,
            HasImage = !string.IsNullOrWhiteSpace(post.ImageContentType),
            ReadingTimeMinutes = Math.Max(1, (int)Math.Ceiling(wordCount / 200d))
        };
    }
}

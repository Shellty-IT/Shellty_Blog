namespace Shellty_Blog.Models;

public sealed record BlogPostQuery(
    string? SearchTerm = null,
    string? Category = null,
    string Sort = "newest",
    int Page = 1,
    int PageSize = 6);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int CurrentPage,
    int PageSize)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

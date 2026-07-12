using Shellty_Blog.Models;

namespace Shellty_Blog.Tests;

public class PagedResultTests
{
    [Theory]
    [InlineData(0, 6, 1)]
    [InlineData(1, 6, 1)]
    [InlineData(6, 6, 1)]
    [InlineData(7, 6, 2)]
    [InlineData(18, 6, 3)]
    public void TotalPages_ReturnsExpectedValue(int totalCount, int pageSize, int expected)
    {
        var result = new PagedResult<int>([], totalCount, 1, pageSize);

        Assert.Equal(expected, result.TotalPages);
    }
}

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace Shellty_Blog.Models;

[SuppressMessage("Usage", "CA1002:DoNotExposeGenericLists")]
public class ApplicationUser : IdentityUser
{
    [StringLength(100)]
    public string DisplayName { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public ICollection<AdminRequest> AdminRequests { get; init; } = [];

    public ICollection<AdminApproval> AdminApprovals { get; init; } = [];
}
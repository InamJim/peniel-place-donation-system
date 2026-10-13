using Microsoft.AspNetCore.Identity;

namespace PenielPlace.Web.Models;

public sealed class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}

using Microsoft.AspNetCore.Identity;
using PenielPlace.Web.Models;

namespace PenielPlace.Web.Security;

public static class IdentitySeed
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in new[] { "Volunteer", "Leadership" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException($"Could not create role {roleName}: {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        // Configure BootstrapAdmin:Email and BootstrapAdmin:Password using environment
        // variables or a secret store. Never commit a bootstrap password to appsettings.json.
        var email = configuration["BootstrapAdmin:Email"]?.Trim();
        var password = configuration["BootstrapAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = "Peniel Place Leadership",
                LockoutEnabled = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException("Could not create bootstrap leadership user: " +
                    string.Join("; ", createResult.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, "Leadership"))
        {
            var roleResult = await userManager.AddToRoleAsync(user, "Leadership");
            if (!roleResult.Succeeded)
                throw new InvalidOperationException("Could not assign Leadership role: " +
                    string.Join("; ", roleResult.Errors.Select(e => e.Description)));
        }
    }
}

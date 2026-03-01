using Microsoft.AspNetCore.Identity;
using NewsSite.Domain.Security;

namespace NewsSite.Api.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        var roles = new[] { Roles.Subscriber, Roles.Writer, Roles.Editor };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await EnsureUserAsync(userManager, "subscriber@test.com", "Password1!", Roles.Subscriber);
        await EnsureUserAsync(userManager, "writer@test.com", "Password1!", Roles.Writer);
        await EnsureUserAsync(userManager, "editor@test.com", "Password1!", Roles.Editor);
    }

    private static async Task EnsureUserAsync(UserManager<IdentityUser> userManager, string email, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var created = await userManager.CreateAsync(user, password);
            if (!created.Succeeded)
                throw new Exception(string.Join(", ", created.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var added = await userManager.AddToRoleAsync(user, role);
            if (!added.Succeeded)
                throw new Exception(string.Join(", ", added.Errors.Select(e => e.Description)));
        }
    }
}
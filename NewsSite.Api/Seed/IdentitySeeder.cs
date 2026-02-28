using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NewsSite.Domain.Security;

namespace NewsSite.Api.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        // Roles
        var roles = new[] { Roles.Subscriber, Roles.Writer, Roles.Editor };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Users (simple predictable passwords for local dev only)
        await EnsureUserAsync(userManager, "subscriber@test.com", "Password1!", Roles.Subscriber);
        await EnsureUserAsync(userManager, "writer@test.com", "Password1!", Roles.Writer);
        await EnsureUserAsync(userManager, "editor@test.com", "Password1!", Roles.Editor);
    }

    private static async Task EnsureUserAsync(
        UserManager<IdentityUser> userManager,
        string email,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createRes = await userManager.CreateAsync(user, password);
            if (!createRes.Succeeded)
            {
                var errors = string.Join(", ", createRes.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create user {email}: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var addRoleRes = await userManager.AddToRoleAsync(user, role);
            if (!addRoleRes.Succeeded)
            {
                var errors = string.Join(", ", addRoleRes.Errors.Select(e => e.Description));
                throw new Exception($"Failed to add role {role} to user {email}: {errors}");
            }
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewsSite.Domain.Entities;
using NewsSite.Infrastructure.Data;

namespace NewsSite.Api.Seed;

public static class DomainSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        // Only seed once
        if (await db.Articles.AnyAsync())
            return;

        var writer = await userManager.FindByEmailAsync("writer@test.com");
        var subscriber = await userManager.FindByEmailAsync("subscriber@test.com");

        if (writer is null || subscriber is null)
            return;

        // 1) Article by writer
        var article = new Article
        {
            Title = "Seeded: Writer article",
            Content = "This article is seeded to make authorization testing fast.",
            AuthorId = writer.Id,
            CreatedAt = DateTime.UtcNow
        };

        db.Articles.Add(article);

        // 2) Comment by subscriber on that article
        var comment = new Comment
        {
            ArticleId = article.Id,
            Content = "Seeded: Subscriber comment (use this to test delete/update rules).",
            UserId = subscriber.Id,
            CreatedAt = DateTime.UtcNow
        };

        db.Comments.Add(comment);

        await db.SaveChangesAsync();
    }
}
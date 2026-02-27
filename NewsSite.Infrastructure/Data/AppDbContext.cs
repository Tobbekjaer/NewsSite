using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewsSite.Domain.Entities;

namespace NewsSite.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Article configuration
        builder.Entity<Article>(entity =>
        {
            entity.ToTable("articles");

            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("article_id");

            entity.Property(a => a.Title)
                .HasColumnName("title")
                .IsRequired();

            entity.Property(a => a.Content)
                .HasColumnName("content")
                .IsRequired();

            entity.Property(a => a.AuthorId)
                .HasColumnName("author_id")
                .IsRequired();

            entity.Property(a => a.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.HasMany(a => a.Comments)
                .WithOne(c => c.Article)
                .HasForeignKey(c => c.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Comment configuration
        builder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments");

            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("comment_id");

            entity.Property(c => c.Content)
                .HasColumnName("content")
                .IsRequired();

            entity.Property(c => c.ArticleId)
                .HasColumnName("article_id")
                .IsRequired();

            entity.Property(c => c.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.HasIndex(c => c.ArticleId);
            entity.HasIndex(c => c.UserId);
        });
    }
}

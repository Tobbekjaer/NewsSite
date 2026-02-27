namespace NewsSite.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Content { get; set; } = string.Empty;

    public Guid ArticleId { get; set; }
    
    // Navigation back to Article
    public Article? Article { get; set; }

    // Identity user id (string by default)
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
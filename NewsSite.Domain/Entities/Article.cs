namespace NewsSite.Domain.Entities;

public class Article
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    // Identity user id (string by default)
    public string AuthorId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
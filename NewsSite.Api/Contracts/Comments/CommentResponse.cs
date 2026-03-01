namespace NewsSite.Api.Contracts.Comments;

public record CommentResponse(
    Guid Id,
    string Content,
    Guid ArticleId,
    string UserId,
    DateTime CreatedAt
);
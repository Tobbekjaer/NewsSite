using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsSite.Api.Auth;
using NewsSite.Api.Contracts.Comments;
using NewsSite.Domain.Entities;
using NewsSite.Domain.Security;
using NewsSite.Infrastructure.Data;

namespace NewsSite.Api.Controllers;

[ApiController]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuthorizationService _authz;

    public CommentsController(AppDbContext db, IAuthorizationService authz)
    {
        _db = db;
        _authz = authz;
    }

    // Public: read comments on an article
    [HttpGet("articles/{articleId:guid}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> GetForArticle(Guid articleId)
    {
        var articleExists = await _db.Articles.AsNoTracking().AnyAsync(a => a.Id == articleId);
        if (!articleExists) return NotFound();

        var comments = await _db.Comments
            .AsNoTracking()
            .Where(c => c.ArticleId == articleId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentResponse(
                c.Id,
                c.Content,
                c.ArticleId,
                c.UserId,
                c.CreatedAt
            ))
            .ToListAsync();

        return Ok(comments);
    }

    // Subscriber: create comment on an article
    [HttpPost("articles/{articleId:guid}/comments")]
    [Authorize(Roles = Roles.Subscriber)]
    public async Task<IActionResult> Create(Guid articleId, [FromBody] CreateCommentRequest req)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var article = await _db.Articles.AsNoTracking().FirstOrDefaultAsync(a => a.Id == articleId);
        if (article is null) return NotFound();

        var comment = new Comment
        {
            ArticleId = articleId,
            Content = req.Content,
            UserId = userId
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        var response = new CommentResponse(
            comment.Id,
            comment.Content,
            comment.ArticleId,
            comment.UserId,
            comment.CreatedAt
        );

        return CreatedAtAction(nameof(GetForArticle), new { articleId }, response);
    }

    // Update: handled by CommentAuthorizationHandler (Editor OR Subscriber owner)
    [HttpPut("comments/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommentRequest req)
    {
        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null) return NotFound();

        var auth = await _authz.AuthorizeAsync(User, comment, CommentOperations.Update);
        if (!auth.Succeeded) return Forbid();

        comment.Content = req.Content;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // Delete: handled by CommentAuthorizationHandler (Editor OR Subscriber owner OR Writer owns article)
    [HttpDelete("comments/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        // IMPORTANT: include Article so writer rule can check AuthorId
        var comment = await _db.Comments
            .Include(c => c.Article)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment is null) return NotFound();

        var auth = await _authz.AuthorizeAsync(User, comment, CommentOperations.Delete);
        if (!auth.Succeeded) return Forbid();

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

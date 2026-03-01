using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsSite.Api.Auth;
using NewsSite.Api.Contracts.Articles;
using NewsSite.Domain.Entities;
using NewsSite.Domain.Security;
using NewsSite.Infrastructure.Data;

namespace NewsSite.Api.Controllers;

[ApiController]
[Route("articles")]
public class ArticlesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuthorizationService _authz;

    public ArticlesController(AppDbContext db, IAuthorizationService authz)
    {
        _db = db;
        _authz = authz;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var articles = await _db.Articles
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(articles);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var article = await _db.Articles
            .AsNoTracking()
            .Include(a => a.Comments)
            .FirstOrDefaultAsync(a => a.Id == id);

        return article is null ? NotFound() : Ok(article);
    }

    // Create: Writer only
    [HttpPost]
    [Authorize(Roles = Roles.Writer)]
    public async Task<IActionResult> Create([FromBody] CreateArticleRequest req)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var article = new Article
        {
            Title = req.Title,
            Content = req.Content,
            AuthorId = userId
        };

        _db.Articles.Add(article);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = article.Id }, article);
    }

    // Update: handled by ArticleAuthorizationHandler (Editor OR Writer owner)
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateArticleRequest req)
    {
        var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
        if (article is null) return NotFound();

        var auth = await _authz.AuthorizeAsync(User, article, ArticleOperations.Update);
        if (!auth.Succeeded) return Forbid();

        article.Title = req.Title;
        article.Content = req.Content;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Delete: handled by ArticleAuthorizationHandler (Editor OR Writer owner)
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
        if (article is null) return NotFound();

        var auth = await _authz.AuthorizeAsync(User, article, ArticleOperations.Delete);
        if (!auth.Succeeded) return Forbid();

        _db.Articles.Remove(article);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

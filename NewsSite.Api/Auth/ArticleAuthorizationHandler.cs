using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using NewsSite.Domain.Entities;
using NewsSite.Domain.Security;

namespace NewsSite.Api.Auth;

public class ArticleAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Article>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Article resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Task.CompletedTask;

        // Editor can do everything on articles
        if (context.User.IsInRole(Roles.Editor))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Writer can update/delete own articles
        var isWriterOwner = context.User.IsInRole(Roles.Writer) && resource.AuthorId == userId;
        if (isWriterOwner &&
            (requirement.Name == ArticleOperations.Update.Name || requirement.Name == ArticleOperations.Delete.Name))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
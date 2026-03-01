using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using NewsSite.Domain.Entities;
using NewsSite.Domain.Security;

namespace NewsSite.Api.Auth;

public class CommentAuthorizationHandler
    : AuthorizationHandler<OperationAuthorizationRequirement, Comment>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Comment resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Task.CompletedTask;

        // Editor can do everything
        if (context.User.IsInRole(Roles.Editor))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (requirement.Name == CommentOperations.Update.Name)
        {
            // Subscriber can update own comment
            if (context.User.IsInRole(Roles.Subscriber) && resource.UserId == userId)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }

        if (requirement.Name == CommentOperations.Delete.Name)
        {
            // Subscriber can delete own comment
            if (context.User.IsInRole(Roles.Subscriber) && resource.UserId == userId)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Writer can delete comments on own articles (needs resource.Article loaded)
            if (context.User.IsInRole(Roles.Writer) &&
                resource.Article is not null &&
                resource.Article.AuthorId == userId)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
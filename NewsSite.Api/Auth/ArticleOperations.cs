using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace NewsSite.Api.Auth;

public static class ArticleOperations
{
    public static readonly OperationAuthorizationRequirement Update =
        new() { Name = nameof(Update) };

    public static readonly OperationAuthorizationRequirement Delete =
        new() { Name = nameof(Delete) };
}
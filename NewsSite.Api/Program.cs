using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Logging;
using NewsSite.Api.Auth;
using NewsSite.Api.Extensions;
using NewsSite.Api.Seed;
using NewsSite.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

IdentityModelEventSource.ShowPII = true;

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IAuthorizationHandler, ArticleAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CommentAuthorizationHandler>();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Debug for dev
app.MapGet("/_debug/path", (HttpRequest req) => Results.Ok(new { req.PathBase, req.Path }));
app.MapGet("/_debug/endpoints", (IEnumerable<EndpointDataSource> sources) =>
{
    var endpoints = sources.SelectMany(s => s.Endpoints)
        .Select(e => new { e.DisplayName, routePattern = (e as RouteEndpoint)?.RoutePattern.RawText })
        .OrderBy(x => x.routePattern)
        .ToList();
    return Results.Ok(endpoints);
});

app.Run();
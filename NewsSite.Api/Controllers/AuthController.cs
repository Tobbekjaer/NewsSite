using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsSite.Api.Auth;
using NewsSite.Domain.Security;

namespace NewsSite.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly JwtTokenService _jwt;

    public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, JwtTokenService jwt)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwt = jwt;
    }

    public record LoginRequest(string Email, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user is null) return Unauthorized();

        var ok = await _signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: false);
        if (!ok.Succeeded) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwt.CreateToken(user, roles);

        return Ok(new { access_token = token, token_type = "Bearer" });
    }

    public record RegisterRequest(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var user = new IdentityUser { UserName = req.Email, Email = req.Email, EmailConfirmed = true };
        var created = await _userManager.CreateAsync(user, req.Password);
        if (!created.Succeeded) return BadRequest(created.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, Roles.Subscriber);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwt.CreateToken(user, roles);

        return Ok(new { access_token = token, token_type = "Bearer" });
    }
}
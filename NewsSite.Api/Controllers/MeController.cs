using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace NewsSite.Api.Controllers;

[ApiController]
[Route("me")]
public class MeController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public IActionResult Get()
    {
        return Ok(new
        {
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            email = User.FindFirstValue(ClaimTypes.Email),
            roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList()
        });
    }
}
using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected Guid? CurrentUserId
    {
        get
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(userIdStr, out var id) ? id : null;
        }
    }

    protected string? CurrentUserEmail => User.FindFirstValue(ClaimTypes.Email);

    protected string? GetClientIpAddress()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            return forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}

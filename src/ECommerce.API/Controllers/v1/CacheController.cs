using Asp.Versioning;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/cache")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class CacheController : BaseApiController
{
    private readonly ICacheService _cacheService;

    public CacheController(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    [HttpPost("clear-prefix")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearByPrefix([FromQuery] string prefix, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return BadRequest(ApiResponse<bool>.FailureResponse("Validation failed", "Prefix parameter is required."));
        }

        await _cacheService.RemoveByPrefixAsync(prefix, cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResponse(true, $"Cache entries matching prefix '{prefix}' evicted."));
    }

    [HttpPost("clear-catalog")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCatalogCache(CancellationToken cancellationToken)
    {
        await _cacheService.RemoveByPrefixAsync("catalog:", cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Catalog cache entries evicted."));
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetCacheStatus()
    {
        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            Provider = "Hybrid Distributed Cache (Redis / In-Memory)",
            IsOperational = true,
            TimestampUtc = DateTime.UtcNow
        }));
    }
}

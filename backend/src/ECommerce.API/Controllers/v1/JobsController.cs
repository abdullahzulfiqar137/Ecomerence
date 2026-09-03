using Asp.Versioning;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/jobs")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class JobsController : BaseApiController
{
    private readonly IBackgroundJobService _jobService;

    public JobsController(IBackgroundJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpPost("release-holds")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReleaseExpiredHolds(CancellationToken cancellationToken)
    {
        var result = await _jobService.ReleaseExpiredStockHoldsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("cleanup-carts")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CleanupAbandonedCarts([FromQuery] int olderThanDays = 30, CancellationToken cancellationToken = default)
    {
        var result = await _jobService.CleanupAbandonedGuestCartsAsync(olderThanDays, cancellationToken);
        return Ok(result);
    }

    [HttpPost("deactivate-coupons")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateExpiredCoupons(CancellationToken cancellationToken)
    {
        var result = await _jobService.DeactivateExpiredCouponsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("sync-alerts")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncLowStockAlerts(CancellationToken cancellationToken)
    {
        var result = await _jobService.SyncLowStockAlertsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("process-outbox")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessOutbox(CancellationToken cancellationToken)
    {
        var result = await _jobService.ProcessPendingOutboxNotificationsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetJobsStatus()
    {
        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            Engine = "Asynchronous Background Task Service (.NET Hosted Service)",
            IsOperational = true,
            RegisteredJobs = new[]
            {
                "Inventory Reservation Release",
                "Abandoned Cart Cleanup",
                "Expired Coupon Deactivation",
                "Low Stock Alert Sync",
                "Outbox Dispatch Worker"
            },
            ServerTimeUtc = DateTime.UtcNow
        }));
    }
}

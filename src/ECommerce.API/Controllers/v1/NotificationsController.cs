using Asp.Versioning;
using ECommerce.Application.DTOs.Notifications;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notifications")]
public class NotificationsController : BaseApiController
{
    private readonly INotificationService _notificationService;
    private readonly IValidator<SendEmailRequestDto> _sendEmailValidator;

    public NotificationsController(
        INotificationService notificationService,
        IValidator<SendEmailRequestDto> sendEmailValidator)
    {
        _notificationService = notificationService;
        _sendEmailValidator = sendEmailValidator;
    }

    [Authorize]
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.GetUserNotificationsAsync(CurrentUserId!.Value, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var result = await _notificationService.MarkAsReadAsync(id, CurrentUserId!.Value, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("send-custom")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendCustomEmail([FromBody] SendEmailRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _sendEmailValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<NotificationDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _notificationService.SendCustomEmailAsync(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("orders/{orderId:guid}/send-confirmation")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendOrderConfirmation(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _notificationService.SendOrderConfirmationAsync(orderId, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpGet("outbox")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOutbox([FromQuery] NotificationStatus? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.GetOutboxMessagesAsync(status, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("{id:guid}/resend")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResendNotification(Guid id, CancellationToken cancellationToken)
    {
        var result = await _notificationService.ResendFailedNotificationAsync(id, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}

using Asp.Versioning;
using ECommerce.Application.DTOs.Reviews;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reviews")]
public class ReviewsController : BaseApiController
{
    private readonly IReviewService _reviewService;
    private readonly IValidator<CreateProductReviewDto> _createValidator;
    private readonly IValidator<ModerateReviewDto> _moderateValidator;
    private readonly IValidator<AdminReviewResponseDto> _responseValidator;

    public ReviewsController(
        IReviewService reviewService,
        IValidator<CreateProductReviewDto> createValidator,
        IValidator<ModerateReviewDto> moderateValidator,
        IValidator<AdminReviewResponseDto> responseValidator)
    {
        _reviewService = reviewService;
        _createValidator = createValidator;
        _moderateValidator = moderateValidator;
        _responseValidator = responseValidator;
    }

    [HttpGet("products/{productId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductReviewDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductReviews(Guid productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = "newest", CancellationToken cancellationToken = default)
    {
        var result = await _reviewService.GetProductReviewsAsync(productId, page, pageSize, sortBy, cancellationToken);
        return Ok(result);
    }

    [HttpGet("products/{productId:guid}/summary")]
    [ProducesResponseType(typeof(ApiResponse<ProductReviewSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductReviewSummary(Guid productId, CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetProductReviewSummaryAsync(productId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductReviewDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ProductReviewDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitReview([FromBody] CreateProductReviewDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ProductReviewDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _reviewService.SubmitReviewAsync(CurrentUserId!.Value, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{id:guid}/helpful")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VoteHelpful(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reviewService.VoteHelpfulAsync(id, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager,Moderator")]
    [HttpGet("moderation")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductReviewDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModerationQueue([FromQuery] ReviewStatus? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _reviewService.GetReviewsForModerationAsync(status, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager,Moderator")]
    [HttpPatch("{id:guid}/moderate")]
    [ProducesResponseType(typeof(ApiResponse<ProductReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductReviewDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ModerateReview(Guid id, [FromBody] ModerateReviewDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _moderateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ProductReviewDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _reviewService.ModerateReviewAsync(id, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager,Moderator")]
    [HttpPost("{id:guid}/respond")]
    [ProducesResponseType(typeof(ApiResponse<ProductReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductReviewDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddStaffResponse(Guid id, [FromBody] AdminReviewResponseDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _responseValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ProductReviewDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _reviewService.AddAdminResponseAsync(id, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager,Moderator")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reviewService.DeleteReviewAsync(id, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}

using Asp.Versioning;
using ECommerce.Application.DTOs.Analytics;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/analytics")]
[Authorize(Roles = "SuperAdmin,Admin,FinanceManager")]
public class AnalyticsController : BaseApiController
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IValidator<SalesTrendRequestDto> _salesTrendValidator;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        IValidator<SalesTrendRequestDto> salesTrendValidator)
    {
        _analyticsService = analyticsService;
        _salesTrendValidator = salesTrendValidator;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardKpiDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardKpi(CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetDashboardKpiAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("sales-trend")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SalesTrendDataPointDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SalesTrendDataPointDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSalesTrend([FromQuery] SalesTrendRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _salesTrendValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<IReadOnlyList<SalesTrendDataPointDto>>.FailureResponse("Validation failed", errors));
        }

        var result = await _analyticsService.GetSalesTrendAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("top-products")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TopSellingProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopSellingProducts([FromQuery] int top = 10, CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetTopSellingProductsAsync(top, cancellationToken);
        return Ok(result);
    }

    [HttpGet("order-status")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrderStatusDistributionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderStatusDistribution(CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetOrderStatusDistributionAsync(cancellationToken);
        return Ok(result);
    }
}

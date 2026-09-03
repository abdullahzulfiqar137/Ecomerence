using Asp.Versioning;
using ECommerce.Application.DTOs.Search;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/search")]
public class SearchController : BaseApiController
{
    private readonly ISearchService _searchService;
    private readonly IValidator<SearchRequestDto> _searchValidator;

    public SearchController(
        ISearchService searchService,
        IValidator<SearchRequestDto> searchValidator)
    {
        _searchService = searchService;
        _searchValidator = searchValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SearchResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SearchResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchProducts([FromQuery] SearchRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _searchValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<SearchResponseDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _searchService.SearchProductsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("suggestions")]
    [ProducesResponseType(typeof(ApiResponse<SearchSuggestionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuggestions([FromQuery] string q, CancellationToken cancellationToken)
    {
        var result = await _searchService.GetSuggestionsAsync(q, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin,Admin,ProductManager")]
    [HttpPost("reindex")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReindexCatalog(CancellationToken cancellationToken)
    {
        var result = await _searchService.RebuildSearchIndexAsync(cancellationToken);
        return Ok(result);
    }
}

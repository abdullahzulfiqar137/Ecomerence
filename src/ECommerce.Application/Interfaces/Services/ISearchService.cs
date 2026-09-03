using ECommerce.Application.DTOs.Search;
using ECommerce.Application.Models;

namespace ECommerce.Application.Interfaces.Services;

public interface ISearchService
{
    Task<ApiResponse<SearchResponseDto>> SearchProductsAsync(SearchRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<SearchSuggestionDto>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> RebuildSearchIndexAsync(CancellationToken cancellationToken = default);
}

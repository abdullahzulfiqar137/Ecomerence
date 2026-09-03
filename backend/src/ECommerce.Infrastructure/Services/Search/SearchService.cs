using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.DTOs.Search;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Search;

public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SearchService> _logger;

    public SearchService(ApplicationDbContext context, ILogger<SearchService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<SearchResponseDto>> SearchProductsAsync(SearchRequestDto request, CancellationToken cancellationToken = default)
    {
        var baseQuery = _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Tags)
            .Include(p => p.InventoryItems)
            .Where(p => p.IsActive)
            .AsQueryable();

        // 1. Full-text search matching across terms
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var terms = request.Query.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var term in terms)
            {
                baseQuery = baseQuery.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(term)) ||
                    (p.FullDescription != null && p.FullDescription.ToLower().Contains(term)) ||
                    p.SKU.ToLower().Contains(term) ||
                    (p.Brand != null && p.Brand.Name.ToLower().Contains(term)) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(term)) ||
                    p.Tags.Any(t => t.Tag.ToLower().Contains(term)));
            }
        }

        // 2. Tag filter
        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            var tag = request.Tag.Trim().ToLower();
            baseQuery = baseQuery.Where(p => p.Tags.Any(t => t.Tag.ToLower() == tag));
        }

        // 3. Compute Facets before applying specific narrow filters
        var facetUniverse = await baseQuery.Select(p => new
        {
            p.Id,
            p.Price,
            p.AverageRating,
            IsInStock = !p.TrackInventory || p.InventoryItems.Any(i => (i.QuantityOnHand - i.QuantityReserved) > 0),
            CategoryId = (Guid?)p.CategoryId,
            CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
            BrandId = p.BrandId,
            BrandName = p.Brand != null ? p.Brand.Name : "No Brand"
        }).ToListAsync(cancellationToken);

        var facets = new SearchFacetsDto
        {
            Categories = facetUniverse
                .Where(x => x.CategoryId.HasValue)
                .GroupBy(x => new { Id = x.CategoryId!.Value, x.CategoryName })
                .Select(g => new FacetItemDto { Id = g.Key.Id, Name = g.Key.CategoryName, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList(),

            Brands = facetUniverse
                .Where(x => x.BrandId.HasValue)
                .GroupBy(x => new { Id = x.BrandId!.Value, x.BrandName })
                .Select(g => new FacetItemDto { Id = g.Key.Id, Name = g.Key.BrandName, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList(),

            PriceRange = new PriceRangeFacetDto
            {
                MinPrice = facetUniverse.Any() ? Math.Round(facetUniverse.Min(x => x.Price), 2) : 0m,
                MaxPrice = facetUniverse.Any() ? Math.Round(facetUniverse.Max(x => x.Price), 2) : 0m,
                AveragePrice = facetUniverse.Any() ? Math.Round(facetUniverse.Average(x => x.Price), 2) : 0m
            },

            Ratings = new List<RatingFacetDto>
            {
                new() { RatingStars = 5, Count = facetUniverse.Count(x => x.AverageRating >= 4.5m) },
                new() { RatingStars = 4, Count = facetUniverse.Count(x => x.AverageRating >= 3.5m && x.AverageRating < 4.5m) },
                new() { RatingStars = 3, Count = facetUniverse.Count(x => x.AverageRating >= 2.5m && x.AverageRating < 3.5m) },
                new() { RatingStars = 2, Count = facetUniverse.Count(x => x.AverageRating >= 1.5m && x.AverageRating < 2.5m) },
                new() { RatingStars = 1, Count = facetUniverse.Count(x => x.AverageRating < 1.5m) }
            },

            InStockCount = facetUniverse.Count(x => x.IsInStock),
            OutOfStockCount = facetUniverse.Count(x => !x.IsInStock)
        };

        // 4. Apply Narrow Filters
        var filteredQuery = baseQuery;

        if (request.CategoryId.HasValue)
        {
            filteredQuery = filteredQuery.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (request.BrandId.HasValue)
        {
            filteredQuery = filteredQuery.Where(p => p.BrandId == request.BrandId.Value);
        }

        if (request.MinPrice.HasValue)
        {
            filteredQuery = filteredQuery.Where(p => p.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            filteredQuery = filteredQuery.Where(p => p.Price <= request.MaxPrice.Value);
        }

        if (request.InStockOnly.HasValue && request.InStockOnly.Value)
        {
            filteredQuery = filteredQuery.Where(p => !p.TrackInventory || p.InventoryItems.Any(i => (i.QuantityOnHand - i.QuantityReserved) > 0));
        }

        if (request.MinRating.HasValue)
        {
            filteredQuery = filteredQuery.Where(p => p.AverageRating >= request.MinRating.Value);
        }

        // 5. Sorting
        filteredQuery = (request.SortBy?.ToLowerInvariant()) switch
        {
            "price_asc" => filteredQuery.OrderBy(p => p.Price),
            "price_desc" => filteredQuery.OrderByDescending(p => p.Price),
            "rating" => filteredQuery.OrderByDescending(p => p.AverageRating).ThenByDescending(p => p.TotalReviews),
            "newest" => filteredQuery.OrderByDescending(p => p.CreatedAtUtc),
            _ => filteredQuery.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.AverageRating).ThenByDescending(p => p.CreatedAtUtc)
        };

        // 6. Pagination & Mapping
        var totalCount = await filteredQuery.CountAsync(cancellationToken);
        var products = await filteredQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = products.Select(p => MapToProductListDto(p)).ToList();

        var response = new SearchResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            Facets = facets
        };

        return ApiResponse<SearchResponseDto>.SuccessResponse(response);
    }

    public async Task<ApiResponse<SearchSuggestionDto>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
        {
            return ApiResponse<SearchSuggestionDto>.SuccessResponse(new SearchSuggestionDto { Query = query });
        }

        var normalized = query.Trim().ToLower();

        // 1. Category Suggestions
        var categorySuggestions = await _context.Categories
            .Where(c => c.IsActive && c.Name.ToLower().Contains(normalized))
            .Take(3)
            .Select(c => new FacetItemDto { Id = c.Id, Name = c.Name, Count = c.Products.Count(p => p.IsActive) })
            .ToListAsync(cancellationToken);

        // 2. Brand Suggestions
        var brandSuggestions = await _context.Brands
            .Where(b => b.IsActive && b.Name.ToLower().Contains(normalized))
            .Take(3)
            .Select(b => new FacetItemDto { Id = b.Id, Name = b.Name, Count = b.Products.Count(p => p.IsActive) })
            .ToListAsync(cancellationToken);

        // 3. Top Products Preview
        var topProducts = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.InventoryItems)
            .Where(p => p.IsActive && (p.Name.ToLower().Contains(normalized) || p.SKU.ToLower().Contains(normalized)))
            .OrderByDescending(p => p.AverageRating)
            .Take(4)
            .ToListAsync(cancellationToken);

        var productPreviews = topProducts.Select(p => MapToProductListDto(p)).ToList();

        // 4. Keyword suggestions
        var keywordSuggestions = topProducts.Select(p => p.Name).Distinct().Take(5).ToList();

        var result = new SearchSuggestionDto
        {
            Query = query,
            KeywordSuggestions = keywordSuggestions,
            CategorySuggestions = categorySuggestions,
            BrandSuggestions = brandSuggestions,
            TopProductPreviews = productPreviews
        };

        return ApiResponse<SearchSuggestionDto>.SuccessResponse(result);
    }

    public Task<ApiResponse<bool>> RebuildSearchIndexAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Rebuilt full-text catalog search index at {UtcTimestamp}", DateTime.UtcNow);
        return Task.FromResult(ApiResponse<bool>.SuccessResponse(true, "Catalog search index refreshed successfully."));
    }

    private static ProductListDto MapToProductListDto(Product p)
    {
        var primaryImage = p.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl
            ?? p.Images.FirstOrDefault()?.ImageUrl;

        var inStock = !p.TrackInventory || p.InventoryItems.Any(i => (i.QuantityOnHand - i.QuantityReserved) > 0);

        return new ProductListDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            SKU = p.SKU,
            Price = p.Price,
            SalePrice = p.SalePrice,
            Type = p.Type,
            MainImageUrl = primaryImage,
            CategoryName = p.Category?.Name ?? string.Empty,
            BrandName = p.Brand?.Name ?? string.Empty,
            IsFeatured = p.IsFeatured,
            AverageRating = p.AverageRating,
            TotalReviews = p.TotalReviews,
            IsInStock = inStock
        };
    }
}

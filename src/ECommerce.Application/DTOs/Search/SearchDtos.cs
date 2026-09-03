using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.DTOs.Search;

public class SearchRequestDto
{
    public string? Query { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStockOnly { get; set; }
    public decimal? MinRating { get; set; }
    public string? Tag { get; set; }
    public Dictionary<string, string>? Attributes { get; set; } // e.g. "Color": "Black", "Size": "XL"

    public string? SortBy { get; set; } = "relevance"; // relevance, price_asc, price_desc, rating, newest
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SearchResponseDto
{
    public IReadOnlyList<ProductListDto> Items { get; set; } = new List<ProductListDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
    public SearchFacetsDto Facets { get; set; } = new();
}

public class SearchFacetsDto
{
    public List<FacetItemDto> Categories { get; set; } = new();
    public List<FacetItemDto> Brands { get; set; } = new();
    public PriceRangeFacetDto PriceRange { get; set; } = new();
    public List<RatingFacetDto> Ratings { get; set; } = new();
    public int InStockCount { get; set; }
    public int OutOfStockCount { get; set; }
}

public class FacetItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class PriceRangeFacetDto
{
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal AveragePrice { get; set; }
}

public class RatingFacetDto
{
    public int RatingStars { get; set; }
    public int Count { get; set; }
}

public class SearchSuggestionDto
{
    public string Query { get; set; } = string.Empty;
    public List<string> KeywordSuggestions { get; set; } = new();
    public List<FacetItemDto> CategorySuggestions { get; set; } = new();
    public List<FacetItemDto> BrandSuggestions { get; set; } = new();
    public List<ProductListDto> TopProductPreviews { get; set; } = new();
}

using ECommerce.Application.DTOs.Catalog;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Specifications;

public class ProductWithFilterSpecification : BaseSpecification<Product>
{
    public ProductWithFilterSpecification(ProductFilterParams filterParams)
        : base(p =>
            p.IsActive &&
            (string.IsNullOrEmpty(filterParams.SearchTerm) || p.Name.Contains(filterParams.SearchTerm) || (p.ShortDescription != null && p.ShortDescription.Contains(filterParams.SearchTerm)) || p.SKU.Contains(filterParams.SearchTerm)) &&
            (!filterParams.CategoryId.HasValue || p.CategoryId == filterParams.CategoryId.Value) &&
            (!filterParams.BrandId.HasValue || p.BrandId == filterParams.BrandId.Value) &&
            (!filterParams.MinPrice.HasValue || p.Price >= filterParams.MinPrice.Value) &&
            (!filterParams.MaxPrice.HasValue || p.Price <= filterParams.MaxPrice.Value) &&
            (!filterParams.IsFeatured.HasValue || p.IsFeatured == filterParams.IsFeatured.Value) &&
            (!filterParams.Type.HasValue || p.Type == filterParams.Type.Value))
    {
        AddInclude(p => p.Category!);
        AddInclude(p => p.Brand!);
        AddInclude(p => p.Images);

        ApplyPaging((filterParams.Page - 1) * filterParams.PageSize, filterParams.PageSize);

        if (!string.IsNullOrEmpty(filterParams.SortBy))
        {
            switch (filterParams.SortBy.ToLowerInvariant())
            {
                case "price":
                    if (filterParams.SortDescending)
                        ApplyOrderByDescending(p => p.Price);
                    else
                        ApplyOrderBy(p => p.Price);
                    break;
                case "rating":
                    ApplyOrderByDescending(p => p.AverageRating);
                    break;
                case "name":
                    if (filterParams.SortDescending)
                        ApplyOrderByDescending(p => p.Name);
                    else
                        ApplyOrderBy(p => p.Name);
                    break;
                default:
                    ApplyOrderByDescending(p => p.CreatedAtUtc);
                    break;
            }
        }
        else
        {
            ApplyOrderByDescending(p => p.CreatedAtUtc);
        }
    }
}

public class ProductWithFilterForCountSpecification : BaseSpecification<Product>
{
    public ProductWithFilterForCountSpecification(ProductFilterParams filterParams)
        : base(p =>
            p.IsActive &&
            (string.IsNullOrEmpty(filterParams.SearchTerm) || p.Name.Contains(filterParams.SearchTerm) || (p.ShortDescription != null && p.ShortDescription.Contains(filterParams.SearchTerm)) || p.SKU.Contains(filterParams.SearchTerm)) &&
            (!filterParams.CategoryId.HasValue || p.CategoryId == filterParams.CategoryId.Value) &&
            (!filterParams.BrandId.HasValue || p.BrandId == filterParams.BrandId.Value) &&
            (!filterParams.MinPrice.HasValue || p.Price >= filterParams.MinPrice.Value) &&
            (!filterParams.MaxPrice.HasValue || p.Price <= filterParams.MaxPrice.Value) &&
            (!filterParams.IsFeatured.HasValue || p.IsFeatured == filterParams.IsFeatured.Value) &&
            (!filterParams.Type.HasValue || p.Type == filterParams.Type.Value))
    {
    }
}

public class ProductWithDetailsSpecification : BaseSpecification<Product>
{
    public ProductWithDetailsSpecification(Guid id)
        : base(p => p.Id == id)
    {
        AddInclude(p => p.Category!);
        AddInclude(p => p.Brand!);
        AddInclude(p => p.Images);
        AddInclude(p => p.Variants);
    }

    public ProductWithDetailsSpecification(string slug)
        : base(p => p.Slug == slug.ToLowerInvariant())
    {
        AddInclude(p => p.Category!);
        AddInclude(p => p.Brand!);
        AddInclude(p => p.Images);
        AddInclude(p => p.Variants);
    }
}

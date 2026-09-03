namespace ECommerce.Application.Common;

public static class CacheKeys
{
    public const string CatalogPrefix = "catalog:";
    public const string Categories = "catalog:categories:all";
    public const string Brands = "catalog:brands:all";
    public const string ProductById = "catalog:product:{0}";
    public const string ProductBySlug = "catalog:product:slug:{0}";
    public const string FeaturedProducts = "catalog:products:featured";

    public const string InternationalPrefix = "intl:";
    public const string ActiveCurrencies = "intl:currencies:active";
    public const string ActiveCountries = "intl:countries:active";
    public const string ActiveTaxRules = "intl:taxrules:active";

    public const string SettingsPrefix = "settings:";
    public const string ShippingMethods = "settings:shipping:methods";

    public static string ForProduct(Guid id) => string.Format(ProductById, id);
    public static string ForProductSlug(string slug) => string.Format(ProductBySlug, slug);
}

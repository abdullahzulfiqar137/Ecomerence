namespace ECommerce.Domain.Constants;

public static class AppRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Seller = "Seller";
    public const string Vendor = "Vendor";
    public const string WarehouseManager = "WarehouseManager";
    public const string InventoryManager = "InventoryManager";
    public const string CustomerSupport = "CustomerSupport";
    public const string MarketingManager = "MarketingManager";
    public const string Customer = "Customer";

    public static readonly IReadOnlyList<string> AllRoles = new[]
    {
        SuperAdmin,
        Admin,
        Seller,
        Vendor,
        WarehouseManager,
        InventoryManager,
        CustomerSupport,
        MarketingManager,
        Customer
    };
}

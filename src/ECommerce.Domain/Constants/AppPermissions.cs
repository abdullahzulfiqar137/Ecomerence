namespace ECommerce.Domain.Constants;

public static class AppPermissions
{
    // Products
    public const string ProductsRead = "Permissions.Products.Read";
    public const string ProductsCreate = "Permissions.Products.Create";
    public const string ProductsEdit = "Permissions.Products.Edit";
    public const string ProductsDelete = "Permissions.Products.Delete";

    // Categories & Brands
    public const string CategoriesManage = "Permissions.Categories.Manage";
    public const string BrandsManage = "Permissions.Brands.Manage";

    // Orders
    public const string OrdersRead = "Permissions.Orders.Read";
    public const string OrdersManage = "Permissions.Orders.Manage";
    public const string OrdersCancel = "Permissions.Orders.Cancel";

    // Inventory & Warehouses
    public const string InventoryRead = "Permissions.Inventory.Read";
    public const string InventoryManage = "Permissions.Inventory.Manage";

    // Users & Roles
    public const string UsersRead = "Permissions.Users.Read";
    public const string UsersManage = "Permissions.Users.Manage";
    public const string RolesManage = "Permissions.Roles.Manage";

    // Marketing & Discounts
    public const string CouponsManage = "Permissions.Coupons.Manage";
    public const string PromotionsManage = "Permissions.Promotions.Manage";

    // Reviews & Reports
    public const string ReviewsModerate = "Permissions.Reviews.Moderate";
    public const string ReportsView = "Permissions.Reports.View";

    // Settings
    public const string SettingsManage = "Permissions.Settings.Manage";
}

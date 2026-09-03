using System.Security.Cryptography;
using System.Text;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger, bool seedCatalogData = true)
    {
        try
        {
            await SeedRolesAndPermissionsAsync(context);
            await SeedUsersAsync(context);
            await SeedCurrenciesAndCountriesAsync(context);

            if (seedCatalogData)
            {
                await SeedCategoriesAndBrandsAsync(context);
                await SeedProductsAndVariantsAsync(context);
                await SeedWarehousesAndInventoryAsync(context);
                await SeedShippingAsync(context);
                await SeedCouponsAndTaxRulesAsync(context);
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Database seeded successfully (CatalogData: {CatalogData}).", seedCatalogData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task SeedRolesAndPermissionsAsync(ApplicationDbContext context)
    {
        if (await context.Roles.AnyAsync()) return;

        var roles = AppRoles.AllRoles.Select(roleName => new Role
        {
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant(),
            Description = $"{roleName} platform role"
        }).ToList();

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();

        // Seed Core Permissions
        var permissions = new List<Permission>
        {
            new() { Name = AppPermissions.ProductsRead, Group = "Catalog", Description = "View products" },
            new() { Name = AppPermissions.ProductsCreate, Group = "Catalog", Description = "Create products" },
            new() { Name = AppPermissions.ProductsEdit, Group = "Catalog", Description = "Edit products" },
            new() { Name = AppPermissions.ProductsDelete, Group = "Catalog", Description = "Delete products" },
            new() { Name = AppPermissions.CategoriesManage, Group = "Catalog", Description = "Manage categories" },
            new() { Name = AppPermissions.BrandsManage, Group = "Catalog", Description = "Manage brands" },
            new() { Name = AppPermissions.OrdersRead, Group = "Orders", Description = "View orders" },
            new() { Name = AppPermissions.OrdersManage, Group = "Orders", Description = "Manage orders" },
            new() { Name = AppPermissions.OrdersCancel, Group = "Orders", Description = "Cancel orders" },
            new() { Name = AppPermissions.InventoryRead, Group = "Inventory", Description = "View inventory" },
            new() { Name = AppPermissions.InventoryManage, Group = "Inventory", Description = "Manage inventory" },
            new() { Name = AppPermissions.UsersRead, Group = "Users", Description = "View users" },
            new() { Name = AppPermissions.UsersManage, Group = "Users", Description = "Manage users" },
            new() { Name = AppPermissions.CouponsManage, Group = "Marketing", Description = "Manage coupons" },
            new() { Name = AppPermissions.ReviewsModerate, Group = "Reviews", Description = "Moderate reviews" },
            new() { Name = AppPermissions.ReportsView, Group = "Reports", Description = "View financial reports" },
        };

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();

        var adminRole = await context.Roles.FirstAsync(r => r.Name == AppRoles.Admin);
        var superAdminRole = await context.Roles.FirstAsync(r => r.Name == AppRoles.SuperAdmin);

        var allDbPermissions = await context.Permissions.ToListAsync();
        foreach (var perm in allDbPermissions)
        {
            context.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = perm.Id });
            context.RolePermissions.Add(new RolePermission { RoleId = superAdminRole.Id, PermissionId = perm.Id });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(ApplicationDbContext context)
    {
        var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@ecommerce.global");
        if (existingAdmin != null)
        {
            existingAdmin.PasswordHash = HashPassword("AdminPassword123!");
            await context.SaveChangesAsync();
            return;
        }

        var adminRole = await context.Roles.FirstAsync(r => r.Name == AppRoles.SuperAdmin);
        var customerRole = await context.Roles.FirstAsync(r => r.Name == AppRoles.Customer);

        var adminUser = new User
        {
            Email = "admin@ecommerce.global",
            NormalizedEmail = "ADMIN@ECOMMERCE.GLOBAL",
            UserName = "admin@ecommerce.global",
            NormalizedUserName = "ADMIN@ECOMMERCE.GLOBAL",
            FirstName = "Super",
            LastName = "Admin",
            EmailConfirmed = true,
            PasswordHash = HashPassword("AdminPassword123!"),
            CustomerProfile = new CustomerProfile
            {
                PreferredCurrency = "USD",
                PreferredLanguage = "en"
            }
        };

        var customerUser = new User
        {
            Email = "customer@ecommerce.global",
            NormalizedEmail = "CUSTOMER@ECOMMERCE.GLOBAL",
            UserName = "customer@ecommerce.global",
            NormalizedUserName = "CUSTOMER@ECOMMERCE.GLOBAL",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+923001234567",
            EmailConfirmed = true,
            PasswordHash = HashPassword("CustomerPassword123!"),
            CustomerProfile = new CustomerProfile
            {
                PreferredCurrency = "USD",
                PreferredLanguage = "en"
            },
            Addresses = new List<Address>
            {
                new()
                {
                    Type = AddressType.Both,
                    FirstName = "John",
                    LastName = "Doe",
                    StreetLine1 = "Main Commercial Boulevard, Gulberg III",
                    City = "Lahore",
                    StateOrProvince = "Punjab",
                    PostalCode = "54000",
                    CountryCode = "PK",
                    PhoneNumber = "+923001234567",
                    IsDefault = true
                }
            }
        };

        await context.Users.AddRangeAsync(adminUser, customerUser);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
        context.UserRoles.Add(new UserRole { UserId = customerUser.Id, RoleId = customerRole.Id });
        await context.SaveChangesAsync();
    }

    private static async Task SeedCurrenciesAndCountriesAsync(ApplicationDbContext context)
    {
        if (await context.Currencies.AnyAsync()) return;

        var currencies = new List<Currency>
        {
            new() { Code = "USD", Name = "US Dollar", Symbol = "$", ExchangeRateToBase = 1.0m, IsBaseCurrency = true },
            new() { Code = "PKR", Name = "Pakistani Rupee", Symbol = "Rs.", ExchangeRateToBase = 280.0m },
            new() { Code = "EUR", Name = "Euro", Symbol = "€", ExchangeRateToBase = 0.92m },
            new() { Code = "GBP", Name = "British Pound", Symbol = "£", ExchangeRateToBase = 0.79m },
            new() { Code = "AED", Name = "UAE Dirham", Symbol = "AED", ExchangeRateToBase = 3.67m },
            new() { Code = "SAR", Name = "Saudi Riyal", Symbol = "SAR", ExchangeRateToBase = 3.75m },
            new() { Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", ExchangeRateToBase = 1.36m },
            new() { Code = "AUD", Name = "Australian Dollar", Symbol = "A$", ExchangeRateToBase = 1.52m },
        };

        var countries = new List<Country>
        {
            new() { IsoCode = "PK", Name = "Pakistan", PhonePrefix = "+92", DefaultCurrency = "PKR" },
            new() { IsoCode = "US", Name = "United States", PhonePrefix = "+1", DefaultCurrency = "USD" },
            new() { IsoCode = "GB", Name = "United Kingdom", PhonePrefix = "+44", DefaultCurrency = "GBP" },
            new() { IsoCode = "AE", Name = "United Arab Emirates", PhonePrefix = "+971", DefaultCurrency = "AED" },
            new() { IsoCode = "SA", Name = "Saudi Arabia", PhonePrefix = "+966", DefaultCurrency = "SAR" },
            new() { IsoCode = "CA", Name = "Canada", PhonePrefix = "+1", DefaultCurrency = "CAD" },
            new() { IsoCode = "DE", Name = "Germany", PhonePrefix = "+49", DefaultCurrency = "EUR" },
        };

        await context.Currencies.AddRangeAsync(currencies);
        await context.Countries.AddRangeAsync(countries);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAndBrandsAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        var electronics = new Category
        {
            Name = "Electronics",
            Slug = "electronics",
            Description = "Premium electronics, gadgets and devices.",
            IsFeatured = true,
            DisplayOrder = 1,
            MetaTitle = "Electronics Store | Global E-Commerce",
            MetaDescription = "Buy smartphones, laptops, audio gear and accessories."
        };

        var fashion = new Category
        {
            Name = "Fashion",
            Slug = "fashion",
            Description = "Trending apparel, footwear and accessories.",
            IsFeatured = true,
            DisplayOrder = 2,
            MetaTitle = "Fashion & Apparel | Global E-Commerce",
            MetaDescription = "Shop luxury and everyday fashion collections."
        };

        var homeLiving = new Category
        {
            Name = "Home & Living",
            Slug = "home-living",
            Description = "Modern furniture, decor and home appliances.",
            IsFeatured = true,
            DisplayOrder = 3
        };

        await context.Categories.AddRangeAsync(electronics, fashion, homeLiving);
        await context.SaveChangesAsync();

        var brands = new List<Brand>
        {
            new() { Name = "Apple", Slug = "apple", Website = "https://apple.com", IsFeatured = true },
            new() { Name = "Samsung", Slug = "samsung", Website = "https://samsung.com", IsFeatured = true },
            new() { Name = "Nike", Slug = "nike", Website = "https://nike.com", IsFeatured = true },
            new() { Name = "Adidas", Slug = "adidas", Website = "https://adidas.com", IsFeatured = true },
            new() { Name = "Sony", Slug = "sony", Website = "https://sony.com", IsFeatured = true },
            new() { Name = "Dell", Slug = "dell", Website = "https://dell.com", IsFeatured = false },
        };

        await context.Brands.AddRangeAsync(brands);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAndVariantsAsync(ApplicationDbContext context)
    {
        if (await context.Products.AnyAsync()) return;

        var fashionCategory = await context.Categories.FirstAsync(c => c.Slug == "fashion");
        var electronicsCategory = await context.Categories.FirstAsync(c => c.Slug == "electronics");
        var nikeBrand = await context.Brands.FirstAsync(b => b.Slug == "nike");
        var appleBrand = await context.Brands.FirstAsync(b => b.Slug == "apple");
        var sonyBrand = await context.Brands.FirstAsync(b => b.Slug == "sony");

        // Product 1: Variable Product (T-Shirt with S/M/L/XL & Black/White/Blue)
        var tshirt = new Product
        {
            Name = "Signature Cotton Crewneck T-Shirt",
            Slug = "signature-cotton-crewneck-tshirt",
            SKU = "TSHIRT-SIG-001",
            Barcode = "8901234567890",
            Type = ProductType.Variable,
            ShortDescription = "Ultra-soft 100% combed organic cotton classic crewneck tee.",
            FullDescription = "Engineered for superior comfort and durability. Featuring reinforced stitching, pre-shrunk fabric, and an ergonomic tailored fit.",
            Price = 29.99m,
            SalePrice = 24.99m,
            CostPrice = 10.00m,
            Currency = "USD",
            CategoryId = fashionCategory.Id,
            BrandId = nikeBrand.Id,
            IsFeatured = true,
            IsNew = true,
            IsBestSeller = true,
            AverageRating = 4.8m,
            TotalReviews = 42,
            Images = new List<ProductImage>
            {
                new() { ImageUrl = "https://images.unsplash.com/photo-1521572267360-ee0c2909d518?w=800", AltText = "Signature Cotton Crewneck T-Shirt Front", IsMain = true, DisplayOrder = 1 }
            },
            Tags = new List<ProductTag>
            {
                new() { Tag = "t-shirt" }, new() { Tag = "cotton" }, new() { Tag = "casual" }, new() { Tag = "summer" }
            },
            Specifications = new List<ProductSpecification>
            {
                new() { Group = "Material", Name = "Fabric", Value = "100% Organic Combed Cotton" },
                new() { Group = "Care", Name = "Washing", Value = "Machine wash cold, tumble dry low" }
            }
        };

        // Create 4 variants for T-Shirt
        var variants = new List<ProductVariant>
        {
            new() { Product = tshirt, Name = "Small / Black", SKU = "TSHIRT-SIG-S-BLK", Size = "S", Color = "Black", ColorCode = "#000000", Price = 24.99m, Weight = 0.2m },
            new() { Product = tshirt, Name = "Medium / Black", SKU = "TSHIRT-SIG-M-BLK", Size = "M", Color = "Black", ColorCode = "#000000", Price = 24.99m, Weight = 0.22m },
            new() { Product = tshirt, Name = "Large / Black", SKU = "TSHIRT-SIG-L-BLK", Size = "L", Color = "Black", ColorCode = "#000000", Price = 24.99m, Weight = 0.25m },
            new() { Product = tshirt, Name = "Large / White", SKU = "TSHIRT-SIG-L-WHT", Size = "L", Color = "White", ColorCode = "#FFFFFF", Price = 24.99m, Weight = 0.25m },
            new() { Product = tshirt, Name = "Large / Navy Blue", SKU = "TSHIRT-SIG-L-BLU", Size = "L", Color = "Blue", ColorCode = "#000080", Price = 24.99m, Weight = 0.25m },
        };

        tshirt.Variants = variants;

        // Product 2: Smartphone
        var phone = new Product
        {
            Name = "Titanium Pro Smartphone 256GB",
            Slug = "titanium-pro-smartphone-256gb",
            SKU = "PHONE-TIT-256",
            Barcode = "8909876543210",
            Type = ProductType.Simple,
            ShortDescription = "Flagship smartphone featuring aerospace-grade titanium design.",
            FullDescription = "Powered by next-gen 3nm architecture, 120Hz OLED Super Retina display, 48MP Pro camera system with 5x optical zoom, and all-day battery life.",
            Price = 1199.00m,
            SalePrice = 1099.00m,
            CostPrice = 750.00m,
            Currency = "USD",
            CategoryId = electronicsCategory.Id,
            BrandId = appleBrand.Id,
            IsFeatured = true,
            IsBestSeller = true,
            AverageRating = 4.9m,
            TotalReviews = 128,
            Images = new List<ProductImage>
            {
                new() { ImageUrl = "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=800", AltText = "Titanium Pro Smartphone", IsMain = true, DisplayOrder = 1 }
            },
            Tags = new List<ProductTag>
            {
                new() { Tag = "smartphone" }, new() { Tag = "flagship" }, new() { Tag = "oled" }
            }
        };

        // Product 3: Headphones
        var headphones = new Product
        {
            Name = "Wireless Noise Cancelling Headphones",
            Slug = "wireless-noise-cancelling-headphones",
            SKU = "AUDIO-NC-PRO",
            Barcode = "8905556667778",
            Type = ProductType.Simple,
            ShortDescription = "Industry-leading active noise cancellation with hi-res audio.",
            FullDescription = "Experience crystal-clear sound with custom 40mm drivers, 30-hour battery life, speak-to-chat smart listening, and multipoint Bluetooth pairing.",
            Price = 349.99m,
            SalePrice = 299.99m,
            CostPrice = 180.00m,
            Currency = "USD",
            CategoryId = electronicsCategory.Id,
            BrandId = sonyBrand.Id,
            IsFeatured = true,
            AverageRating = 4.7m,
            TotalReviews = 65,
            Images = new List<ProductImage>
            {
                new() { ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800", AltText = "Wireless Noise Cancelling Headphones", IsMain = true, DisplayOrder = 1 }
            }
        };

        await context.Products.AddRangeAsync(tshirt, phone, headphones);
        await context.SaveChangesAsync();
    }

    private static async Task SeedWarehousesAndInventoryAsync(ApplicationDbContext context)
    {
        if (await context.Warehouses.AnyAsync()) return;

        var primaryWarehouse = new Warehouse
        {
            Name = "Fulfillment Center 1 (Pakistan Hub)",
            Code = "WH-PK-01",
            ContactPerson = "Warehouse Supervisor",
            PhoneNumber = "+924231112233",
            Email = "warehouse-pk@ecommerce.global",
            StreetLine1 = "Plot 42, Industrial Area, Kot Lakhpat",
            City = "Lahore",
            StateOrProvince = "Punjab",
            PostalCode = "54770",
            CountryCode = "PK",
            IsPrimary = true
        };

        var usWarehouse = new Warehouse
        {
            Name = "US Distribution Center",
            Code = "WH-US-01",
            ContactPerson = "US Logistics Lead",
            PhoneNumber = "+12145550199",
            Email = "warehouse-us@ecommerce.global",
            StreetLine1 = "100 Logistics Parkway, Suite 500",
            City = "Dallas",
            StateOrProvince = "TX",
            PostalCode = "75201",
            CountryCode = "US",
            IsPrimary = false
        };

        await context.Warehouses.AddRangeAsync(primaryWarehouse, usWarehouse);
        await context.SaveChangesAsync();

        // Seed Inventory for products
        var allProducts = await context.Products.Include(p => p.Variants).ToListAsync();
        foreach (var product in allProducts)
        {
            if (product.Variants.Any())
            {
                foreach (var variant in product.Variants)
                {
                    context.InventoryItems.Add(new InventoryItem
                    {
                        WarehouseId = primaryWarehouse.Id,
                        ProductId = product.Id,
                        ProductVariantId = variant.Id,
                        QuantityOnHand = 150,
                        QuantityReserved = 0,
                        ReorderPoint = 20,
                        ReorderQuantity = 100
                    });
                }
            }
            else
            {
                context.InventoryItems.Add(new InventoryItem
                {
                    WarehouseId = primaryWarehouse.Id,
                    ProductId = product.Id,
                    ProductVariantId = null,
                    QuantityOnHand = 75,
                    QuantityReserved = 0,
                    ReorderPoint = 15,
                    ReorderQuantity = 50
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedShippingAsync(ApplicationDbContext context)
    {
        if (await context.ShippingZones.AnyAsync()) return;

        var domesticZone = new ShippingZone
        {
            Name = "Domestic Pakistan",
            Description = "All provinces and major cities in Pakistan",
            CountryCodes = "PK"
        };

        var internationalZone = new ShippingZone
        {
            Name = "International Worldwide",
            Description = "North America, Europe, Middle East and Asia",
            CountryCodes = "US;GB;AE;SA;CA;DE"
        };

        await context.ShippingZones.AddRangeAsync(domesticZone, internationalZone);
        await context.SaveChangesAsync();

        var standardShipping = new ShippingMethod
        {
            Name = "Standard Shipping",
            Code = "STD",
            Description = "Cost-effective door-to-door ground courier delivery.",
            EstimatedDeliveryDays = "3-5 Business Days",
            DisplayOrder = 1
        };

        var expressShipping = new ShippingMethod
        {
            Name = "Express Courier Shipping",
            Code = "EXP",
            Description = "Fast priority air shipping with live tracking.",
            EstimatedDeliveryDays = "1-2 Business Days",
            DisplayOrder = 2
        };

        await context.ShippingMethods.AddRangeAsync(standardShipping, expressShipping);
        await context.SaveChangesAsync();

        // Rates
        var domesticStdRate = new ShippingRate
        {
            ShippingZoneId = domesticZone.Id,
            ShippingMethodId = standardShipping.Id,
            BaseRate = 5.00m,
            RatePerKg = 1.50m,
            FreeShippingThreshold = 50.00m,
            Currency = "USD"
        };

        var domesticExpRate = new ShippingRate
        {
            ShippingZoneId = domesticZone.Id,
            ShippingMethodId = expressShipping.Id,
            BaseRate = 12.00m,
            RatePerKg = 3.00m,
            FreeShippingThreshold = 150.00m,
            Currency = "USD"
        };

        var intlStdRate = new ShippingRate
        {
            ShippingZoneId = internationalZone.Id,
            ShippingMethodId = standardShipping.Id,
            BaseRate = 25.00m,
            RatePerKg = 5.00m,
            FreeShippingThreshold = 200.00m,
            Currency = "USD"
        };

        await context.ShippingRates.AddRangeAsync(domesticStdRate, domesticExpRate, intlStdRate);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCouponsAndTaxRulesAsync(ApplicationDbContext context)
    {
        if (await context.Coupons.AnyAsync()) return;

        var welcomeCoupon = new Coupon
        {
            Code = "WELCOME10",
            Description = "10% off on your first order!",
            DiscountType = DiscountType.Percentage,
            DiscountValue = 10.0m,
            MinimumOrderAmount = 25.0m,
            MaximumDiscountAmount = 50.0m,
            StartsAtUtc = DateTime.UtcNow.AddDays(-30),
            ExpiresAtUtc = DateTime.UtcNow.AddYears(1),
            UsageLimitTotal = 10000,
            UsageLimitPerCustomer = 1
        };

        var flashCoupon = new Coupon
        {
            Code = "FLASH20",
            Description = "$20 off on orders over $100",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 20.0m,
            MinimumOrderAmount = 100.0m,
            StartsAtUtc = DateTime.UtcNow.AddDays(-5),
            ExpiresAtUtc = DateTime.UtcNow.AddMonths(3),
            UsageLimitTotal = 500,
            UsageLimitPerCustomer = 2
        };

        await context.Coupons.AddRangeAsync(welcomeCoupon, flashCoupon);

        var taxRules = new List<TaxRule>
        {
            new() { Name = "PK Standard Sales Tax", CountryCode = "PK", Rate = 0.17m, Priority = 1 },
            new() { Name = "US Standard General Tax", CountryCode = "US", Rate = 0.08m, Priority = 1 },
            new() { Name = "UK VAT Standard", CountryCode = "GB", Rate = 0.20m, Priority = 1 },
            new() { Name = "UAE Standard VAT", CountryCode = "AE", Rate = 0.05m, Priority = 1 },
        };

        await context.TaxRules.AddRangeAsync(taxRules);
        await context.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        const string saltKey = "GlobalECommercePlatform_2026_SaltSecureKey!";
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(saltKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hash);
    }
}

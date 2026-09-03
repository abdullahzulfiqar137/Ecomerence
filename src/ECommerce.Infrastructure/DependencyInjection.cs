using System.Text;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services.Admin;
using ECommerce.Infrastructure.Services.Analytics;
using ECommerce.Infrastructure.Services.Authentication;
using ECommerce.Infrastructure.Services.Caching;
using ECommerce.Infrastructure.Services.Cart;
using ECommerce.Infrastructure.Services.Catalog;
using ECommerce.Infrastructure.Services.Checkout;
using ECommerce.Infrastructure.Services.Coupons;
using ECommerce.Infrastructure.Services.Idempotency;
using ECommerce.Infrastructure.Services.International;
using ECommerce.Infrastructure.Services.Inventory;
using ECommerce.Infrastructure.Services.Jobs;
using ECommerce.Infrastructure.Services.Notifications;
using ECommerce.Infrastructure.Services.Orders;
using ECommerce.Infrastructure.Services.Payments;
using ECommerce.Infrastructure.Services.Reviews;
using ECommerce.Infrastructure.Services.Search;
using ECommerce.Infrastructure.Services.Shipping;
using ECommerce.Infrastructure.Services.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=DESKTOP-O8LF2FG;Database=ECommerceDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

        // EF Core DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();

        // Authentication Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();

        // Catalog Services
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IProductAttributeService, ProductAttributeService>();
        services.AddScoped<IProductVariantService, ProductVariantService>();

        // Inventory Services
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IInventoryService, InventoryService>();

        // Cart and Wishlist Services
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IWishlistService, WishlistService>();

        // Orders Services & State Machine
        services.AddSingleton<IOrderStateMachine, OrderStateMachine>();
        services.AddScoped<IOrderService, OrderService>();

        // Checkout Pipeline Service
        services.AddScoped<ICheckoutService, CheckoutService>();

        // Payment Gateways & Services
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        services.AddScoped<IPaymentGateway, CashOnDeliveryGateway>();
        services.AddScoped<IPaymentGateway, JazzCashPaymentGateway>();
        services.AddScoped<IPaymentGateway, EasyPaisaPaymentGateway>();
        services.AddScoped<IPaymentGateway, PayPalPaymentGateway>();
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
        services.AddScoped<IPaymentService, PaymentService>();

        // Shipping Service
        services.AddScoped<IShippingService, ShippingService>();

        // Coupons & Promotions Service
        services.AddScoped<ICouponService, CouponService>();

        // International Currency & Regional Tax Services
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<ITaxService, TaxService>();

        // Product Reviews Service
        services.AddScoped<IReviewService, ReviewService>();

        // Notifications & Email Engine
        services.AddSingleton<IEmailTemplateEngine, EmailTemplateEngine>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<INotificationService, NotificationService>();

        // Admin Analytics & User Management Services
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IAdminUserService, AdminUserService>();

        // Search & Faceted Navigation Engine
        services.AddScoped<ISearchService, SearchService>();

        // Distributed Caching Layer
        services.AddDistributedMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();

        // Safe Request Idempotency Engine
        services.AddScoped<IIdempotencyService, IdempotencyService>();

        // File & Media Storage Engine
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // Background Jobs Engine & Automated Workers
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddHostedService<AutomatedMaintenanceBackgroundWorker>();

        // JWT Authentication Configuration
        var jwtSecret = configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForGlobalECommercePlatform2026MustBeAtLeast32BytesLong!";
        var jwtIssuer = configuration["JwtSettings:Issuer"] ?? "ECommerceAPI";
        var jwtAudience = configuration["JwtSettings:Audience"] ?? "ECommerceClients";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }
}

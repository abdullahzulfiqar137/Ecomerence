using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.DTOs.Analytics;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Data.Seed;
using ECommerce.Infrastructure.Services.Admin;
using ECommerce.Infrastructure.Services.Analytics;
using ECommerce.Infrastructure.Services.Orders;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class AnalyticsAndAdminTests
{
    private readonly ApplicationDbContext _context;
    private readonly AnalyticsService _analyticsService;
    private readonly AdminUserService _adminUserService;
    private readonly OrderService _orderService;

    public AnalyticsAndAdminTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        _analyticsService = new AnalyticsService(_context, NullLogger<AnalyticsService>.Instance);
        _adminUserService = new AdminUserService(_context, NullLogger<AdminUserService>.Instance);
        _orderService = new OrderService(_context, new OrderStateMachine(), NullLogger<OrderService>.Instance);
    }

    [Fact]
    public async Task GetDashboardKpi_And_SalesTrend_ShouldReturnAggregatedMetrics()
    {
        var user = await _context.Users.FirstAsync();
        var product = await _context.Products.FirstAsync();

        // Create an order
        await _orderService.CreateOrderAsync(user.Id, new CreateOrderDto
        {
            CustomerName = "Analytics Customer",
            CustomerEmail = user.Email,
            CustomerPhone = "+12025550199",
            ShippingAddress = new CreateOrderAddressDto
            {
                FirstName = "Analytics",
                LastName = "Tester",
                StreetLine1 = "100 Data St",
                City = "Seattle",
                CountryCode = "US"
            },
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 2 }
            }
        });

        // 1. Dashboard KPI
        var kpiRes = await _analyticsService.GetDashboardKpiAsync();
        kpiRes.Success.Should().BeTrue();
        kpiRes.Data.Should().NotBeNull();
        kpiRes.Data!.TotalOrders.Should().BeGreaterThan(0);
        kpiRes.Data.TotalRevenue.Should().BeGreaterThan(0);

        // 2. Sales Trend
        var trendRes = await _analyticsService.GetSalesTrendAsync(new SalesTrendRequestDto
        {
            GroupBy = "day"
        });
        trendRes.Success.Should().BeTrue();
        trendRes.Data.Should().NotBeEmpty();

        // 3. Top Products
        var topRes = await _analyticsService.GetTopSellingProductsAsync(5);
        topRes.Success.Should().BeTrue();
        topRes.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AdminUser_AssignRoles_And_UpdateStatus_ShouldModifyUser()
    {
        var user = await _context.Users.FirstAsync(u => u.Email == "customer@ecommerce.global");

        // 1. Assign Role: Admin
        var roleRes = await _adminUserService.AssignRolesAsync(user.Id, new AssignUserRolesDto
        {
            RoleNames = new List<string> { "Admin", "Customer" }
        });

        roleRes.Success.Should().BeTrue();
        roleRes.Data!.Roles.Should().Contain("Admin");

        // 2. Update Status: Deactivate
        var statusRes = await _adminUserService.UpdateUserStatusAsync(user.Id, new UpdateUserStatusDto
        {
            IsActive = false,
            Reason = "Administrative review"
        });

        statusRes.Success.Should().BeTrue();
        statusRes.Data!.IsActive.Should().BeFalse();
    }
}

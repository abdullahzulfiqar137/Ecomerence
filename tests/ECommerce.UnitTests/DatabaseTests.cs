using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Data.Seed;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class DatabaseTests
{
    [Fact]
    public async Task ApplicationDbContext_And_Seeder_ShouldSeedAllCoreEntities()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        await DatabaseSeeder.SeedAsync(context, NullLogger.Instance);

        // Verify Roles
        var rolesCount = await context.Roles.CountAsync();
        rolesCount.Should().Be(9);

        // Verify Users
        var usersCount = await context.Users.CountAsync();
        usersCount.Should().Be(2);

        // Verify Products
        var productsCount = await context.Products.CountAsync();
        productsCount.Should().Be(3);

        // Verify Variants
        var variantsCount = await context.ProductVariants.CountAsync();
        variantsCount.Should().Be(5);

        // Verify Currencies
        var currenciesCount = await context.Currencies.CountAsync();
        currenciesCount.Should().Be(8);

        // Verify Warehouses & Inventory
        var warehousesCount = await context.Warehouses.CountAsync();
        warehousesCount.Should().Be(2);

        var inventoryCount = await context.InventoryItems.CountAsync();
        inventoryCount.Should().BeGreaterThan(0);

        // Verify Coupons
        var couponsCount = await context.Coupons.CountAsync();
        couponsCount.Should().Be(2);
    }
}

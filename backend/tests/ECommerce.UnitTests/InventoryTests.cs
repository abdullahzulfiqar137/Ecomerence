using ECommerce.Application.DTOs.Inventory;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Data.Seed;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services.Inventory;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.UnitTests;

public class InventoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly WarehouseService _warehouseService;
    private readonly InventoryService _inventoryService;

    public InventoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        var warehouseRepo = new GenericRepository<Warehouse>(_context);

        _warehouseService = new WarehouseService(warehouseRepo, _context, NullLogger<WarehouseService>.Instance);
        _inventoryService = new InventoryService(_context, NullLogger<InventoryService>.Instance);
    }

    [Fact]
    public async Task GetAllWarehousesAsync_ShouldReturnSeededWarehouses()
    {
        var response = await _warehouseService.GetAllWarehousesAsync();

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Should().HaveCount(2); // Seeded PK and US hubs
    }

    [Fact]
    public async Task AdjustStockAsync_ShouldUpdateQuantityAndLogMovement()
    {
        var warehouse = await _context.Warehouses.FirstAsync();
        var product = await _context.Products.FirstAsync();

        var adjustRequest = new AdjustStockRequestDto
        {
            WarehouseId = warehouse.Id,
            ProductId = product.Id,
            NewQuantity = 200,
            Reason = "Annual Physical Inventory Audit"
        };

        var response = await _inventoryService.AdjustStockAsync(adjustRequest, "test-auditor");

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.QuantityOnHand.Should().Be(200);

        // Check movements
        var movements = await _context.StockMovements.Where(m => m.InventoryItemId == response.Data.Id).ToListAsync();
        movements.Should().NotBeEmpty();
        movements.Last().ReferenceType.Should().Be("StockAdjustment");
    }

    [Fact]
    public async Task Reserve_Release_And_Commit_Lifecycle_ShouldManageStockCorrectly()
    {
        var item = await _context.InventoryItems.FirstAsync(i => i.QuantityOnHand > 20);

        var initialOnHand = item.QuantityOnHand;
        var initialReserved = item.QuantityReserved;

        // 1. Reserve 5 units
        var reserveRequest = new CreateReservationRequestDto
        {
            WarehouseId = item.WarehouseId,
            ProductId = item.ProductId,
            ProductVariantId = item.ProductVariantId,
            Quantity = 5,
            HoldDurationMinutes = 15,
            CartSessionId = "session_xyz123"
        };

        var reserveResponse = await _inventoryService.ReserveStockAsync(reserveRequest);

        reserveResponse.Success.Should().BeTrue();
        reserveResponse.Data.Should().NotBeNull();
        reserveResponse.Data!.Quantity.Should().Be(5);

        // Refresh item from db
        item = await _context.InventoryItems.FindAsync(item.Id);
        item!.QuantityReserved.Should().Be(initialReserved + 5);

        // 2. Release reservation
        var releaseResponse = await _inventoryService.ReleaseReservationAsync(reserveResponse.Data.Id);
        releaseResponse.Success.Should().BeTrue();

        item = await _context.InventoryItems.FindAsync(item.Id);
        item!.QuantityReserved.Should().Be(initialReserved);

        // 3. New reservation for commit
        var reserve2 = await _inventoryService.ReserveStockAsync(reserveRequest);
        reserve2.Success.Should().BeTrue();

        // 4. Commit reservation upon order placement
        var commitResponse = await _inventoryService.CommitReservationAsync(reserve2.Data!.Id, "ORD-2026-0001");
        commitResponse.Success.Should().BeTrue();

        item = await _context.InventoryItems.FindAsync(item.Id);
        item!.QuantityOnHand.Should().Be(initialOnHand - 5);
        item.QuantityReserved.Should().Be(initialReserved);
    }
}

using ECommerce.Application.DTOs.Analytics;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(ApplicationDbContext context, ILogger<AnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<DashboardKpiDto>> GetDashboardKpiAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var startOfToday = now.Date;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var validOrders = _context.Orders.Where(o => o.Status != OrderStatus.Cancelled);

        var totalRev = await validOrders.SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;
        var todayRev = await validOrders.Where(o => o.CreatedAtUtc >= startOfToday).SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;
        var monthRev = await validOrders.Where(o => o.CreatedAtUtc >= startOfMonth).SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;

        var totalOrdersCount = await _context.Orders.CountAsync(cancellationToken);
        var todayOrdersCount = await _context.Orders.CountAsync(o => o.CreatedAtUtc >= startOfToday, cancellationToken);
        var pendingOrdersCount = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing, cancellationToken);

        var totalCustomers = await _context.Users.CountAsync(cancellationToken);
        var newCustomersToday = await _context.Users.CountAsync(u => u.CreatedAtUtc >= startOfToday, cancellationToken);

        var aov = totalOrdersCount > 0 ? Math.Round(totalRev / totalOrdersCount, 2) : 0m;

        var lowStockCount = await _context.InventoryItems.CountAsync(i => (i.QuantityOnHand - i.QuantityReserved) <= i.ReorderPoint, cancellationToken);

        var kpi = new DashboardKpiDto
        {
            TotalRevenue = Math.Round(totalRev, 2),
            TodayRevenue = Math.Round(todayRev, 2),
            MonthToDateRevenue = Math.Round(monthRev, 2),
            TotalOrders = totalOrdersCount,
            TodayOrders = todayOrdersCount,
            PendingOrders = pendingOrdersCount,
            TotalCustomers = totalCustomers,
            NewCustomersToday = newCustomersToday,
            AverageOrderValue = aov,
            LowStockItemsCount = lowStockCount
        };

        return ApiResponse<DashboardKpiDto>.SuccessResponse(kpi);
    }

    public async Task<ApiResponse<IReadOnlyList<SalesTrendDataPointDto>>> GetSalesTrendAsync(SalesTrendRequestDto request, CancellationToken cancellationToken = default)
    {
        var start = request.StartDateUtc ?? DateTime.UtcNow.AddDays(-30);
        var end = request.EndDateUtc ?? DateTime.UtcNow;

        var orders = await _context.Orders
            .Where(o => o.CreatedAtUtc >= start && o.CreatedAtUtc <= end && o.Status != OrderStatus.Cancelled)
            .Select(o => new { o.CreatedAtUtc, o.TotalAmount })
            .ToListAsync(cancellationToken);

        var groupBy = (request.GroupBy ?? "day").ToLowerInvariant();

        var grouped = orders
            .GroupBy(o => groupBy switch
            {
                "month" => o.CreatedAtUtc.ToString("yyyy-MM"),
                "year" => o.CreatedAtUtc.ToString("yyyy"),
                _ => o.CreatedAtUtc.ToString("yyyy-MM-dd")
            })
            .OrderBy(g => g.Key)
            .Select(g => new SalesTrendDataPointDto
            {
                Period = g.Key,
                Revenue = Math.Round(g.Sum(x => x.TotalAmount), 2),
                OrdersCount = g.Count()
            })
            .ToList();

        return ApiResponse<IReadOnlyList<SalesTrendDataPointDto>>.SuccessResponse(grouped);
    }

    public async Task<ApiResponse<IReadOnlyList<TopSellingProductDto>>> GetTopSellingProductsAsync(int top = 10, CancellationToken cancellationToken = default)
    {
        var items = await _context.OrderItems
            .Include(i => i.Order)
            .Where(i => i.Order != null && i.Order.Status != OrderStatus.Cancelled)
            .GroupBy(i => new { i.ProductId, i.ProductName, i.SKU })
            .Select(g => new TopSellingProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                SKU = g.Key.SKU,
                UnitsSold = g.Sum(x => x.Quantity),
                TotalRevenue = Math.Round(g.Sum(x => x.TotalLineAmount), 2)
            })
            .OrderByDescending(x => x.UnitsSold)
            .Take(top)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<TopSellingProductDto>>.SuccessResponse(items);
    }

    public async Task<ApiResponse<IReadOnlyList<OrderStatusDistributionDto>>> GetOrderStatusDistributionAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .GroupBy(o => o.Status)
            .Select(g => new OrderStatusDistributionDto
            {
                Status = g.Key,
                StatusName = g.Key.ToString(),
                Count = g.Count(),
                TotalValue = Math.Round(g.Sum(x => x.TotalAmount), 2)
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<OrderStatusDistributionDto>>.SuccessResponse(orders);
    }
}

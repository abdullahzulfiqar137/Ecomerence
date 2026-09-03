using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Analytics;

public class DashboardKpiDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal MonthToDateRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TodayOrders { get; set; }
    public int PendingOrders { get; set; }
    public int TotalCustomers { get; set; }
    public int NewCustomersToday { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int LowStockItemsCount { get; set; }
}

public class SalesTrendRequestDto
{
    public DateTime? StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public string GroupBy { get; set; } = "day"; // day, month, year
}

public class SalesTrendDataPointDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrdersCount { get; set; }
}

public class TopSellingProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class OrderStatusDistributionDto
{
    public OrderStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalValue { get; set; }
}

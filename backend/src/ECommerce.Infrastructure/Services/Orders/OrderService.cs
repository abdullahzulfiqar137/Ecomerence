using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Orders;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IOrderStateMachine _stateMachine;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        ApplicationDbContext context,
        IOrderStateMachine stateMachine,
        ILogger<OrderService> logger)
    {
        _context = context;
        _stateMachine = stateMachine;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<OrderDto>>> GetOrdersAsync(OrderFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Include(o => o.ShippingAddress)
            .Include(o => o.BillingAddress)
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.OrderNotes)
            .AsQueryable();

        if (filterParams.CustomerUserId.HasValue)
        {
            query = query.Where(o => o.UserId == filterParams.CustomerUserId.Value);
        }

        if (filterParams.Status.HasValue)
        {
            query = query.Where(o => o.Status == filterParams.Status.Value);
        }

        if (filterParams.PaymentStatus.HasValue)
        {
            query = query.Where(o => o.PaymentStatus == filterParams.PaymentStatus.Value);
        }

        if (filterParams.ShippingStatus.HasValue)
        {
            query = query.Where(o => o.ShippingStatus == filterParams.ShippingStatus.Value);
        }

        if (filterParams.FromDate.HasValue)
        {
            query = query.Where(o => o.CreatedAtUtc >= filterParams.FromDate.Value);
        }

        if (filterParams.ToDate.HasValue)
        {
            query = query.Where(o => o.CreatedAtUtc <= filterParams.ToDate.Value);
        }

        if (filterParams.MinTotal.HasValue)
        {
            query = query.Where(o => o.TotalAmount >= filterParams.MinTotal.Value);
        }

        if (filterParams.MaxTotal.HasValue)
        {
            query = query.Where(o => o.TotalAmount <= filterParams.MaxTotal.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
        {
            var search = filterParams.SearchTerm.Trim();
            query = query.Where(o =>
                o.OrderNumber.Contains(search) ||
                o.CustomerName.Contains(search) ||
                o.CustomerEmail.Contains(search) ||
                o.CustomerPhone.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((filterParams.Page - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = orders.Select(o => MapToOrderDto(o)).ToList();
        var pagedResult = new PagedResult<OrderDto>(dtos, totalCount, filterParams.Page, filterParams.PageSize);

        return ApiResponse<PagedResult<OrderDto>>.SuccessResponse(pagedResult);
    }

    public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(Guid orderId, Guid? requestingUserId = null, bool isAdmin = false, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.ShippingAddress)
            .Include(o => o.BillingAddress)
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.OrderNotes)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            return ApiResponse<OrderDto>.FailureResponse("Not Found", "Order not found.");
        }

        if (!isAdmin && requestingUserId.HasValue && order.UserId != requestingUserId.Value)
        {
            return ApiResponse<OrderDto>.FailureResponse("Forbidden", "You do not have access to this order.");
        }

        return ApiResponse<OrderDto>.SuccessResponse(MapToOrderDto(order, !isAdmin));
    }

    public async Task<ApiResponse<OrderDto>> GetOrderByNumberAsync(string orderNumber, Guid? requestingUserId = null, bool isAdmin = false, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.ShippingAddress)
            .Include(o => o.BillingAddress)
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.OrderNotes)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber.Trim(), cancellationToken);

        if (order == null)
        {
            return ApiResponse<OrderDto>.FailureResponse("Not Found", "Order not found.");
        }

        if (!isAdmin && requestingUserId.HasValue && order.UserId != requestingUserId.Value)
        {
            return ApiResponse<OrderDto>.FailureResponse("Forbidden", "You do not have access to this order.");
        }

        return ApiResponse<OrderDto>.SuccessResponse(MapToOrderDto(order, !isAdmin));
    }

    public async Task<ApiResponse<OrderDto>> CreateOrderAsync(Guid? userId, CreateOrderDto request, CancellationToken cancellationToken = default)
    {
        // Check idempotency if provided
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existingOrder = await _context.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                .Include(o => o.OrderNotes)
                .FirstOrDefaultAsync(o => o.IdempotencyKey == request.IdempotencyKey, cancellationToken);

            if (existingOrder != null)
            {
                return ApiResponse<OrderDto>.SuccessResponse(MapToOrderDto(existingOrder), "Order retrieved from previous submission.");
            }
        }

        // 1. Fetch products & variants for snapshot data
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var variantIds = request.Items.Where(i => i.ProductVariantId.HasValue).Select(i => i.ProductVariantId!.Value).Distinct().ToList();

        var products = await _context.Products
            .Include(p => p.Images)
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var variants = await _context.ProductVariants
            .Where(v => variantIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        // 2. Create Shipping Address
        var shippingAddress = new OrderAddress
        {
            FirstName = request.ShippingAddress.FirstName.Trim(),
            LastName = request.ShippingAddress.LastName.Trim(),
            Company = request.ShippingAddress.Company?.Trim(),
            PhoneNumber = request.ShippingAddress.PhoneNumber?.Trim(),
            StreetLine1 = request.ShippingAddress.StreetLine1.Trim(),
            StreetLine2 = request.ShippingAddress.StreetLine2?.Trim(),
            City = request.ShippingAddress.City.Trim(),
            StateOrProvince = request.ShippingAddress.StateOrProvince.Trim(),
            PostalCode = request.ShippingAddress.PostalCode.Trim(),
            CountryCode = request.ShippingAddress.CountryCode.Trim().ToUpperInvariant()
        };
        await _context.OrderAddresses.AddAsync(shippingAddress, cancellationToken);

        OrderAddress? billingAddress = null;
        if (request.BillingAddress != null)
        {
            billingAddress = new OrderAddress
            {
                FirstName = request.BillingAddress.FirstName.Trim(),
                LastName = request.BillingAddress.LastName.Trim(),
                Company = request.BillingAddress.Company?.Trim(),
                PhoneNumber = request.BillingAddress.PhoneNumber?.Trim(),
                StreetLine1 = request.BillingAddress.StreetLine1.Trim(),
                StreetLine2 = request.BillingAddress.StreetLine2?.Trim(),
                City = request.BillingAddress.City.Trim(),
                StateOrProvince = request.BillingAddress.StateOrProvince.Trim(),
                PostalCode = request.BillingAddress.PostalCode.Trim(),
                CountryCode = request.BillingAddress.CountryCode.Trim().ToUpperInvariant()
            };
            await _context.OrderAddresses.AddAsync(billingAddress, cancellationToken);
        }

        // 3. Generate Order Number
        var orderNumber = GenerateOrderNumber();

        // 4. Create Order Entity
        var order = new Order
        {
            OrderNumber = orderNumber,
            IdempotencyKey = request.IdempotencyKey,
            UserId = userId,
            CustomerName = request.CustomerName.Trim(),
            CustomerEmail = request.CustomerEmail.Trim(),
            CustomerPhone = request.CustomerPhone.Trim(),
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            ShippingStatus = ShipmentStatus.Pending,
            Currency = request.Currency,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            AppliedCouponCode = request.AppliedCouponCode,
            Notes = request.Notes,
            CreatedAtUtc = DateTime.UtcNow
        };

        decimal subtotal = 0m;

        // 5. Build immutable OrderItem snapshots
        foreach (var itemReq in request.Items)
        {
            if (!products.TryGetValue(itemReq.ProductId, out var product))
            {
                return ApiResponse<OrderDto>.FailureResponse("Bad Request", $"Product ID '{itemReq.ProductId}' not found.");
            }

            ProductVariant? variant = null;
            if (itemReq.ProductVariantId.HasValue && !variants.TryGetValue(itemReq.ProductVariantId.Value, out variant))
            {
                return ApiResponse<OrderDto>.FailureResponse("Bad Request", $"Product Variant ID '{itemReq.ProductVariantId}' not found.");
            }

            var unitPrice = variant?.SalePrice ?? variant?.Price ?? product.SalePrice ?? product.Price;
            var sku = variant?.SKU ?? product.SKU;
            var mainImage = variant?.ImageUrl
                ?? product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault(i => i.IsMain)?.ImageUrl
                ?? product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;

            var lineTotal = unitPrice * itemReq.Quantity;
            subtotal += lineTotal;

            var orderItem = new OrderItem
            {
                Order = order,
                ProductId = product.Id,
                ProductVariantId = variant?.Id,
                ProductName = product.Name,
                VariantName = variant?.Name,
                SKU = sku,
                ImageUrl = mainImage,
                Quantity = itemReq.Quantity,
                UnitPrice = unitPrice,
                DiscountAmount = 0m,
                TaxAmount = 0m,
                TotalLineAmount = lineTotal
            };

            await _context.OrderItems.AddAsync(orderItem, cancellationToken);
        }

        order.SubtotalAmount = subtotal;
        order.TotalAmount = subtotal; // Checkout pipeline will compute shipping/tax/coupon discounts

        // 6. Record Initial Status History
        var statusHistory = new OrderStatusHistory
        {
            Order = order,
            Status = OrderStatus.Pending,
            Comment = "Order placed successfully.",
            CreatedAtUtc = DateTime.UtcNow,
            ChangedBy = userId.HasValue ? "Customer" : "Guest"
        };
        await _context.OrderStatusHistories.AddAsync(statusHistory, cancellationToken);

        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created order {OrderNumber} for {CustomerEmail} with {ItemCount} items, total {Total}",
            order.OrderNumber, order.CustomerEmail, order.Items.Count, order.TotalAmount);

        return await GetOrderByIdAsync(order.Id, userId, isAdmin: true, cancellationToken);
    }

    public async Task<ApiResponse<OrderDto>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto request, string? changedBy = null, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.ShippingAddress)
            .Include(o => o.BillingAddress)
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.OrderNotes)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            return ApiResponse<OrderDto>.FailureResponse("Not Found", "Order not found.");
        }

        if (!_stateMachine.CanTransition(order.Status, request.NewStatus))
        {
            return ApiResponse<OrderDto>.FailureResponse("Invalid Transition",
                $"Cannot transition order from '{order.Status}' to '{request.NewStatus}'. Allowed transitions: {string.Join(", ", _stateMachine.GetAllowedTransitions(order.Status))}");
        }

        var oldStatus = order.Status;
        order.Status = request.NewStatus;

        var history = new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = request.NewStatus,
            Comment = request.Comment ?? $"Status changed from {oldStatus} to {request.NewStatus}",
            CreatedAtUtc = DateTime.UtcNow,
            ChangedBy = changedBy ?? "System"
        };

        await _context.OrderStatusHistories.AddAsync(history, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} status changed: {OldStatus} -> {NewStatus}", order.OrderNumber, oldStatus, request.NewStatus);

        return ApiResponse<OrderDto>.SuccessResponse(MapToOrderDto(order), "Order status updated successfully.");
    }

    public async Task<ApiResponse<OrderDto>> CancelOrderAsync(Guid orderId, CancelOrderRequestDto request, Guid? requestingUserId = null, bool isAdmin = false, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.ShippingAddress)
            .Include(o => o.BillingAddress)
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.OrderNotes)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            return ApiResponse<OrderDto>.FailureResponse("Not Found", "Order not found.");
        }

        if (!isAdmin && requestingUserId.HasValue && order.UserId != requestingUserId.Value)
        {
            return ApiResponse<OrderDto>.FailureResponse("Forbidden", "You do not have access to cancel this order.");
        }

        if (!_stateMachine.CanTransition(order.Status, OrderStatus.Cancelled))
        {
            return ApiResponse<OrderDto>.FailureResponse("Invalid Transition",
                $"Order in status '{order.Status}' cannot be cancelled.");
        }

        order.Status = OrderStatus.Cancelled;

        var history = new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = OrderStatus.Cancelled,
            Comment = $"Order cancelled: {request.Reason}",
            CreatedAtUtc = DateTime.UtcNow,
            ChangedBy = isAdmin ? "Admin" : "Customer"
        };

        await _context.OrderStatusHistories.AddAsync(history, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} cancelled. Reason: {Reason}", order.OrderNumber, request.Reason);

        return ApiResponse<OrderDto>.SuccessResponse(MapToOrderDto(order), "Order cancelled successfully.");
    }

    public async Task<ApiResponse<OrderNoteDto>> AddOrderNoteAsync(Guid orderId, AddOrderNoteDto request, string? authorName = null, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order == null)
        {
            return ApiResponse<OrderNoteDto>.FailureResponse("Not Found", "Order not found.");
        }

        var note = new OrderNote
        {
            OrderId = orderId,
            Note = request.Note.Trim(),
            IsCustomerVisible = request.IsCustomerVisible,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = authorName ?? "Staff"
        };

        await _context.OrderNotes.AddAsync(note, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new OrderNoteDto
        {
            Id = note.Id,
            Note = note.Note,
            IsCustomerVisible = note.IsCustomerVisible,
            CreatedAtUtc = note.CreatedAtUtc,
            CreatedBy = note.CreatedBy
        };

        return ApiResponse<OrderNoteDto>.SuccessResponse(dto, "Order note added successfully.");
    }

    private static string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomSuffix = Random.Shared.Next(100000, 999999);
        return $"ORD-{timestamp}-{randomSuffix}";
    }

    private static OrderDto MapToOrderDto(Order o, bool filterInternalNotes = false)
    {
        var notes = o.OrderNotes.AsEnumerable();
        if (filterInternalNotes)
        {
            notes = notes.Where(n => n.IsCustomerVisible);
        }

        return new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            IdempotencyKey = o.IdempotencyKey,
            UserId = o.UserId,
            CustomerName = o.CustomerName,
            CustomerEmail = o.CustomerEmail,
            CustomerPhone = o.CustomerPhone,
            Status = o.Status,
            PaymentStatus = o.PaymentStatus,
            ShippingStatus = o.ShippingStatus,
            Currency = o.Currency,
            ExchangeRate = o.ExchangeRate,
            SubtotalAmount = o.SubtotalAmount,
            DiscountAmount = o.DiscountAmount,
            TaxAmount = o.TaxAmount,
            ShippingAmount = o.ShippingAmount,
            TotalAmount = o.TotalAmount,
            AppliedCouponCode = o.AppliedCouponCode,
            Notes = o.Notes,
            CreatedAtUtc = o.CreatedAtUtc,
            ShippingAddress = new OrderAddressDto
            {
                Id = o.ShippingAddress?.Id ?? Guid.Empty,
                FirstName = o.ShippingAddress?.FirstName ?? string.Empty,
                LastName = o.ShippingAddress?.LastName ?? string.Empty,
                Company = o.ShippingAddress?.Company,
                PhoneNumber = o.ShippingAddress?.PhoneNumber,
                StreetLine1 = o.ShippingAddress?.StreetLine1 ?? string.Empty,
                StreetLine2 = o.ShippingAddress?.StreetLine2,
                City = o.ShippingAddress?.City ?? string.Empty,
                StateOrProvince = o.ShippingAddress?.StateOrProvince ?? string.Empty,
                PostalCode = o.ShippingAddress?.PostalCode ?? string.Empty,
                CountryCode = o.ShippingAddress?.CountryCode ?? string.Empty
            },
            BillingAddress = o.BillingAddress == null ? null : new OrderAddressDto
            {
                Id = o.BillingAddress.Id,
                FirstName = o.BillingAddress.FirstName,
                LastName = o.BillingAddress.LastName,
                Company = o.BillingAddress.Company,
                PhoneNumber = o.BillingAddress.PhoneNumber,
                StreetLine1 = o.BillingAddress.StreetLine1,
                StreetLine2 = o.BillingAddress.StreetLine2,
                City = o.BillingAddress.City,
                StateOrProvince = o.BillingAddress.StateOrProvince,
                PostalCode = o.BillingAddress.PostalCode,
                CountryCode = o.BillingAddress.CountryCode
            },
            Items = o.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductVariantId = i.ProductVariantId,
                ProductName = i.ProductName,
                VariantName = i.VariantName,
                SKU = i.SKU,
                ImageUrl = i.ImageUrl,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                DiscountAmount = i.DiscountAmount,
                TaxAmount = i.TaxAmount,
                TotalLineAmount = i.TotalLineAmount
            }).ToList(),
            StatusHistory = o.StatusHistory.OrderByDescending(h => h.CreatedAtUtc).Select(h => new OrderStatusHistoryDto
            {
                Id = h.Id,
                Status = h.Status,
                Comment = h.Comment,
                CreatedAtUtc = h.CreatedAtUtc,
                ChangedBy = h.ChangedBy
            }).ToList(),
            OrderNotes = notes.OrderByDescending(n => n.CreatedAtUtc).Select(n => new OrderNoteDto
            {
                Id = n.Id,
                Note = n.Note,
                IsCustomerVisible = n.IsCustomerVisible,
                CreatedAtUtc = n.CreatedAtUtc,
                CreatedBy = n.CreatedBy
            }).ToList()
        };
    }
}

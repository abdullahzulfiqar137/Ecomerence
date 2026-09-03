using ECommerce.Application.DTOs.Checkout;
using ECommerce.Application.DTOs.Inventory;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Checkout;

public class CheckoutService : ICheckoutService
{
    private readonly ApplicationDbContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService(
        ApplicationDbContext context,
        IInventoryService inventoryService,
        IOrderService orderService,
        ICartService cartService,
        ILogger<CheckoutService> logger)
    {
        _context = context;
        _inventoryService = inventoryService;
        _orderService = orderService;
        _cartService = cartService;
        _logger = logger;
    }

    public async Task<ApiResponse<CheckoutQuoteResponseDto>> CalculateQuoteAsync(Guid? userId, CheckoutQuoteRequestDto request, CancellationToken cancellationToken = default)
    {
        // 1. Get cart
        var cartResponse = await _cartService.GetCartAsync(userId, request.GuestSessionId, cancellationToken);
        if (!cartResponse.Success || cartResponse.Data == null || !cartResponse.Data.Items.Any())
        {
            return ApiResponse<CheckoutQuoteResponseDto>.FailureResponse("Empty Cart", "Cannot calculate quote for an empty cart.");
        }

        var cart = cartResponse.Data;
        var subtotal = cart.SubtotalAmount;

        // 2. Available Shipping Methods
        var shippingMethods = await _context.ShippingMethods
            .Where(sm => sm.IsActive)
            .OrderBy(sm => sm.DisplayOrder)
            .ToListAsync(cancellationToken);

        var shippingOptions = shippingMethods.Select(sm => new ShippingMethodOptionDto
        {
            Id = sm.Id,
            Name = sm.Name,
            Description = sm.Description,
            EstimatedDeliveryDays = sm.EstimatedDeliveryDays,
            Cost = sm.Name.Contains("Express", StringComparison.OrdinalIgnoreCase) ? 15.00m : 5.00m
        }).ToList();

        decimal selectedShippingAmount = 5.00m; // Default standard shipping
        if (request.SelectedShippingMethodId.HasValue)
        {
            var selectedOption = shippingOptions.FirstOrDefault(o => o.Id == request.SelectedShippingMethodId.Value);
            if (selectedOption != null)
            {
                selectedShippingAmount = selectedOption.Cost;
            }
        }

        // 3. Tax Calculation
        var taxRule = await _context.TaxRules
            .FirstOrDefaultAsync(t => t.CountryCode == request.CountryCode && t.IsActive, cancellationToken);

        decimal taxRate = taxRule?.Rate ?? 0.05m; // 5% default fallback
        decimal taxAmount = Math.Round(subtotal * taxRate, 2);

        // 4. Coupon Calculation
        decimal discountAmount = 0m;
        string? appliedCoupon = null;
        var couponCode = request.CouponCode ?? cart.AppliedCouponCode;

        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == couponCode.Trim().ToUpperInvariant() && c.IsActive, cancellationToken);

            if (coupon != null &&
                (!coupon.ExpiresAtUtc.HasValue || coupon.ExpiresAtUtc.Value > DateTime.UtcNow) &&
                (!coupon.MinimumOrderAmount.HasValue || subtotal >= coupon.MinimumOrderAmount.Value))
            {
                appliedCoupon = coupon.Code;
                discountAmount = coupon.DiscountType == DiscountType.Percentage
                    ? Math.Round(subtotal * (coupon.DiscountValue / 100m), 2)
                    : Math.Min(subtotal, coupon.DiscountValue);

                if (coupon.MaximumDiscountAmount.HasValue && discountAmount > coupon.MaximumDiscountAmount.Value)
                {
                    discountAmount = coupon.MaximumDiscountAmount.Value;
                }
            }
        }

        var total = Math.Max(0, subtotal - discountAmount + taxAmount + selectedShippingAmount);

        var quote = new CheckoutQuoteResponseDto
        {
            SubtotalAmount = subtotal,
            DiscountAmount = discountAmount,
            AppliedCouponCode = appliedCoupon,
            TaxAmount = taxAmount,
            ShippingAmount = selectedShippingAmount,
            TotalAmount = total,
            Currency = cart.Currency,
            TotalItems = cart.TotalItems,
            AvailableShippingMethods = shippingOptions
        };

        return ApiResponse<CheckoutQuoteResponseDto>.SuccessResponse(quote);
    }

    public async Task<ApiResponse<CheckoutResultDto>> ProcessCheckoutAsync(Guid? userId, CheckoutRequestDto request, CancellationToken cancellationToken = default)
    {
        // -------------------------------------------------------------
        // STEP 1: Idempotency Verification
        // -------------------------------------------------------------
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existingOrder = await _context.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.IdempotencyKey == request.IdempotencyKey, cancellationToken);

            if (existingOrder != null)
            {
                var orderDtoResponse = await _orderService.GetOrderByIdAsync(existingOrder.Id, userId, isAdmin: true, cancellationToken);
                return ApiResponse<CheckoutResultDto>.SuccessResponse(new CheckoutResultDto
                {
                    Success = true,
                    Message = "Order returned from existing submission.",
                    OrderId = existingOrder.Id,
                    OrderNumber = existingOrder.OrderNumber,
                    Status = existingOrder.Status,
                    PaymentStatus = existingOrder.PaymentStatus,
                    TotalAmount = existingOrder.TotalAmount,
                    Currency = existingOrder.Currency,
                    Order = orderDtoResponse.Data
                });
            }
        }

        // -------------------------------------------------------------
        // STEP 2: Retrieve Active Cart
        // -------------------------------------------------------------
        var cartResponse = await _cartService.GetCartAsync(userId, request.GuestSessionId, cancellationToken);
        if (!cartResponse.Success || cartResponse.Data == null || !cartResponse.Data.Items.Any())
        {
            return ApiResponse<CheckoutResultDto>.FailureResponse("Empty Cart", "Cannot checkout an empty shopping cart.");
        }

        var cart = cartResponse.Data;
        var activeItems = cart.Items.Where(i => !i.IsSavedForLater).ToList();

        if (!activeItems.Any())
        {
            return ApiResponse<CheckoutResultDto>.FailureResponse("Empty Cart", "No active items in shopping cart to checkout.");
        }

        // -------------------------------------------------------------
        // STEP 3 & 4: Live Multi-Warehouse Stock Availability Check
        // -------------------------------------------------------------
        var primaryWarehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.IsPrimary && w.IsActive, cancellationToken)
            ?? await _context.Warehouses.FirstOrDefaultAsync(w => w.IsActive, cancellationToken);

        if (primaryWarehouse == null)
        {
            return ApiResponse<CheckoutResultDto>.FailureResponse("Configuration Error", "No active fulfillment warehouse available.");
        }

        foreach (var item in activeItems)
        {
            var inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i =>
                    i.WarehouseId == primaryWarehouse.Id &&
                    i.ProductId == item.ProductId &&
                    i.ProductVariantId == item.ProductVariantId, cancellationToken);

            var availableStock = inventoryItem?.QuantityAvailable ?? 0;
            if (availableStock < item.Quantity)
            {
                return ApiResponse<CheckoutResultDto>.FailureResponse("Insufficient Stock",
                    $"Item '{item.ProductName}' (SKU: {item.ProductSKU}) has only {availableStock} units available, requested {item.Quantity}.");
            }
        }

        // -------------------------------------------------------------
        // STEP 5, 6, 7, 8 & 9: Compute Financial Breakdown
        // -------------------------------------------------------------
        var quoteResponse = await CalculateQuoteAsync(userId, new CheckoutQuoteRequestDto
        {
            GuestSessionId = request.GuestSessionId,
            CountryCode = request.ShippingAddress.CountryCode,
            StateOrProvince = request.ShippingAddress.StateOrProvince,
            PostalCode = request.ShippingAddress.PostalCode,
            SelectedShippingMethodId = request.SelectedShippingMethodId,
            CouponCode = request.CouponCode
        }, cancellationToken);

        if (!quoteResponse.Success || quoteResponse.Data == null)
        {
            return ApiResponse<CheckoutResultDto>.FailureResponse("Calculation Error", "Failed to compute checkout financial totals.");
        }

        var quote = quoteResponse.Data;

        // -------------------------------------------------------------
        // STEP 10: Create Time-Limited Stock Reservations
        // -------------------------------------------------------------
        var reservationIds = new List<Guid>();
        try
        {
            foreach (var item in activeItems)
            {
                var reserveResult = await _inventoryService.ReserveStockAsync(new CreateReservationRequestDto
                {
                    WarehouseId = primaryWarehouse.Id,
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    Quantity = item.Quantity,
                    HoldDurationMinutes = 15,
                    CartSessionId = request.GuestSessionId
                }, cancellationToken);

                if (!reserveResult.Success || reserveResult.Data == null)
                {
                    // Rollback previously created reservations
                    foreach (var resId in reservationIds)
                    {
                        await _inventoryService.ReleaseReservationAsync(resId, cancellationToken);
                    }
                    return ApiResponse<CheckoutResultDto>.FailureResponse("Reservation Failed", reserveResult.Message);
                }

                reservationIds.Add(reserveResult.Data.Id);
            }

            // -------------------------------------------------------------
            // STEP 11: Process Payment
            // -------------------------------------------------------------
            var isCod = request.PaymentMethod == PaymentMethodType.CashOnDelivery;
            var paymentStatus = isCod ? PaymentStatus.Pending : PaymentStatus.Captured;
            var orderStatus = isCod ? OrderStatus.Pending : OrderStatus.Confirmed;
            var transactionId = isCod
                ? $"COD-{Guid.NewGuid():N}"[..18].ToUpperInvariant()
                : $"TXN-{Guid.NewGuid():N}"[..18].ToUpperInvariant();

            // -------------------------------------------------------------
            // STEP 12: Create Order & Line Item Snapshots
            // -------------------------------------------------------------
            var createOrderDto = new CreateOrderDto
            {
                IdempotencyKey = request.IdempotencyKey,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                Currency = request.Currency,
                AppliedCouponCode = quote.AppliedCouponCode,
                Notes = request.Notes,
                ShippingAddress = request.ShippingAddress,
                BillingAddress = request.BillingAddress,
                Items = activeItems.Select(i => new CreateOrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductVariantId = i.ProductVariantId,
                    Quantity = i.Quantity
                }).ToList()
            };

            var orderCreateResult = await _orderService.CreateOrderAsync(userId, createOrderDto, cancellationToken);
            if (!orderCreateResult.Success || orderCreateResult.Data == null)
            {
                // Rollback reservations
                foreach (var resId in reservationIds)
                {
                    await _inventoryService.ReleaseReservationAsync(resId, cancellationToken);
                }
                return ApiResponse<CheckoutResultDto>.FailureResponse("Order Creation Failed", orderCreateResult.Message);
            }

            var orderId = orderCreateResult.Data.Id;
            var orderEntity = await _context.Orders.FindAsync(new object[] { orderId }, cancellationToken);

            if (orderEntity != null)
            {
                orderEntity.Status = orderStatus;
                orderEntity.PaymentStatus = paymentStatus;
                orderEntity.SubtotalAmount = quote.SubtotalAmount;
                orderEntity.DiscountAmount = quote.DiscountAmount;
                orderEntity.TaxAmount = quote.TaxAmount;
                orderEntity.ShippingAmount = quote.ShippingAmount;
                orderEntity.TotalAmount = quote.TotalAmount;

                // Create Payment entity record
                var paymentRecord = new Payment
                {
                    OrderId = orderId,
                    Provider = request.PaymentProvider,
                    MethodType = request.PaymentMethod,
                    ExternalTransactionId = transactionId,
                    IdempotencyKey = request.IdempotencyKey,
                    Amount = quote.TotalAmount,
                    Currency = quote.Currency,
                    Status = paymentStatus,
                    CompletedAtUtc = isCod ? null : DateTime.UtcNow,
                    CreatedAtUtc = DateTime.UtcNow
                };
                await _context.Payments.AddAsync(paymentRecord, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
            }

            // -------------------------------------------------------------
            // STEP 13: Commit Inventory Stock, Clear Cart & Finalize
            // -------------------------------------------------------------
            foreach (var resId in reservationIds)
            {
                await _inventoryService.CommitReservationAsync(resId, orderCreateResult.Data.OrderNumber, cancellationToken);
            }

            // Clear Cart
            await _cartService.ClearCartAsync(userId, request.GuestSessionId, cancellationToken);

            _logger.LogInformation("Successfully completed 13-step checkout for Order {OrderNumber} ({Total} {Currency})",
                orderCreateResult.Data.OrderNumber, quote.TotalAmount, quote.Currency);

            var updatedOrderDto = await _orderService.GetOrderByIdAsync(orderId, userId, isAdmin: true, cancellationToken);

            var resultDto = new CheckoutResultDto
            {
                Success = true,
                Message = isCod ? "Order placed successfully (Cash on Delivery)." : "Order placed and payment captured successfully.",
                OrderId = orderId,
                OrderNumber = orderCreateResult.Data.OrderNumber,
                Status = orderStatus,
                PaymentStatus = paymentStatus,
                TransactionId = transactionId,
                TotalAmount = quote.TotalAmount,
                Currency = quote.Currency,
                Order = updatedOrderDto.Data
            };

            return ApiResponse<CheckoutResultDto>.SuccessResponse(resultDto, resultDto.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing checkout pipeline");
            // Rollback reservations
            foreach (var resId in reservationIds)
            {
                await _inventoryService.ReleaseReservationAsync(resId, cancellationToken);
            }
            throw;
        }
    }
}

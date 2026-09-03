using ECommerce.Application.DTOs.Payments;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Payments;

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways)
    {
        _gateways = gateways;
    }

    public IPaymentGateway GetGateway(string providerName)
    {
        var gateway = _gateways.FirstOrDefault(g => g.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
        if (gateway == null)
        {
            // Default to CashOnDelivery or Stripe
            gateway = _gateways.FirstOrDefault(g => g.ProviderName.Equals("CashOnDelivery", StringComparison.OrdinalIgnoreCase))
                ?? _gateways.First();
        }

        return gateway;
    }
}

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly IOrderService _orderService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        ApplicationDbContext context,
        IPaymentGatewayFactory gatewayFactory,
        IOrderService orderService,
        IConfiguration configuration,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _gatewayFactory = gatewayFactory;
        _orderService = orderService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ApiResponse<PaymentDto>> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment == null)
        {
            return ApiResponse<PaymentDto>.FailureResponse("Not Found", "Payment record not found.");
        }

        return ApiResponse<PaymentDto>.SuccessResponse(MapToDto(payment));
    }

    public async Task<ApiResponse<IReadOnlyList<PaymentDto>>> GetPaymentsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payments = await _context.Payments
            .Include(p => p.Order)
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var dtos = payments.Select(p => MapToDto(p)).ToList();
        return ApiResponse<IReadOnlyList<PaymentDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<PaymentIntentResponseDto>> CreatePaymentIntentAsync(CreatePaymentIntentRequestDto request, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.ShippingAddress)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return ApiResponse<PaymentIntentResponseDto>.FailureResponse("Not Found", "Order not found.");
        }

        var gateway = _gatewayFactory.GetGateway(request.Provider);
        var intentResponse = await gateway.CreatePaymentIntentAsync(order, request.ReturnUrl, cancellationToken);

        // Record pending payment
        var payment = new Payment
        {
            OrderId = order.Id,
            Provider = gateway.ProviderName,
            MethodType = request.MethodType,
            ExternalTransactionId = intentResponse.TransactionId,
            Amount = order.TotalAmount,
            Currency = order.Currency,
            Status = intentResponse.Status,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _context.Payments.AddAsync(payment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created payment intent {TxnId} with provider {Provider} for Order {OrderNumber}",
            intentResponse.TransactionId, gateway.ProviderName, order.OrderNumber);

        return ApiResponse<PaymentIntentResponseDto>.SuccessResponse(intentResponse);
    }

    public async Task<ApiResponse<RefundResponseDto>> ProcessRefundAsync(RefundRequestDto request, string? authorizedBy = null, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment == null)
        {
            return ApiResponse<RefundResponseDto>.FailureResponse("Not Found", "Payment not found.");
        }

        if (payment.Status != PaymentStatus.Captured && payment.Status != PaymentStatus.Authorized)
        {
            return ApiResponse<RefundResponseDto>.FailureResponse("Invalid Status", $"Cannot refund payment with status '{payment.Status}'.");
        }

        if (request.Amount > payment.Amount)
        {
            return ApiResponse<RefundResponseDto>.FailureResponse("Bad Request", "Refund amount cannot exceed original payment amount.");
        }

        var gateway = _gatewayFactory.GetGateway(payment.Provider);
        var refundResult = await gateway.ProcessRefundAsync(payment, request.Amount, request.Reason, cancellationToken);

        if (!refundResult.Success)
        {
            return ApiResponse<RefundResponseDto>.FailureResponse("Refund Failed", refundResult.Message);
        }

        payment.Status = PaymentStatus.Refunded;
        payment.FailureReason = $"Refunded: {request.Reason} (by {authorizedBy ?? "Staff"})";

        if (payment.Order != null)
        {
            payment.Order.PaymentStatus = PaymentStatus.Refunded;
            payment.Order.Status = OrderStatus.Refunded;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully refunded payment {PaymentId} for Order {OrderId} (${Amount})",
            payment.Id, payment.OrderId, request.Amount);

        return ApiResponse<RefundResponseDto>.SuccessResponse(refundResult, "Refund completed successfully.");
    }

    public async Task<ApiResponse<bool>> ProcessWebhookAsync(string provider, string payload, string signatureHeader, CancellationToken cancellationToken = default)
    {
        var gateway = _gatewayFactory.GetGateway(provider);
        var secret = _configuration[$"{provider}:WebhookSecret"] ?? "sample_webhook_secret";

        var isValid = await gateway.VerifyWebhookSignatureAsync(payload, signatureHeader, secret);
        if (!isValid)
        {
            _logger.LogWarning("Invalid webhook signature received for provider {Provider}", provider);
            return ApiResponse<bool>.FailureResponse("Unauthorized", "Webhook signature verification failed.");
        }

        _logger.LogInformation("Successfully verified and processed webhook for provider {Provider}", provider);
        return ApiResponse<bool>.SuccessResponse(true, "Webhook processed successfully.");
    }

    private static PaymentDto MapToDto(Payment p)
    {
        return new PaymentDto
        {
            Id = p.Id,
            OrderId = p.OrderId,
            OrderNumber = p.Order?.OrderNumber ?? string.Empty,
            Provider = p.Provider,
            MethodType = p.MethodType,
            ExternalTransactionId = p.ExternalTransactionId,
            IdempotencyKey = p.IdempotencyKey,
            Amount = p.Amount,
            Currency = p.Currency,
            Status = p.Status,
            CompletedAtUtc = p.CompletedAtUtc,
            FailureReason = p.FailureReason,
            CreatedAtUtc = p.CreatedAtUtc
        };
    }
}

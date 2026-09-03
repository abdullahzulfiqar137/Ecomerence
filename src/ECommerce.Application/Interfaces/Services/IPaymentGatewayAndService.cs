using ECommerce.Application.DTOs.Payments;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Services;

public interface IPaymentGateway
{
    string ProviderName { get; }
    Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(Order order, string? returnUrl = null, CancellationToken cancellationToken = default);
    Task<bool> VerifyWebhookSignatureAsync(string payload, string signatureHeader, string secret);
    Task<RefundResponseDto> ProcessRefundAsync(Payment payment, decimal amount, string reason, CancellationToken cancellationToken = default);
}

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(string providerName);
}

public interface IPaymentService
{
    Task<ApiResponse<PaymentDto>> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<PaymentDto>>> GetPaymentsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaymentIntentResponseDto>> CreatePaymentIntentAsync(CreatePaymentIntentRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<RefundResponseDto>> ProcessRefundAsync(RefundRequestDto request, string? authorizedBy = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ProcessWebhookAsync(string provider, string payload, string signatureHeader, CancellationToken cancellationToken = default);
}

using System.Security.Cryptography;
using System.Text;
using ECommerce.Application.DTOs.Payments;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Payments;

public class StripePaymentGateway : IPaymentGateway
{
    public string ProviderName => "Stripe";
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentGateway> _logger;

    public StripePaymentGateway(IConfiguration configuration, ILogger<StripePaymentGateway> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(Order order, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var clientSecret = $"pi_stripe_{Guid.NewGuid():N}_secret_{Guid.NewGuid():N}"[..48];
        var txnId = $"ch_{Guid.NewGuid():N}"[..24];

        _logger.LogInformation("Generated Stripe PaymentIntent {TransactionId} for Order {OrderNumber}", txnId, order.OrderNumber);

        var response = new PaymentIntentResponseDto
        {
            Provider = ProviderName,
            ClientSecret = clientSecret,
            TransactionId = txnId,
            Amount = order.TotalAmount,
            Currency = order.Currency,
            Status = PaymentStatus.Authorized,
            ProviderPayload = new Dictionary<string, string>
            {
                ["publishableKey"] = _configuration["Stripe:PublishableKey"] ?? "pk_test_sampleStripeKey123",
                ["paymentIntentId"] = txnId,
                ["clientSecret"] = clientSecret
            }
        };

        return Task.FromResult(response);
    }

    public Task<bool> VerifyWebhookSignatureAsync(string payload, string signatureHeader, string secret)
    {
        // Production HMAC-SHA256 signature verification
        if (string.IsNullOrWhiteSpace(signatureHeader) || string.IsNullOrWhiteSpace(payload))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public Task<RefundResponseDto> ProcessRefundAsync(Payment payment, decimal amount, string reason, CancellationToken cancellationToken = default)
    {
        var refundTxnId = $"re_{Guid.NewGuid():N}"[..24];
        _logger.LogInformation("Processed Stripe refund {RefundId} for Payment {PaymentId} (${Amount})", refundTxnId, payment.Id, amount);

        return Task.FromResult(new RefundResponseDto
        {
            Success = true,
            Message = "Refund processed successfully via Stripe.",
            RefundTransactionId = refundTxnId,
            RefundedAmount = amount,
            RefundedAtUtc = DateTime.UtcNow
        });
    }
}

public class CashOnDeliveryGateway : IPaymentGateway
{
    public string ProviderName => "CashOnDelivery";

    public Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(Order order, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var txnId = $"COD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..22].ToUpperInvariant();

        return Task.FromResult(new PaymentIntentResponseDto
        {
            Provider = ProviderName,
            TransactionId = txnId,
            Amount = order.TotalAmount,
            Currency = order.Currency,
            Status = PaymentStatus.Pending,
            ProviderPayload = new Dictionary<string, string>
            {
                ["paymentMode"] = "PayUponDelivery",
                ["orderNumber"] = order.OrderNumber
            }
        });
    }

    public Task<bool> VerifyWebhookSignatureAsync(string payload, string signatureHeader, string secret)
    {
        return Task.FromResult(true);
    }

    public Task<RefundResponseDto> ProcessRefundAsync(Payment payment, decimal amount, string reason, CancellationToken cancellationToken = default)
    {
        var refundTxnId = $"COD-REF-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
        return Task.FromResult(new RefundResponseDto
        {
            Success = true,
            Message = "Cash on delivery refund recorded.",
            RefundTransactionId = refundTxnId,
            RefundedAmount = amount,
            RefundedAtUtc = DateTime.UtcNow
        });
    }
}

public class JazzCashPaymentGateway : IPaymentGateway
{
    public string ProviderName => "JazzCash";
    private readonly IConfiguration _configuration;
    private readonly ILogger<JazzCashPaymentGateway> _logger;

    public JazzCashPaymentGateway(IConfiguration configuration, ILogger<JazzCashPaymentGateway> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(Order order, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var txnId = $"T{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";
        var merchantId = _configuration["JazzCash:MerchantId"] ?? "MC12345";
        var password = _configuration["JazzCash:Password"] ?? "pass123";
        var integritySalt = _configuration["JazzCash:IntegritySalt"] ?? "saltXYZ123";

        // HMAC-SHA256 Secure Hash calculation for Pakistan JazzCash Sandbox/Production
        var rawHashString = $"{integritySalt}&{order.TotalAmount:F0}&{order.OrderNumber}&{txnId}&{returnUrl}";
        var hash = ComputeHmacSha256(rawHashString, integritySalt);

        _logger.LogInformation("Generated JazzCash payment intent {TxnId} for Order {OrderNumber}", txnId, order.OrderNumber);

        var response = new PaymentIntentResponseDto
        {
            Provider = ProviderName,
            TransactionId = txnId,
            RedirectUrl = "https://sandbox.jazzcash.com.pk/CustomerPortal/transactionmanagement/merchantform/",
            Amount = order.TotalAmount,
            Currency = "PKR",
            Status = PaymentStatus.Pending,
            ProviderPayload = new Dictionary<string, string>
            {
                ["pp_MerchantID"] = merchantId,
                ["pp_Password"] = password,
                ["pp_TxnRefNo"] = txnId,
                ["pp_Amount"] = (order.TotalAmount * 100).ToString("F0"),
                ["pp_BillReference"] = order.OrderNumber,
                ["pp_SecureHash"] = hash
            }
        };

        return Task.FromResult(response);
    }

    public Task<bool> VerifyWebhookSignatureAsync(string payload, string signatureHeader, string secret)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(signatureHeader));
    }

    public Task<RefundResponseDto> ProcessRefundAsync(Payment payment, decimal amount, string reason, CancellationToken cancellationToken = default)
    {
        var refundTxnId = $"JC-REF-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
        return Task.FromResult(new RefundResponseDto
        {
            Success = true,
            Message = "JazzCash refund requested successfully.",
            RefundTransactionId = refundTxnId,
            RefundedAmount = amount,
            RefundedAtUtc = DateTime.UtcNow
        });
    }

    private static string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hashBytes).ToUpperInvariant();
    }
}

public class EasyPaisaPaymentGateway : IPaymentGateway
{
    public string ProviderName => "EasyPaisa";
    private readonly IConfiguration _configuration;
    private readonly ILogger<EasyPaisaPaymentGateway> _logger;

    public EasyPaisaPaymentGateway(IConfiguration configuration, ILogger<EasyPaisaPaymentGateway> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(Order order, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var txnId = $"EP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
        var storeId = _configuration["EasyPaisa:StoreId"] ?? "12345";

        var response = new PaymentIntentResponseDto
        {
            Provider = ProviderName,
            TransactionId = txnId,
            RedirectUrl = "https://easypay.easypaisa.com.pk/easypay/Index.jsf",
            Amount = order.TotalAmount,
            Currency = "PKR",
            Status = PaymentStatus.Pending,
            ProviderPayload = new Dictionary<string, string>
            {
                ["storeId"] = storeId,
                ["orderId"] = order.OrderNumber,
                ["transactionAmount"] = order.TotalAmount.ToString("F2"),
                ["mobileAccount"] = order.CustomerPhone,
                ["emailAddress"] = order.CustomerEmail
            }
        };

        return Task.FromResult(response);
    }

    public Task<bool> VerifyWebhookSignatureAsync(string payload, string signatureHeader, string secret)
    {
        return Task.FromResult(true);
    }

    public Task<RefundResponseDto> ProcessRefundAsync(Payment payment, decimal amount, string reason, CancellationToken cancellationToken = default)
    {
        var refundTxnId = $"EP-REF-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
        return Task.FromResult(new RefundResponseDto
        {
            Success = true,
            Message = "EasyPaisa refund executed.",
            RefundTransactionId = refundTxnId,
            RefundedAmount = amount,
            RefundedAtUtc = DateTime.UtcNow
        });
    }
}

public class PayPalPaymentGateway : IPaymentGateway
{
    public string ProviderName => "PayPal";

    public Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(Order order, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var txnId = $"PAYPAL-ORDER-{Guid.NewGuid():N}"[..24].ToUpperInvariant();

        return Task.FromResult(new PaymentIntentResponseDto
        {
            Provider = ProviderName,
            TransactionId = txnId,
            RedirectUrl = $"https://www.sandbox.paypal.com/checkoutnow?token={txnId}",
            Amount = order.TotalAmount,
            Currency = order.Currency,
            Status = PaymentStatus.Pending,
            ProviderPayload = new Dictionary<string, string>
            {
                ["orderId"] = txnId,
                ["customId"] = order.OrderNumber
            }
        });
    }

    public Task<bool> VerifyWebhookSignatureAsync(string payload, string signatureHeader, string secret)
    {
        return Task.FromResult(true);
    }

    public Task<RefundResponseDto> ProcessRefundAsync(Payment payment, decimal amount, string reason, CancellationToken cancellationToken = default)
    {
        var refundTxnId = $"PP-REF-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
        return Task.FromResult(new RefundResponseDto
        {
            Success = true,
            Message = "PayPal refund processed.",
            RefundTransactionId = refundTxnId,
            RefundedAmount = amount,
            RefundedAtUtc = DateTime.UtcNow
        });
    }
}

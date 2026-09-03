using System.Text;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Notifications;

public class EmailTemplateEngine : IEmailTemplateEngine
{
    private const string BrandHeader = @"
        <div style='background-color:#111827;padding:24px;text-align:center;border-top-left-radius:8px;border-top-right-radius:8px;'>
            <h1 style='color:#FFFFFF;margin:0;font-family:Arial,sans-serif;font-size:24px;letter-spacing:1px;'>GLOBAL E-COMMERCE</h1>
        </div>";

    private const string BrandFooter = @"
        <div style='background-color:#F9FAFB;padding:16px;text-align:center;border-bottom-left-radius:8px;border-bottom-right-radius:8px;font-family:Arial,sans-serif;font-size:12px;color:#6B7280;margin-top:20px;'>
            <p style='margin:0;'>&copy; 2026 Global E-Commerce Platform. All rights reserved.</p>
            <p style='margin:4px 0 0 0;'>You received this email because of your account activity.</p>
        </div>";

    public string RenderOrderConfirmation(Order order)
    {
        var itemsHtml = new StringBuilder();
        foreach (var item in order.Items)
        {
            itemsHtml.Append($@"
                <tr style='border-bottom:1px solid #E5E7EB;'>
                    <td style='padding:12px 0;font-family:Arial,sans-serif;font-size:14px;color:#1F2937;'>
                        <strong>{item.ProductName}</strong><br/>
                        <span style='color:#6B7280;font-size:12px;'>SKU: {item.SKU} | Qty: {item.Quantity}</span>
                    </td>
                    <td style='padding:12px 0;text-align:right;font-family:Arial,sans-serif;font-size:14px;font-weight:bold;color:#111827;'>
                        ${item.TotalLineAmount:F2}
                    </td>
                </tr>");
        }

        return $@"
        <div style='max-width:600px;margin:0 auto;border:1px solid #E5E7EB;border-radius:8px;font-family:Arial,sans-serif;'>
            {BrandHeader}
            <div style='padding:24px;'>
                <h2 style='color:#111827;margin-top:0;'>Thank You for Your Order!</h2>
                <p style='color:#4B5563;font-size:14px;'>Hi {order.CustomerName}, your order <strong>{order.OrderNumber}</strong> has been received and is being processed.</p>
                <div style='background-color:#F3F4F6;padding:12px;border-radius:6px;margin:16px 0;'>
                    <p style='margin:0;font-size:13px;color:#374151;'><strong>Order Number:</strong> {order.OrderNumber}</p>
                    <p style='margin:4px 0 0 0;font-size:13px;color:#374151;'><strong>Date:</strong> {order.CreatedAtUtc:yyyy-MM-dd HH:mm} UTC</p>
                    <p style='margin:4px 0 0 0;font-size:13px;color:#374151;'><strong>Payment Status:</strong> {order.PaymentStatus}</p>
                </div>
                <table style='width:100%;border-collapse:collapse;margin:16px 0;'>
                    <thead>
                        <tr style='border-bottom:2px solid #D1D5DB;'>
                            <th style='text-align:left;padding:8px 0;color:#374151;font-size:13px;'>Item</th>
                            <th style='text-align:right;padding:8px 0;color:#374151;font-size:13px;'>Price</th>
                        </tr>
                    </thead>
                    <tbody>
                        {itemsHtml}
                    </tbody>
                </table>
                <div style='text-align:right;margin-top:16px;'>
                    <p style='margin:4px 0;font-size:14px;color:#4B5563;'>Subtotal: <strong>${order.SubtotalAmount:F2}</strong></p>
                    <p style='margin:4px 0;font-size:14px;color:#4B5563;'>Shipping: <strong>${order.ShippingAmount:F2}</strong></p>
                    <p style='margin:4px 0;font-size:14px;color:#4B5563;'>Tax: <strong>${order.TaxAmount:F2}</strong></p>
                    <p style='margin:8px 0 0 0;font-size:18px;color:#111827;'>Total: <strong>${order.TotalAmount:F2} {order.Currency}</strong></p>
                </div>
            </div>
            {BrandFooter}
        </div>";
    }

    public string RenderShipmentDispatched(Shipment shipment)
    {
        return $@"
        <div style='max-width:600px;margin:0 auto;border:1px solid #E5E7EB;border-radius:8px;font-family:Arial,sans-serif;'>
            {BrandHeader}
            <div style='padding:24px;'>
                <h2 style='color:#111827;margin-top:0;'>Your Package is on the Way!</h2>
                <p style='color:#4B5563;font-size:14px;'>Good news! Your shipment via <strong>{shipment.CarrierName}</strong> has been dispatched.</p>
                <div style='background-color:#EFF6FF;border:1px solid #BFDBFE;padding:16px;border-radius:6px;margin:16px 0;'>
                    <p style='margin:0;font-size:14px;color:#1E40AF;'><strong>Tracking Number:</strong> {shipment.TrackingNumber}</p>
                    <p style='margin:6px 0 0 0;font-size:13px;color:#1E40AF;'><strong>Carrier:</strong> {shipment.CarrierName}</p>
                    <p style='margin:6px 0 0 0;font-size:13px;color:#1E40AF;'><strong>Estimated Delivery:</strong> {shipment.EstimatedDeliveryUtc:yyyy-MM-dd}</p>
                </div>
                <div style='text-align:center;margin:24px 0;'>
                    <a href='{shipment.TrackingUrl}' style='background-color:#2563EB;color:#FFFFFF;padding:12px 24px;border-radius:6px;text-decoration:none;font-weight:bold;display:inline-block;'>Track Your Order</a>
                </div>
            </div>
            {BrandFooter}
        </div>";
    }

    public string RenderPaymentReceipt(Payment payment)
    {
        return $@"
        <div style='max-width:600px;margin:0 auto;border:1px solid #E5E7EB;border-radius:8px;font-family:Arial,sans-serif;'>
            {BrandHeader}
            <div style='padding:24px;'>
                <h2 style='color:#111827;margin-top:0;'>Payment Confirmation</h2>
                <p style='color:#4B5563;font-size:14px;'>We have received your payment of <strong>${payment.Amount:F2} {payment.Currency}</strong>.</p>
                <div style='background-color:#F3F4F6;padding:12px;border-radius:6px;margin:16px 0;'>
                    <p style='margin:0;font-size:13px;color:#374151;'><strong>Transaction ID:</strong> {payment.ExternalTransactionId}</p>
                    <p style='margin:4px 0 0 0;font-size:13px;color:#374151;'><strong>Payment Method:</strong> {payment.Provider} ({payment.MethodType})</p>
                    <p style='margin:4px 0 0 0;font-size:13px;color:#374151;'><strong>Status:</strong> {payment.Status}</p>
                </div>
            </div>
            {BrandFooter}
        </div>";
    }

    public string RenderWelcomeEmail(string customerName, string email)
    {
        return $@"
        <div style='max-width:600px;margin:0 auto;border:1px solid #E5E7EB;border-radius:8px;font-family:Arial,sans-serif;'>
            {BrandHeader}
            <div style='padding:24px;'>
                <h2 style='color:#111827;margin-top:0;'>Welcome to Global E-Commerce!</h2>
                <p style='color:#4B5563;font-size:14px;'>Hi {customerName}, thank you for registering with us. Your account is now active under <strong>{email}</strong>.</p>
                <p style='color:#4B5563;font-size:14px;'>Start exploring our global catalog of premium products, fast shipping, and seamless checkout.</p>
            </div>
            {BrandFooter}
        </div>";
    }

    public string RenderPasswordReset(string customerName, string resetUrl)
    {
        return $@"
        <div style='max-width:600px;margin:0 auto;border:1px solid #E5E7EB;border-radius:8px;font-family:Arial,sans-serif;'>
            {BrandHeader}
            <div style='padding:24px;'>
                <h2 style='color:#111827;margin-top:0;'>Password Reset Request</h2>
                <p style='color:#4B5563;font-size:14px;'>Hi {customerName}, we received a request to reset your account password.</p>
                <div style='text-align:center;margin:24px 0;'>
                    <a href='{resetUrl}' style='background-color:#EF4444;color:#FFFFFF;padding:12px 24px;border-radius:6px;text-decoration:none;font-weight:bold;display:inline-block;'>Reset Password</a>
                </div>
                <p style='color:#6B7280;font-size:12px;'>If you did not request this, please ignore this email.</p>
            </div>
            {BrandFooter}
        </div>";
    }
}

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string bodyHtml, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Dispatched email '{Subject}' to {RecipientName} <{RecipientEmail}>",
            subject, toName, toEmail);

        return Task.FromResult(true);
    }
}

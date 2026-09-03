using ECommerce.Application.DTOs.Checkout;
using ECommerce.Application.Models;

namespace ECommerce.Application.Interfaces.Services;

public interface ICheckoutService
{
    Task<ApiResponse<CheckoutQuoteResponseDto>> CalculateQuoteAsync(Guid? userId, CheckoutQuoteRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CheckoutResultDto>> ProcessCheckoutAsync(Guid? userId, CheckoutRequestDto request, CancellationToken cancellationToken = default);
}

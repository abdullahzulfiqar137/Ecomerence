using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Models;

namespace ECommerce.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> LogoutAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> VerifyEmailAsync(VerifyEmailRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

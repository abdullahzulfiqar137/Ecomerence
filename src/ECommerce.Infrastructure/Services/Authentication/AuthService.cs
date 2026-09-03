using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Authentication;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ApplicationDbContext context,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var isEmailUnique = await _userRepository.IsEmailUniqueAsync(request.Email, null, cancellationToken);
        if (!isEmailUnique)
        {
            return ApiResponse<AuthResponseDto>.FailureResponse("Registration failed", "An account with this email already exists.");
        }

        var user = new User
        {
            Email = request.Email.Trim(),
            NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
            UserName = request.Email.Trim(),
            NormalizedUserName = request.Email.Trim().ToUpperInvariant(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            EmailConfirmed = true, // Auto-confirm for dev; email token workflow supported
            CustomerProfile = new CustomerProfile
            {
                PreferredCurrency = "USD",
                PreferredLanguage = "en"
            }
        };

        var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == AppRoles.Customer, cancellationToken);
        if (customerRole != null)
        {
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = customerRole.Id });
        }

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var roles = new List<string> { AppRoles.Customer };
        var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions, out var expiresAt, out var jwtId);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id, jwtId, ipAddress);

        await _userRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New user registered: {Email}", user.Email);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAtUtc = expiresAt,
            User = MapToUserDto(user, roles, permissions)
        };

        return ApiResponse<AuthResponseDto>.SuccessResponse(response, "User registered successfully.");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || !user.IsActive)
        {
            return ApiResponse<AuthResponseDto>.FailureResponse("Authentication failed", "Invalid email or password.");
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            user.AccessFailedCount++;
            await _userRepository.SaveChangesAsync(cancellationToken);
            return ApiResponse<AuthResponseDto>.FailureResponse("Authentication failed", "Invalid email or password.");
        }

        user.AccessFailedCount = 0;

        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
        var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions, out var expiresAt, out var jwtId);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id, jwtId, ipAddress);

        await _userRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User logged in: {Email}", user.Email);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAtUtc = expiresAt,
            User = MapToUserDto(user, roles, permissions)
        };

        return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Login successful.");
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (tokenEntity == null || !tokenEntity.IsActive || tokenEntity.User == null)
        {
            return ApiResponse<AuthResponseDto>.FailureResponse("Invalid refresh token", "Refresh token is invalid or has expired.");
        }

        var user = tokenEntity.User;
        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
        var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);

        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions, out var expiresAt, out var jwtId);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id, jwtId, ipAddress);

        // Token rotation: Revoke old token and assign replacement
        tokenEntity.RevokedAtUtc = DateTime.UtcNow;
        tokenEntity.RevokedByIp = ipAddress;
        tokenEntity.ReplacedByToken = newRefreshToken.Token;
        tokenEntity.ReasonRevoked = "Replaced by new token rotation";

        await _userRepository.AddRefreshTokenAsync(newRefreshToken, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAtUtc = expiresAt,
            User = MapToUserDto(user, roles, permissions)
        };

        return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Token refreshed successfully.");
    }

    public async Task<ApiResponse<bool>> LogoutAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        await _userRepository.RevokeRefreshTokenAsync(refreshToken, null, "User logged out", cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Logged out successfully.");
    }

    public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            // Return success anyway to avoid user enumeration
            return ApiResponse<bool>.SuccessResponse(true, "If an account exists with this email, a reset password link has been sent.");
        }

        _logger.LogInformation("Password reset requested for {Email}", user.Email);
        return ApiResponse<bool>.SuccessResponse(true, "If an account exists with this email, a reset password link has been sent.");
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            return ApiResponse<bool>.FailureResponse("Password reset failed", "Invalid reset request.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString();

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password reset completed for {Email}", user.Email);
        return ApiResponse<bool>.SuccessResponse(true, "Password has been reset successfully. You can now login with your new password.");
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return ApiResponse<bool>.FailureResponse("Failed", "User not found.");
        }

        var isOldPasswordValid = _passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash);
        if (!isOldPasswordValid)
        {
            return ApiResponse<bool>.FailureResponse("Failed", "Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString();

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully.");
    }

    public async Task<ApiResponse<bool>> VerifyEmailAsync(VerifyEmailRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            return ApiResponse<bool>.FailureResponse("Verification failed", "User not found.");
        }

        user.EmailConfirmed = true;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Email verified successfully.");
    }

    public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return ApiResponse<UserDto>.FailureResponse("Not found", "User not found.");
        }

        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
        var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);

        return ApiResponse<UserDto>.SuccessResponse(MapToUserDto(user, roles, permissions));
    }

    private static UserDto MapToUserDto(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            Roles = roles.ToList(),
            Permissions = permissions.ToList()
        };
    }
}

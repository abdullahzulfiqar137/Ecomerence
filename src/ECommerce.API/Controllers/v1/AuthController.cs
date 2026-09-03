using Asp.Versioning;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[EnableRateLimiting("AuthPolicy")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<RefreshTokenRequestDto> _refreshTokenValidator;
    private readonly IValidator<ForgotPasswordRequestDto> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordRequestDto> _resetPasswordValidator;
    private readonly IValidator<ChangePasswordRequestDto> _changePasswordValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<RefreshTokenRequestDto> refreshTokenValidator,
        IValidator<ForgotPasswordRequestDto> forgotPasswordValidator,
        IValidator<ResetPasswordRequestDto> resetPasswordValidator,
        IValidator<ChangePasswordRequestDto> changePasswordValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshTokenValidator = refreshTokenValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _authService.RegisterAsync(request, GetClientIpAddress(), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _authService.LoginAsync(request, GetClientIpAddress(), cancellationToken);
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _refreshTokenValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _authService.RefreshTokenAsync(request, GetClientIpAddress(), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.LogoutAsync(request.RefreshToken, GetClientIpAddress(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _forgotPasswordValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<bool>.FailureResponse("Validation failed", errors));
        }

        var result = await _authService.ForgotPasswordAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _resetPasswordValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<bool>.FailureResponse("Validation failed", errors));
        }

        var result = await _authService.ResetPasswordAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized(ApiResponse<bool>.FailureResponse("Unauthorized", "User is not authenticated."));
        }

        var validationResult = await _changePasswordValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<bool>.FailureResponse("Validation failed", errors));
        }

        var result = await _authService.ChangePasswordAsync(CurrentUserId.Value, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyEmailAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (!CurrentUserId.HasValue)
        {
            return Unauthorized(ApiResponse<UserDto>.FailureResponse("Unauthorized", "User is not authenticated."));
        }

        var result = await _authService.GetCurrentUserAsync(CurrentUserId.Value, cancellationToken);
        return Ok(result);
    }
}

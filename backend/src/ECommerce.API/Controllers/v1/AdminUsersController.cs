using Asp.Versioning;
using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/users")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class AdminUsersController : BaseApiController
{
    private readonly IAdminUserService _adminUserService;
    private readonly IValidator<AssignUserRolesDto> _rolesValidator;
    private readonly IValidator<UpdateUserStatusDto> _statusValidator;

    public AdminUsersController(
        IAdminUserService adminUserService,
        IValidator<AssignUserRolesDto> rolesValidator,
        IValidator<UpdateUserStatusDto> statusValidator)
    {
        _adminUserService = adminUserService;
        _rolesValidator = rolesValidator;
        _statusValidator = statusValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AdminUserListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] string? search = null, [FromQuery] string? role = null, [FromQuery] bool? isActive = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _adminUserService.GetUsersAsync(search, role, isActive, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDetailsDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserDetails(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.GetUserDetailsAsync(id, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}/roles")]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDetailsDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDetailsDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignUserRolesDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _rolesValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<AdminUserDetailsDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _adminUserService.AssignRolesAsync(id, request, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDetailsDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDetailsDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateUserStatusDto request, CancellationToken cancellationToken)
    {
        var validationResult = await _statusValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<AdminUserDetailsDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _adminUserService.UpdateUserStatusAsync(id, request, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}

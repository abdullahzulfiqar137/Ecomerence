using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Admin;

public class AdminUserService : IAdminUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminUserService> _logger;

    public AdminUserService(ApplicationDbContext context, ILogger<AdminUserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<AdminUserListDto>>> GetUsersAsync(string? search = null, string? role = null, bool? isActive = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.Email.ToLower().Contains(s) ||
                                     u.FirstName.ToLower().Contains(s) ||
                                     u.LastName.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var r = role.Trim();
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == r));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderByDescending(u => u.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = users.Select(u => new AdminUserListDto
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            PhoneNumber = u.PhoneNumber ?? string.Empty,
            IsActive = u.IsActive,
            EmailConfirmed = u.EmailConfirmed,
            Roles = u.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Name).ToList(),
            CreatedAtUtc = u.CreatedAtUtc,
            LastModifiedAtUtc = u.LastModifiedAtUtc
        }).ToList();

        var pagedResult = new PagedResult<AdminUserListDto>(dtos, total, page, pageSize);
        return ApiResponse<PagedResult<AdminUserListDto>>.SuccessResponse(pagedResult);
    }

    public async Task<ApiResponse<AdminUserDetailsDto>> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return ApiResponse<AdminUserDetailsDto>.FailureResponse("Not Found", "User not found.");
        }

        var orders = await _context.Orders
            .Where(o => o.UserId == userId && o.Status != OrderStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var totalOrders = orders.Count;
        var totalSpent = orders.Sum(o => o.TotalAmount);
        var lastOrderDate = orders.OrderByDescending(o => o.CreatedAtUtc).Select(o => (DateTime?)o.CreatedAtUtc).FirstOrDefault();

        var dto = new AdminUserDetailsDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            Roles = user.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Name).ToList(),
            CreatedAtUtc = user.CreatedAtUtc,
            LastModifiedAtUtc = user.LastModifiedAtUtc,
            TotalOrdersCount = totalOrders,
            LifetimeSpent = Math.Round(totalSpent, 2),
            LastOrderDateUtc = lastOrderDate
        };

        return ApiResponse<AdminUserDetailsDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<AdminUserDetailsDto>> AssignRolesAsync(Guid userId, AssignUserRolesDto request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return ApiResponse<AdminUserDetailsDto>.FailureResponse("Not Found", "User not found.");
        }

        // Fetch valid roles
        var validRoles = await _context.Roles
            .Where(r => request.RoleNames.Contains(r.Name))
            .ToListAsync(cancellationToken);

        // Remove old roles
        _context.UserRoles.RemoveRange(user.UserRoles);

        // Add new roles
        foreach (var r in validRoles)
        {
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = r.Id });
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Assigned roles [{Roles}] to user {UserId}", string.Join(", ", request.RoleNames), userId);

        return await GetUserDetailsAsync(userId, cancellationToken);
    }

    public async Task<ApiResponse<AdminUserDetailsDto>> UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            return ApiResponse<AdminUserDetailsDto>.FailureResponse("Not Found", "User not found.");
        }

        user.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated user {UserId} active status to {IsActive}. Reason: {Reason}",
            userId, request.IsActive, request.Reason ?? "N/A");

        return await GetUserDetailsAsync(userId, cancellationToken);
    }
}

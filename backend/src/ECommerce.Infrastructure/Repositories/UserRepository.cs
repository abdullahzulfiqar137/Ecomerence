using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.CustomerProfile)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToUpperInvariant();
        return await _context.Users
            .Include(u => u.CustomerProfile)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u!.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        return tokenEntity?.User;
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToUpperInvariant();
        return !await _context.Users
            .AnyAsync(u => u.NormalizedEmail == normalizedEmail && (!excludeUserId.HasValue || u.Id != excludeUserId.Value), cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId && ur.Role != null)
            .Select(ur => ur.Role!.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        return await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId) && rp.Permission != null)
            .Select(rp => rp.Permission!.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(string token, string? replacedByToken = null, string? reason = null, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        if (tokenEntity != null)
        {
            tokenEntity.RevokedAtUtc = DateTime.UtcNow;
            tokenEntity.ReplacedByToken = replacedByToken;
            tokenEntity.ReasonRevoked = reason;
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

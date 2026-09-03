using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Services;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions, out DateTime expiresAtUtc, out string jwtId);
    RefreshToken GenerateRefreshToken(Guid userId, string jwtId, string? ipAddress = null);
}

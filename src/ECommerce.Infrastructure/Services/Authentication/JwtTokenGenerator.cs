using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure.Services.Authentication;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions, out DateTime expiresAtUtc, out string jwtId)
    {
        var secret = _configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForGlobalECommercePlatform2026MustBeAtLeast32BytesLong!";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "ECommerceAPI";
        var audience = _configuration["JwtSettings:Audience"] ?? "ECommerceClients";
        var expiryMinutes = int.TryParse(_configuration["JwtSettings:ExpiryMinutes"], out var minutes) ? minutes : 60;

        expiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes);
        jwtId = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    public RefreshToken GenerateRefreshToken(Guid userId, string jwtId, string? ipAddress = null)
    {
        var expiryDays = int.TryParse(_configuration["JwtSettings:RefreshTokenExpiryDays"], out var days) ? days : 7;

        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(randomNumber),
            JwtId = jwtId,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }
}

using System.Security.Cryptography;
using System.Text;
using ECommerce.Application.Interfaces.Services;

namespace ECommerce.Infrastructure.Services.Authentication;

public class PasswordHasher : IPasswordHasher
{
    private const string SaltKey = "GlobalECommercePlatform_2026_SaltSecureKey!";

    public string HashPassword(string password)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(SaltKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hash);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var computedHash = HashPassword(password);
        return computedHash == passwordHash;
    }
}

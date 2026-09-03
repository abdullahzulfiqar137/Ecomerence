using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class IdempotentRequest : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public bool IsProcessing { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; } = DateTime.UtcNow.AddHours(24);
}

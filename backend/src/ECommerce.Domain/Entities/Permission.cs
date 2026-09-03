using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

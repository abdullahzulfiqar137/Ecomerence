namespace ECommerce.Application.DTOs.Admin;

public class AdminUserListDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
}

public class AdminUserDetailsDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }

    // Customer Metrics
    public int TotalOrdersCount { get; set; }
    public decimal LifetimeSpent { get; set; }
    public DateTime? LastOrderDateUtc { get; set; }
}

public class AssignUserRolesDto
{
    public List<string> RoleNames { get; set; } = new();
}

public class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
    public string? Reason { get; set; }
}

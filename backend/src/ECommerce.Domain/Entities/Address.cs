using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Address : AuditableEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public AddressType Type { get; set; } = AddressType.Shipping;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string StreetLine1 { get; set; } = string.Empty;
    public string? StreetLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string StateOrProvince { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsDefault { get; set; } = false;
}

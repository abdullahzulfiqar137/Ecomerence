namespace ECommerce.Domain.ValueObjects;

public record AddressSnapshot(
    string FirstName,
    string LastName,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string StateOrProvince,
    string PostalCode,
    string CountryCode,
    string? PhoneNumber);

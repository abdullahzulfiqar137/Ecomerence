namespace ECommerce.Domain.Constants;

public static class Currencies
{
    public const string USD = "USD";
    public const string EUR = "EUR";
    public const string GBP = "GBP";
    public const string PKR = "PKR";
    public const string AED = "AED";
    public const string SAR = "SAR";
    public const string CAD = "CAD";
    public const string AUD = "AUD";

    public static readonly IReadOnlyList<string> Supported = new[]
    {
        USD, EUR, GBP, PKR, AED, SAR, CAD, AUD
    };
}

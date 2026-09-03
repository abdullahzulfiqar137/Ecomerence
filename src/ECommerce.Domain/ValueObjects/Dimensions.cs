namespace ECommerce.Domain.ValueObjects;

public record Dimensions(decimal Length, decimal Width, decimal Height, string Unit = "cm")
{
    public decimal Volume => Length * Width * Height;
}

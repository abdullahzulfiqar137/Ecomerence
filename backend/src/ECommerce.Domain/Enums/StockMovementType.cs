namespace ECommerce.Domain.Enums;

public enum StockMovementType
{
    PurchaseInflow = 1,
    OrderOutflow = 2,
    Reservation = 3,
    Release = 4,
    ManualAdjustment = 5,
    CustomerReturn = 6,
    DamagedOrLost = 7,
    Transfer = 8
}

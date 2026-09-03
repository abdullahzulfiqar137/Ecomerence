namespace ECommerce.Domain.Entities;

public class PromotionCategory
{
    public Guid PromotionId { get; set; }
    public Promotion? Promotion { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
}

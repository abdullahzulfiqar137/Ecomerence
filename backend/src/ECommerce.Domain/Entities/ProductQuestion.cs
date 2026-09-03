using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ProductQuestion : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string QuestionText { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = true;

    public ICollection<ProductAnswer> Answers { get; set; } = new List<ProductAnswer>();
}

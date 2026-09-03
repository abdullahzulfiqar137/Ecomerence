using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ProductAnswer : AuditableEntity
{
    public Guid QuestionId { get; set; }
    public ProductQuestion? Question { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string AnswerText { get; set; } = string.Empty;
    public bool IsOfficialAnswer { get; set; } = false;
    public bool IsApproved { get; set; } = true;
}

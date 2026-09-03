namespace ECommerce.Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with identifier '({key})' was not found.")
    {
    }
}

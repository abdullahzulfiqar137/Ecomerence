namespace ECommerce.Domain.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Unauthorized access.") : base(message)
    {
    }
}

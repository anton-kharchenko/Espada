namespace Espada.Domain.Rules;

public static class DomainErrors
{
    public static readonly DomainError ObjectNotFound = new("ObjectNotFound", "The object was not found.");
}

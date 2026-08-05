namespace Espada.Domain.Rules
{
    public sealed record DomainError(string Code, string Description)
    {
        public static DomainError None { get; } = new(string.Empty, string.Empty);
    }
}
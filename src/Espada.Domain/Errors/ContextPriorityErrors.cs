using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Errors
{
    public static class ContextPriorityErrors
    {
        public static readonly DomainError OutOfRange = new("ContextPriority.OutOfRange",
            $"Context priority must be between {ContextPriority.Minimum} and {ContextPriority.Maximum}.");
    }
}
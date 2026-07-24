using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Errors
{
    public static class SourceErrors
    {
        public static readonly DomainError NameEmpty = new("Source.Name.Empty", "Source name cannot be empty.");

        public static readonly DomainError NameTooLong = new("Source.Name.TooLong", $"Source name cannot exceed {SourceName.MaxLength} characters.");

        public static readonly DomainError LocatorEmpty = new("Source.Locator.Empty", "Source locator cannot be empty.");

        public static readonly DomainError LocatorTooLong = new("Source.Locator.TooLong", $"Source locator cannot exceed {SourceLocator.MaxLength} characters.");

        public static readonly DomainError AlreadyArchived = new("Source.AlreadyArchived", "Source is already archived.");
    }
}
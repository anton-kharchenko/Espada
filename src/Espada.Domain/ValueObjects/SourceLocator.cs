using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class SourceLocator : ValueObject
    {
        public const int MaxLength = 2048;

        private SourceLocator(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static DomainResult<SourceLocator> Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DomainResult<SourceLocator>.Failure(SourceErrors.LocatorEmpty);
            }

            string normalized = value.Trim();

            return normalized.Length > MaxLength
                ? DomainResult<SourceLocator>.Failure(SourceErrors.LocatorTooLong)
                : DomainResult<SourceLocator>.Success(new SourceLocator(normalized));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
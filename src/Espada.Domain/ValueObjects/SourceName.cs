using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class SourceName : ValueObject
    {
        public const int MaxLength = 200;

        private SourceName(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static DomainResult<SourceName> Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DomainResult<SourceName>.Failure(SourceErrors.NameEmpty);
            }

            string normalized = value.Trim();

            return normalized.Length > MaxLength
                ? DomainResult<SourceName>.Failure(SourceErrors.NameTooLong)
                : DomainResult<SourceName>.Success(new SourceName(normalized));
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
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class RuleKey : ValueObject
    {
        public const int MaxLength = 100;

        private RuleKey(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static DomainResult<RuleKey> Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DomainResult<RuleKey>.Failure(RuleErrors.KeyEmpty);
            }

            string normalized = value.Trim();
            if (normalized.Length > MaxLength)
            {
                return DomainResult<RuleKey>.Failure(RuleErrors.KeyTooLong);
            }

            if (normalized.Any(character =>
                    !char.IsLetterOrDigit(character) && character is not '.' and not '_' and not '-'))
            {
                return DomainResult<RuleKey>.Failure(RuleErrors.KeyInvalid);
            }

            return DomainResult<RuleKey>.Success(new RuleKey(normalized));
        }

        public override string ToString()
        {
            return Value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
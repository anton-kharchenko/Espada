using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class ContextPriority : ValueObject
    {
        public const int Minimum = -100;
        public const int Maximum = 100;

        private ContextPriority(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public static ContextPriority Neutral { get; } = new(0);

        public static DomainResult<ContextPriority> Create(int value)
        {
            return value is < Minimum or > Maximum
                ? DomainResult<ContextPriority>.Failure(ContextPriorityErrors.OutOfRange)
                : DomainResult<ContextPriority>.Success(new ContextPriority(value));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
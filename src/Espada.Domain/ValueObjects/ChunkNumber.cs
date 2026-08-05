using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class ChunkNumber : ValueObject
    {
        private ChunkNumber(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public static ChunkNumber First()
        {
            return new ChunkNumber(1);
        }

        public static DomainResult<ChunkNumber> Create(int value)
        {
            return value < 1
                ? DomainResult<ChunkNumber>.Failure(ChunkErrors.InvalidNumber)
                : DomainResult<ChunkNumber>.Success(new ChunkNumber(value));
        }

        public ChunkNumber Next()
        {
            return new ChunkNumber(checked(Value + 1));
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
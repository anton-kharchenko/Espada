using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class EmbeddingDimensions : ValueObject
    {
        private EmbeddingDimensions(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public static DomainResult<EmbeddingDimensions> Create(int value)
        {
            return value <= 0
                ? DomainResult<EmbeddingDimensions>.Failure(ChunkEmbeddingErrors.DimensionsInvalid)
                : DomainResult<EmbeddingDimensions>.Success(new EmbeddingDimensions(value));
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
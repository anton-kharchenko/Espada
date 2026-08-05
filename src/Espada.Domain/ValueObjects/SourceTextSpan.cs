using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class SourceTextSpan : ValueObject
    {
        private SourceTextSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public int Start { get; }

        public int Length { get; }

        public int EndExclusive => Start + Length;

        public static DomainResult<SourceTextSpan> Create(int start, int length)
        {
            if (start < 0)
            {
                return DomainResult<SourceTextSpan>.Failure(ChunkErrors.SourceSpanStartInvalid);
            }

            if (length < 1)
            {
                return DomainResult<SourceTextSpan>.Failure(ChunkErrors.SourceSpanLengthInvalid);
            }

            return start > int.MaxValue - length
                ? DomainResult<SourceTextSpan>.Failure(ChunkErrors.SourceSpanOverflow)
                : DomainResult<SourceTextSpan>.Success(new SourceTextSpan(start, length));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Start;
            yield return Length;
        }

        public override string ToString()
        {
            return $"[{Start}..{EndExclusive})";
        }
    }
}
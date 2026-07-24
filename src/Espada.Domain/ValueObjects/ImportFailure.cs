using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects;

public sealed class ImportFailure : ValueObject
{
    public const int CodeMaxLength = 100;
    public const int ReasonMaxLength = 1000;

    private ImportFailure(
        string code,
        string reason)
    {
        Code = code;
        Reason = reason;
    }

    public string Code { get; }

    public string Reason { get; }

    public static DomainResult<ImportFailure> Create(string? code, string? reason)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return DomainResult<ImportFailure>.Failure(ImportJobErrors.FailureCodeEmpty);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return DomainResult<ImportFailure>.Failure(ImportJobErrors.FailureReasonEmpty);
        }

        string normalizedCode = code.Trim();
        string normalizedReason = reason.Trim();

        if (normalizedCode.Length > CodeMaxLength)
        {
            return DomainResult<ImportFailure>.Failure(ImportJobErrors.FailureCodeTooLong);
        }

        return normalizedReason.Length > ReasonMaxLength ? DomainResult<ImportFailure>.Failure(ImportJobErrors.FailureReasonTooLong) : DomainResult<ImportFailure>.Success(new ImportFailure(normalizedCode, normalizedReason));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
        yield return Reason;
    }

    public override string ToString() => $"{Code}: {Reason}";
}
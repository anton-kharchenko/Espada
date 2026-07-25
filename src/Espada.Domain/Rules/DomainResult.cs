namespace Espada.Domain.Rules;

public class DomainResult
{
    protected DomainResult(bool isSuccess, DomainError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        switch (isSuccess)
        {
            case true when error != DomainError.None:
                throw new InvalidOperationException("A successful result cannot contain an error.");
            case false when error == DomainError.None:
                throw new InvalidOperationException("A failed result must contain an error.");
            default:
                IsSuccess = isSuccess;
                Error = error;
                break;
        }
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public DomainError Error { get; }

    public static DomainResult Success() => new(true, DomainError.None);

    public static DomainResult Failure(DomainError error) => new(false, error);

    public static DomainResult<TValue> Success<TValue>(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new DomainResult<TValue>(value, true, DomainError.None);
    }

    public static DomainResult<TValue> Failure<TValue>(DomainError error) => new(default, false, error);
}

public sealed class DomainResult<TValue> : DomainResult
{
    internal DomainResult(TValue? value, bool isSuccess, DomainError error) : base(isSuccess, error)
    {
        Value = value;
    }

    public TValue Value => IsFailure ? throw new InvalidOperationException("The value of a failed result cannot be accessed.") : field!;

    public static DomainResult<TValue> Success(TValue value) => new(value, true, DomainError.None);

    public new static DomainResult<TValue> Failure(DomainError error) => new(default, false, error);
}
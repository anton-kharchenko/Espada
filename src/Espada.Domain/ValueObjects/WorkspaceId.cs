using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects;

public sealed class WorkspaceId : ValueObject
{
    private WorkspaceId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static WorkspaceId New() => new(Guid.NewGuid());

    public static WorkspaceId Create(Guid value) => value == Guid.Empty ? throw new ArgumentException("Workspace ID cannot be empty.", nameof(value)) : new WorkspaceId(value);

    public override string ToString() => Value.ToString("D");

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects;

public sealed class WorkspaceId : ValueObject
{
    private WorkspaceId(Guid value) => Value = value;

    public Guid Value { get; }
        
    public override string ToString() => Value.ToString("D");

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
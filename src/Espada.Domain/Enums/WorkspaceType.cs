using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums;

public class WorkspaceType(int id, string name) : Enumeration(id, name)
{
    public static readonly WorkspaceType Personal = new(1, nameof(Personal));
    public static readonly WorkspaceType Team = new(2, nameof(Team));
    public static readonly WorkspaceType Organization = new(3, nameof(Organization));

    public override bool Equals(object? obj) => base.Equals(obj);

    public override int GetHashCode() => base.GetHashCode();
}
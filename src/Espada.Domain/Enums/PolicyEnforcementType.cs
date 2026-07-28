using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums;

public sealed class PolicyEnforcementType(int id, string name) : Enumeration(id, name)
{
    public static readonly PolicyEnforcementType Hard = new(1, nameof(Hard));
    
    public static readonly PolicyEnforcementType Soft = new(2, nameof(Soft));
}

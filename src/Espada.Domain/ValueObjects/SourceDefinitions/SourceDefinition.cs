using Espada.Domain.Enums;

namespace Espada.Domain.ValueObjects.SourceDefinitions;

public abstract record SourceDefinition
{
    public abstract SourceType SourceType { get; }

    public abstract string CanonicalLocator { get; }
}
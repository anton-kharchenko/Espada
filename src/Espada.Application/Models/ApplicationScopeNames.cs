using System.Collections.Frozen;

namespace Espada.Application.Models;

public static class ApplicationScopeNames
{
    public const string WorkspaceCreate = "workspace:create";
    public const string WorkspaceRead = "workspace:read";
    public const string WorkspaceWrite = "workspace:write";
    public const string MemoryRead = "memory:read";
    public const string MemoryWrite = "memory:write";
    public const string SourceRead = "source:read";
    public const string SourceWrite = "source:write";
    public const string ArtifactRead = "artifact:read";
    public const string ArtifactWrite = "artifact:write";
    public const string ContextRead = "context:read";

    public static IReadOnlySet<string> All { get; } = new[]
        {
            WorkspaceCreate,
            WorkspaceRead,
            WorkspaceWrite,
            MemoryRead,
            MemoryWrite,
            SourceRead,
            SourceWrite,
            ArtifactRead,
            ArtifactWrite,
            ContextRead
        }
        .ToFrozenSet(StringComparer.Ordinal);
}

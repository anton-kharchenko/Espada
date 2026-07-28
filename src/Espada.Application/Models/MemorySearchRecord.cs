using Espada.Domain.Aggregates;

namespace Espada.Application.Models
{
    public sealed record MemorySearchRecord(
        Artifact Artifact,
        ArtifactRevision Revision,
        MemoryMetadata Metadata,
        double Score);
}
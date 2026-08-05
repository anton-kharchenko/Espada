using Espada.Domain.Aggregates;

namespace Espada.Application.Models
{
    internal sealed record ArtifactRevisionResponseMappingSource(
        Artifact Artifact,
        ArtifactRevision Revision);
}
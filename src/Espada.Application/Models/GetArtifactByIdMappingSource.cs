using Espada.Domain.Aggregates;
using Espada.Domain.Rules;

namespace Espada.Application.Models
{
    internal sealed record GetArtifactByIdMappingSource(
        Artifact Artifact,
        ArtifactRevision? Revision,
        IReadOnlyList<InstructionRule> InstructionRules,
        IReadOnlyList<PolicyRule> PolicyRules);
}
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;

namespace Espada.Application.Models
{
    public sealed record ContextCandidateRecord(
        Binding Binding,
        Artifact Artifact,
        ArtifactRevision Revision,
        IReadOnlyList<InstructionRule> InstructionRules,
        IReadOnlyList<PolicyRule> PolicyRules,
        MemoryMetadata? MemoryMetadata,
        bool IsMemorySuperseded);
}
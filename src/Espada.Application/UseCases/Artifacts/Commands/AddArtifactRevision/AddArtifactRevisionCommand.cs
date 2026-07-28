using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.Artifacts.Common;

namespace Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision
{
    public sealed record AddArtifactRevisionCommand(
        Guid WorkspaceId,
        Guid ArtifactId,
        string Content,
        IReadOnlyList<InstructionRuleInput>? InstructionRules = null,
        IReadOnlyList<PolicyRuleInput>? PolicyRules = null,
        bool AllowPolicyMutation = false,
        int? RequiredKindTypeId = null) : ICommand<AddArtifactRevisionResponse>;
}

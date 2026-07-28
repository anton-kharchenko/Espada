using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.Artifacts.Common;

namespace Espada.Application.UseCases.Artifacts.Commands.CreateArtifact
{
    public sealed record CreateArtifactCommand(
        Guid WorkspaceId,
        string Title,
        int TypeId,
        string Content,
        int KindTypeId = 1,
        IReadOnlyList<InstructionRuleInput>? InstructionRules = null,
        IReadOnlyList<PolicyRuleInput>? PolicyRules = null,
        bool AllowPolicyMutation = false) : ICommand<CreateArtifactResponse>;
}

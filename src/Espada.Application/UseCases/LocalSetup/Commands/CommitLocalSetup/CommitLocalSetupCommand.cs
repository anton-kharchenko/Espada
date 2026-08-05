using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.LocalSetup.Commands.CommitLocalSetup
{
    public sealed record CommitLocalSetupCommand(
        Guid SetupId,
        Guid DeviceId,
        string WorkspaceName,
        string ProjectName,
        string RepositoryRoot,
        string? CanonicalRemoteUri,
        string InitialInstruction,
        string IdentityIssuer,
        string IdentitySubject,
        string DeviceName,
        IReadOnlyList<LocalSetupInstructionInput> Instructions,
        IReadOnlyList<LocalSetupAgentInput> Agents) : ICommand<CommitLocalSetupResponse>;
}

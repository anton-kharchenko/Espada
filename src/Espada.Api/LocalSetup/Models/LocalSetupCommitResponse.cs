namespace Espada.Api.LocalSetup.Models
{
    internal sealed record LocalSetupCommitResponse(
        Guid WorkspaceId,
        Guid ProjectId,
        Guid RepositorySourceId,
        bool AlreadyCompleted,
        IReadOnlyList<string> ConfiguredAgents);
}

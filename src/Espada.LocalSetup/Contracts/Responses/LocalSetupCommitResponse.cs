namespace Espada.LocalSetup.Contracts.Responses
{
    public sealed record LocalSetupCommitResponse(
        Guid WorkspaceId,
        Guid ProjectId,
        Guid RepositorySourceId,
        bool AlreadyCompleted,
        IReadOnlyList<string> ConfiguredAgents);
}
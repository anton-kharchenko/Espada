namespace Espada.Application.UseCases.LocalSetup.Commands.CommitLocalSetup
{
    public sealed record CommitLocalSetupResponse(
        Guid WorkspaceId,
        Guid ProjectId,
        Guid RepositorySourceId,
        bool AlreadyCompleted);
}
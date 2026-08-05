namespace Espada.Application.Models
{
    public sealed record RepositoryWatchRegistration(
        Guid WorkspaceId,
        Guid SourceId,
        string RepositoryRoot);
}
namespace Espada.Api.WebConsole.Requests
{
    internal sealed record ConsoleSetBindingRequest(
        Guid ArtifactId,
        Guid? BindingId = null,
        Guid? OrganizationId = null,
        Guid? ProjectId = null,
        string? RepositoryCanonicalUri = null,
        string? RepositoryRelativePathPrefix = null,
        string? Branch = null,
        Guid? TaskId = null,
        string? Agent = null);
}
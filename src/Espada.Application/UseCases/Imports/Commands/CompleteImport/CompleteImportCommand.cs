using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Imports.Commands.CompleteImport
{
    public sealed record CompleteImportCommand(
        Guid WorkspaceId,
        Guid ImportJobId,
        Guid ArtifactId,
        Guid ArtifactRevisionId) : ICommand;
}
using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Imports.Commands.CancelImport
{
    public sealed record CancelImportCommand(
        Guid WorkspaceId,
        Guid ImportJobId) : ICommand;
}
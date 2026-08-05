using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Imports.Commands.StartImport
{
    public sealed record StartImportCommand(Guid WorkspaceId, Guid ImportJobId) : ICommand;
}
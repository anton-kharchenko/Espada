using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Imports.Commands.FailImport
{
    public sealed record FailImportCommand(
        Guid WorkspaceId,
        Guid ImportJobId,
        string FailureCode,
        string FailureReason) : ICommand;
}
using Espada.Application.Contracts.Messaging;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects.SourceDefinitions;

namespace Espada.Application.UseCases.Sources.Commands.RegisterSource
{
    public sealed record RegisterSourceCommand(
        Guid WorkspaceId,
        string Name,
        SourceDefinition Definition)
        : ICommand<RegisterSourceResponse>
    {
        public RegisterSourceCommand(Guid workspaceId, string name, string locator, SourceType type) : this(workspaceId,
            name, new LegacySourceDefinition(type.Id, locator))
        {
        }
    }
}
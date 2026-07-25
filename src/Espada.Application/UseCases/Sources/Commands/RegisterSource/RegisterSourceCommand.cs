using Espada.Application.Contracts.Messaging;
using Espada.Domain.Enums;

namespace Espada.Application.UseCases.Sources.Commands.RegisterSource;

public sealed record RegisterSourceCommand(
    Guid WorkspaceId,
    string Name,
    string Locator,
    SourceType Type) 
    : ICommand<RegisterSourceResponse>;
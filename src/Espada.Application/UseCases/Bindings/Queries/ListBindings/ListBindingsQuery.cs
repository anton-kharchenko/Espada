using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Bindings.Queries.ListBindings
{
    public sealed record ListBindingsQuery(
        Guid WorkspaceId) : IQuery<ListBindingsResponse>;
}
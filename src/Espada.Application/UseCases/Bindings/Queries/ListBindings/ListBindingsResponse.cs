using Espada.Application.UseCases.Bindings.Common;

namespace Espada.Application.UseCases.Bindings.Queries.ListBindings
{
    public sealed record ListBindingsResponse(
        IReadOnlyList<BindingResponse> Items);
}
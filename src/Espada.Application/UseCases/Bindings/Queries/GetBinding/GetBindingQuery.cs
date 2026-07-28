using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.Bindings.Common;

namespace Espada.Application.UseCases.Bindings.Queries.GetBinding
{
    public sealed record GetBindingQuery(
        Guid WorkspaceId,
        Guid BindingId) : IQuery<BindingResponse>;
}
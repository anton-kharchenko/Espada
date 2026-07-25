using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.Sources.Common;

namespace Espada.Application.UseCases.Sources.Queries.GetSourceById
{
    public sealed record GetSourceByIdQuery(Guid WorkspaceId, Guid SourceId) : IQuery<SourceResponse>;
}
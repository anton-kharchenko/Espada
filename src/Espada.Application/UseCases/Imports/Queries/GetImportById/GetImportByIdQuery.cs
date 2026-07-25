using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Imports.Queries.GetImportById
{
    public sealed record GetImportByIdQuery(
        Guid WorkspaceId,
        Guid ImportJobId) : IQuery<GetImportByIdResponse>;
}
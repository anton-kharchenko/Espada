using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Imports.Queries.ListImports
{
    public sealed record ListImportsQuery(
        Guid WorkspaceId) : IQuery<ListImportsResponse>;
}

using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Context.Queries.BuildContext
{
    public sealed record BuildContextQuery(
        Guid WorkspaceId,
        Guid? ProjectId,
        Guid? TaskId,
        string? RepositoryRelativePath,
        string? Branch,
        string Agent,
        int TokenBudget) : IQuery<BuildContextResponse>;
}
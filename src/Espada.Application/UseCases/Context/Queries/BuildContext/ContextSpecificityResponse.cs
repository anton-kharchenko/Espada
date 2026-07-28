namespace Espada.Application.UseCases.Context.Queries.BuildContext
{
    public sealed record ContextSpecificityResponse(
        int Agent,
        int Task,
        int Branch,
        int PathSegments,
        int PathBytes,
        int Repository,
        int Project,
        int Organization);
}
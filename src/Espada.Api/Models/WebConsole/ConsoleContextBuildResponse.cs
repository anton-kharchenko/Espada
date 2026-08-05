using Espada.AgentAdapters.Context;
using Espada.Application.UseCases.Context.Queries.BuildContext;

namespace Espada.Api.Models.WebConsole
{
    public sealed record ConsoleContextBuildResponse(
        BuildContextResponse Context,
        AgentContextProjection Projection);
}
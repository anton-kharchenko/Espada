using Espada.AgentAdapters.Context;
using Espada.Application.UseCases.Context.Queries.BuildContext;

namespace Espada.Api.WebConsole
{
    internal sealed record ConsoleContextBuildResponse(
        BuildContextResponse Context,
        AgentContextProjection Projection);
}
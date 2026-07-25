using Espada.Application.UseCases.Sources.Common;
using Espada.Domain.Aggregates;

namespace Espada.Application.Mappings
{
    internal static class SourceMappings
    {
        public static SourceResponse ToResponse(this Source source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return new SourceResponse(
                source.Id.Value,
                source.WorkspaceId.Value,
                source.Name.Value,
                source.Locator.Value,
                source.Type.Id,
                source.Type.Name,
                source.Status.Id,
                source.Status.Name,
                source.CreatedAtUtc);
        }
    }
}
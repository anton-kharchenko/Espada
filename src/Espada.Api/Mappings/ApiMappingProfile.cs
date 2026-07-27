using AutoMapper;
using Espada.Api.Contracts.Models;
using Espada.Application.UseCases.Context.Queries.SearchWorkspaceContext;

namespace Espada.Api.Mappings;

public sealed class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        CreateMap<SearchWorkspaceContextMappingSource, SearchWorkspaceContextQuery>()
            .ConvertUsing(source => new SearchWorkspaceContextQuery(
                source.WorkspaceId,
                source.Request.QueryText,
                source.Request.QueryVector,
                source.Request.ModelIdentifier,
                source.Request.ModelVersion,
                source.Request.TopK,
                source.Request.ArtifactIds,
                source.Request.RevisionIds,
                source.Request.SourceIds,
                source.Request.ArtifactTypeIds,
                source.Request.SourceTypeIds,
                source.Request.CreatedAfterUtc,
                source.Request.MinimumSimilarity,
                source.Request.MinimumArtifactPriority,
                source.Request.MinimumSourcePriority));
    }
}
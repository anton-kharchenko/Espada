using AutoMapper;
using Espada.Application.UseCases.Context.Queries.SearchWorkspaceContext;
using Espada.Daemon.Models;
using Espada.Protocol.Mcp.Contracts.Responses;

namespace Espada.Daemon.Mappings;

public sealed class DaemonMappingProfile : Profile
{
    public DaemonMappingProfile()
    {
        CreateMap<ContextSearchMappingSource, SearchWorkspaceContextQuery>()
            .ConvertUsing(source => new SearchWorkspaceContextQuery(
                source.Request.WorkspaceId,
                source.Request.QueryText,
                source.QueryVector,
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

        CreateMap<WorkspaceContextItemResponse, ContextSearchItem>();
        CreateMap<SearchWorkspaceContextResponse, ContextSearchResponse>();
    }
}
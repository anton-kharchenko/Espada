using AutoMapper;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Application.UseCases.ChunkEmbeddings.Commands.CreateChunkEmbedding;
using Espada.Application.UseCases.Context.Queries.SearchWorkspaceContext;
using Espada.Application.UseCases.Sources.Common;
using Espada.Application.UseCases.Workspaces.Common;
using Espada.Domain.Aggregates;

namespace Espada.Application.Mappings;

public sealed class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<ChunkEmbedding, CreateChunkEmbeddingResponse>()
            .ForCtorParam(
                nameof(CreateChunkEmbeddingResponse.ChunkEmbeddingId),
                options => options.MapFrom(embedding => embedding.Id.Value))
            .ForCtorParam(
                nameof(CreateChunkEmbeddingResponse.ChunkId),
                options => options.MapFrom(embedding => embedding.ChunkId.Value))
            .ForCtorParam(
                nameof(CreateChunkEmbeddingResponse.ChunkContentHash),
                options => options.MapFrom(embedding => embedding.ChunkContentHash.Value))
            .ForCtorParam(
                nameof(CreateChunkEmbeddingResponse.ModelIdentifier),
                options => options.MapFrom(embedding => embedding.Model.Identifier))
            .ForCtorParam(
                nameof(CreateChunkEmbeddingResponse.ModelVersion),
                options => options.MapFrom(embedding => embedding.Model.Version))
            .ForCtorParam(
                nameof(CreateChunkEmbeddingResponse.Dimensions),
                options => options.MapFrom(embedding => embedding.Dimensions.Value));

        CreateMap<WorkspaceContextSearchMappingSource, WorkspaceContextSearch>()
            .ConvertUsing(source => new WorkspaceContextSearch(
                source.Query.WorkspaceId,
                source.Query.QueryText,
                source.Query.QueryVector,
                source.Model.Identifier,
                source.Model.Version,
                source.Query.TopK,
                source.Query.ArtifactIds ?? Array.Empty<Guid>(),
                source.Query.RevisionIds ?? Array.Empty<Guid>(),
                source.Query.SourceIds ?? Array.Empty<Guid>(),
                source.Query.ArtifactTypeIds ?? Array.Empty<int>(),
                source.Query.SourceTypeIds ?? Array.Empty<int>(),
                source.Query.CreatedAfterUtc,
                source.Query.MinimumSimilarity,
                source.Query.MinimumArtifactPriority,
                source.Query.MinimumSourcePriority,
                source.NowUtc));

        CreateMap<WorkspaceContextSearchHit, WorkspaceContextItemResponse>();

        CreateMap<Workspace, WorkspaceResponse>()
            .ForCtorParam(
                nameof(WorkspaceResponse.Id),
                options => options.MapFrom(workspace => workspace.Id.Value))
            .ForCtorParam(
                nameof(WorkspaceResponse.Name),
                options => options.MapFrom(workspace => workspace.Name.Value))
            .ForCtorParam(
                nameof(WorkspaceResponse.TypeId),
                options => options.MapFrom(workspace => workspace.Type.Id))
            .ForCtorParam(
                nameof(WorkspaceResponse.TypeName),
                options => options.MapFrom(workspace => workspace.Type.Name))
            .ForCtorParam(
                nameof(WorkspaceResponse.StatusId),
                options => options.MapFrom(workspace => workspace.Status.Id))
            .ForCtorParam(
                nameof(WorkspaceResponse.StatusName),
                options => options.MapFrom(workspace => workspace.Status.Name));

        CreateMap<Source, SourceResponse>()
            .ForCtorParam(
                nameof(SourceResponse.Id),
                options => options.MapFrom(source => source.Id.Value))
            .ForCtorParam(
                nameof(SourceResponse.WorkspaceId),
                options => options.MapFrom(source => source.WorkspaceId.Value))
            .ForCtorParam(
                nameof(SourceResponse.Name),
                options => options.MapFrom(source => source.Name.Value))
            .ForCtorParam(
                nameof(SourceResponse.Locator),
                options => options.MapFrom(source => source.Locator.Value))
            .ForCtorParam(
                nameof(SourceResponse.TypeId),
                options => options.MapFrom(source => source.Type.Id))
            .ForCtorParam(
                nameof(SourceResponse.TypeName),
                options => options.MapFrom(source => source.Type.Name))
            .ForCtorParam(
                nameof(SourceResponse.StatusId),
                options => options.MapFrom(source => source.Status.Id))
            .ForCtorParam(
                nameof(SourceResponse.StatusName),
                options => options.MapFrom(source => source.Status.Name))
            .ForCtorParam(
                nameof(SourceResponse.Priority),
                options => options.MapFrom(source => source.Priority.Value));
    }
}
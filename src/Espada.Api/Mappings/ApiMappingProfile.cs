using AutoMapper;
using Espada.Api.Contracts.Models;
using Espada.Api.Extensions;
using Espada.Application.UseCases.Context.Queries.SearchWorkspaceContext;
using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Domain.Enums;

namespace Espada.Api.Mappings;

public sealed class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        CreateMap<CreateWorkspaceMappingSource, CreateWorkspaceCommand>()
            .ConvertUsing(source => MapCreateWorkspaceCommand(source));

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

    private static CreateWorkspaceCommand MapCreateWorkspaceCommand(
        CreateWorkspaceMappingSource source) =>
        new(
            source.Request.Name,
            source.Request.TypeId.ToEnumeration<WorkspaceType>()
                ?? throw new InvalidOperationException(
                    $"Workspace type ID '{source.Request.TypeId}' passed validation but could not be resolved."),
            source.IdentityIssuer,
            source.IdentitySubject);
}
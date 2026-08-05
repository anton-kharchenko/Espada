using AutoMapper;
using Espada.Application.Models;
using Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision;
using Espada.Application.UseCases.Artifacts.Commands.CreateArtifact;
using Espada.Application.UseCases.Artifacts.Common;
using Espada.Application.UseCases.Artifacts.Queries.GetArtifactById;
using Espada.Application.UseCases.Artifacts.Queries.ListArtifacts;
using Espada.Application.UseCases.Bindings.Common;
using Espada.Application.UseCases.ChunkEmbeddings.Commands.CreateChunkEmbedding;
using Espada.Application.UseCases.Context.Queries.BuildContext;
using Espada.Application.UseCases.Imports.Queries.GetImportById;
using Espada.Application.UseCases.Imports.Queries.ListImports;
using Espada.Application.UseCases.Memories.Commands.RememberMemory;
using Espada.Application.UseCases.Memories.Queries.SearchMemory;
using Espada.Application.UseCases.Organizations.Common;
using Espada.Application.UseCases.Projects.Common;
using Espada.Application.UseCases.Sources.Common;
using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Application.UseCases.Workspaces.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;

namespace Espada.Application.Mappings
{
    public sealed class ApplicationMappingProfile : Profile
    {
        public ApplicationMappingProfile()
        {
            CreateMap<ArtifactRevisionResponseMappingSource, CreateArtifactResponse>()
                .ForCtorParam(
                    nameof(CreateArtifactResponse.ArtifactId),
                    options => options.MapFrom(source => source.Artifact.Id.Value))
                .ForCtorParam(
                    nameof(CreateArtifactResponse.ArtifactRevisionId),
                    options => options.MapFrom(source => source.Revision.Id.Value))
                .ForCtorParam(
                    nameof(CreateArtifactResponse.RevisionNumber),
                    options => options.MapFrom(source => source.Revision.Number.Value))
                .ForCtorParam(
                    nameof(CreateArtifactResponse.ContentHash),
                    options => options.MapFrom(source => source.Revision.ContentHash.Value))
                .ForCtorParam(
                    nameof(CreateArtifactResponse.SizeInBytes),
                    options => options.MapFrom(source => source.Revision.SizeInBytes))
                .ForCtorParam(
                    nameof(CreateArtifactResponse.CreatedAtUtc),
                    options => options.MapFrom(source => source.Revision.CreatedAtUtc));

            CreateMap<ArtifactRevisionResponseMappingSource, AddArtifactRevisionResponse>()
                .ForCtorParam(
                    nameof(AddArtifactRevisionResponse.ArtifactId),
                    options => options.MapFrom(source => source.Artifact.Id.Value))
                .ForCtorParam(
                    nameof(AddArtifactRevisionResponse.ArtifactRevisionId),
                    options => options.MapFrom(source => source.Revision.Id.Value))
                .ForCtorParam(
                    nameof(AddArtifactRevisionResponse.RevisionNumber),
                    options => options.MapFrom(source => source.Revision.Number.Value))
                .ForCtorParam(
                    nameof(AddArtifactRevisionResponse.ContentHash),
                    options => options.MapFrom(source => source.Revision.ContentHash.Value))
                .ForCtorParam(
                    nameof(AddArtifactRevisionResponse.SizeInBytes),
                    options => options.MapFrom(source => source.Revision.SizeInBytes))
                .ForCtorParam(
                    nameof(AddArtifactRevisionResponse.CreatedAtUtc),
                    options => options.MapFrom(source => source.Revision.CreatedAtUtc));

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

            CreateMap<MemoryWorkspaceContextSearchMappingSource, WorkspaceContextSearch>()
                .ConvertUsing(source => new WorkspaceContextSearch(
                    source.Query.WorkspaceId,
                    source.Query.QueryText.Trim(),
                    source.QueryVector,
                    source.Model == null ? string.Empty : source.Model.Identifier,
                    source.Model == null ? string.Empty : source.Model.Version,
                    source.Query.TopK,
                    Array.Empty<Guid>(),
                    Array.Empty<Guid>(),
                    Array.Empty<Guid>(),
                    Array.Empty<int>(),
                    new[] { ArtifactKindType.Memory.Name },
                    Array.Empty<int>(),
                    source.MemoryCategories,
                    null,
                    null,
                    null,
                    null,
                    source.NowUtc,
                    true));
            CreateMap<ContextSpecificity, ContextSpecificityResponse>();
            CreateMap<ContextSelectorMatch, ContextSelectorMatchResponse>();
            CreateMap<ContextConflict, ContextConflictResponse>();
            CreateMap<ContextExplanation, ContextExplanationResponse>();
            CreateMap<ContextBudgetSummary, ContextBudgetSummaryResponse>();
            CreateMap<ResolvedContextItem, ContextItemResponse>()
                .ForCtorParam(
                    nameof(ContextItemResponse.BindingId),
                    options => options.MapFrom(item => item.Binding.Id.Value))
                .ForCtorParam(
                    nameof(ContextItemResponse.ArtifactId),
                    options => options.MapFrom(item => item.Artifact.Id.Value))
                .ForCtorParam(
                    nameof(ContextItemResponse.RevisionId),
                    options => options.MapFrom(item => item.Revision.Id.Value))
                .ForCtorParam(
                    nameof(ContextItemResponse.ArtifactKind),
                    options => options.MapFrom(item => item.Artifact.KindType.Name))
                .ForCtorParam(
                    nameof(ContextItemResponse.Title),
                    options => options.MapFrom(item => item.Artifact.Title.Value))
                .ForCtorParam(
                    nameof(ContextItemResponse.ArtifactPriority),
                    options => options.MapFrom(item => item.Artifact.Priority.Value))
                .ForCtorParam(
                    nameof(ContextItemResponse.UserConfirmed),
                    options => options.MapFrom(item => item.MemoryMetadata == null
                        ? (bool?)null
                        : item.MemoryMetadata.UserConfirmed))
                .ForCtorParam(
                    nameof(ContextItemResponse.Confidence),
                    options => options.MapFrom(item => item.MemoryMetadata == null
                        ? (decimal?)null
                        : item.MemoryMetadata.Confidence))
                .ForCtorParam(
                    nameof(ContextItemResponse.Provenance),
                    options => options.MapFrom(item => item.MemoryMetadata));
            CreateMap<ResolvedContext, BuildContextResponse>()
                .ForCtorParam(
                    nameof(BuildContextResponse.WorkspaceId),
                    options => options.MapFrom(context => context.Workspace.Id.Value))
                .ForCtorParam(
                    nameof(BuildContextResponse.OrganizationId),
                    options => options.MapFrom(context => context.Workspace.OrganizationId == null
                        ? (Guid?)null
                        : context.Workspace.OrganizationId.Value))
                .ForCtorParam(
                    nameof(BuildContextResponse.ProjectId),
                    options => options.MapFrom(context => context.Project == null
                        ? (Guid?)null
                        : context.Project.Id.Value))
                .ForCtorParam(
                    nameof(BuildContextResponse.TaskId),
                    options => options.MapFrom(context => context.Task == null
                        ? (Guid?)null
                        : context.Task.Id.Value));

            CreateMap<GetImportByIdMappingSource, GetImportByIdResponse>()
                .ConvertUsing(source => MapGetImportByIdResponse(source));

            CreateMap<ImportJob, ImportListItemResponse>()
                .ForCtorParam(
                    nameof(ImportListItemResponse.Id),
                    options => options.MapFrom(importJob => importJob.Id.Value))
                .ForCtorParam(
                    nameof(ImportListItemResponse.SourceId),
                    options => options.MapFrom(importJob => importJob.SourceId.Value))
                .ForCtorParam(
                    nameof(ImportListItemResponse.WorkspaceId),
                    options => options.MapFrom(importJob => importJob.WorkspaceId.Value))
                .ForCtorParam(
                    nameof(ImportListItemResponse.StatusId),
                    options => options.MapFrom(importJob => importJob.Status.Id))
                .ForCtorParam(
                    nameof(ImportListItemResponse.StatusName),
                    options => options.MapFrom(importJob => importJob.Status.Name))
                .ForCtorParam(
                    nameof(ImportListItemResponse.Stage),
                    options => options.MapFrom(importJob => importJob.Stage.Name))
                .ForCtorParam(
                    nameof(ImportListItemResponse.ArtifactId),
                    options => options.MapFrom(importJob => importJob.ArtifactId == null
                        ? (Guid?)null
                        : importJob.ArtifactId.Value))
                .ForCtorParam(
                    nameof(ImportListItemResponse.ArtifactRevisionId),
                    options => options.MapFrom(importJob => importJob.ArtifactRevisionId == null
                        ? (Guid?)null
                        : importJob.ArtifactRevisionId.Value))
                .ForCtorParam(
                    nameof(ImportListItemResponse.FailureCode),
                    options => options.MapFrom(importJob => importJob.Failure == null
                        ? null
                        : importJob.Failure.Code))
                .ForCtorParam(
                    nameof(ImportListItemResponse.FailureReason),
                    options => options.MapFrom(importJob => importJob.Failure == null
                        ? null
                        : importJob.Failure.Reason));

            CreateMap<Workspace, WorkspaceResponse>()
                .ForCtorParam(
                    nameof(WorkspaceResponse.Id),
                    options => options.MapFrom(workspace => workspace.Id.Value))
                .ForCtorParam(
                    nameof(WorkspaceResponse.OrganizationId),
                    options => options.MapFrom(workspace => workspace.OrganizationId == null
                        ? (Guid?)null
                        : workspace.OrganizationId.Value))
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

            CreateMap<Workspace, CreateWorkspaceResponse>()
                .ForCtorParam(
                    nameof(CreateWorkspaceResponse.WorkspaceId),
                    options => options.MapFrom(workspace => workspace.Id.Value))
                .ForCtorParam(
                    nameof(CreateWorkspaceResponse.OrganizationId),
                    options => options.MapFrom(workspace => workspace.OrganizationId == null
                        ? (Guid?)null
                        : workspace.OrganizationId.Value));

            CreateMap<ArtifactRevision, CurrentArtifactRevisionResponse>()
                .ForCtorParam(
                    nameof(CurrentArtifactRevisionResponse.Id),
                    options => options.MapFrom(revision => revision.Id.Value))
                .ForCtorParam(
                    nameof(CurrentArtifactRevisionResponse.Number),
                    options => options.MapFrom(revision => revision.Number.Value))
                .ForCtorParam(
                    nameof(CurrentArtifactRevisionResponse.Content),
                    options => options.MapFrom(revision => revision.Content.Value))
                .ForCtorParam(
                    nameof(CurrentArtifactRevisionResponse.ContentHash),
                    options => options.MapFrom(revision => revision.ContentHash.Value));

            CreateMap<InstructionRule, InstructionRuleResponse>()
                .ForCtorParam(
                    nameof(InstructionRuleResponse.RuleKey),
                    options => options.MapFrom(rule => rule.RuleKey.Value))
                .ForCtorParam(
                    nameof(InstructionRuleResponse.Priority),
                    options => options.MapFrom(rule => rule.Priority.Value));

            CreateMap<PolicyRule, PolicyRuleResponse>()
                .ForCtorParam(
                    nameof(PolicyRuleResponse.RuleKey),
                    options => options.MapFrom(rule => rule.RuleKey.Value))
                .ForCtorParam(
                    nameof(PolicyRuleResponse.Priority),
                    options => options.MapFrom(rule => rule.Priority.Value))
                .ForCtorParam(
                    nameof(PolicyRuleResponse.EnforcementTypeId),
                    options => options.MapFrom(rule => rule.EnforcementType.Id))
                .ForCtorParam(
                    nameof(PolicyRuleResponse.EnforcementTypeName),
                    options => options.MapFrom(rule => rule.EnforcementType.Name));

            CreateMap<Artifact, ArtifactListItemResponse>()
                .ForCtorParam(
                    nameof(ArtifactListItemResponse.Id),
                    options => options.MapFrom(artifact => artifact.Id.Value))
                .ForCtorParam(
                    nameof(ArtifactListItemResponse.Title),
                    options => options.MapFrom(artifact => artifact.Title.Value))
                .ForCtorParam(
                    nameof(ArtifactListItemResponse.KindTypeId),
                    options => options.MapFrom(artifact => artifact.KindType.Id))
                .ForCtorParam(
                    nameof(ArtifactListItemResponse.KindTypeName),
                    options => options.MapFrom(artifact => artifact.KindType.Name))
                .ForCtorParam(
                    nameof(ArtifactListItemResponse.TypeId),
                    options => options.MapFrom(artifact => artifact.Type.Id))
                .ForCtorParam(
                    nameof(ArtifactListItemResponse.TypeName),
                    options => options.MapFrom(artifact => artifact.Type.Name))
                .ForCtorParam(
                    nameof(ArtifactListItemResponse.StatusId),
                    options => options.MapFrom(artifact => artifact.Status.Id))
                .ForCtorParam(
                    nameof(ArtifactListItemResponse.StatusName),
                    options => options.MapFrom(artifact => artifact.Status.Name))
                .ForCtorParam(
                    nameof(ArtifactListItemResponse.Priority),
                    options => options.MapFrom(artifact => artifact.Priority.Value))
                .ForCtorParam(
                    nameof(ArtifactListItemResponse.CurrentRevisionId),
                    options => options.MapFrom(artifact => artifact.CurrentRevisionId == null
                        ? (Guid?)null
                        : artifact.CurrentRevisionId.Value))
                .ForCtorParam(
                    nameof(ArtifactListItemResponse.CurrentRevisionNumber),
                    options => options.MapFrom(artifact => artifact.CurrentRevisionNumber == null
                        ? (int?)null
                        : artifact.CurrentRevisionNumber.Value));

            CreateMap<GetArtifactByIdMappingSource, GetArtifactByIdResponse>()
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.Id),
                    options => options.MapFrom(source => source.Artifact.Id.Value))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.WorkspaceId),
                    options => options.MapFrom(source => source.Artifact.WorkspaceId.Value))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.Title),
                    options => options.MapFrom(source => source.Artifact.Title.Value))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.KindTypeId),
                    options => options.MapFrom(source => source.Artifact.KindType.Id))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.KindTypeName),
                    options => options.MapFrom(source => source.Artifact.KindType.Name))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.TypeId),
                    options => options.MapFrom(source => source.Artifact.Type.Id))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.TypeName),
                    options => options.MapFrom(source => source.Artifact.Type.Name))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.StatusId),
                    options => options.MapFrom(source => source.Artifact.Status.Id))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.StatusName),
                    options => options.MapFrom(source => source.Artifact.Status.Name))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.Priority),
                    options => options.MapFrom(source => source.Artifact.Priority.Value))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.CurrentRevisionId),
                    options => options.MapFrom(source => source.Artifact.CurrentRevisionId == null
                        ? (Guid?)null
                        : source.Artifact.CurrentRevisionId.Value))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.CurrentRevisionNumber),
                    options => options.MapFrom(source => source.Artifact.CurrentRevisionNumber == null
                        ? (int?)null
                        : source.Artifact.CurrentRevisionNumber.Value))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.RevisionCount),
                    options => options.MapFrom(source => source.Artifact.RevisionCount))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.CreatedAtUtc),
                    options => options.MapFrom(source => source.Artifact.CreatedAtUtc))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.UpdatedAtUtc),
                    options => options.MapFrom(source => source.Artifact.UpdatedAtUtc))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.ArchivedAtUtc),
                    options => options.MapFrom(source => source.Artifact.ArchivedAtUtc))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.CurrentRevision),
                    options => options.MapFrom(source => source.Revision))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.InstructionRules),
                    options => options.MapFrom(source => source.InstructionRules))
                .ForCtorParam(
                    nameof(GetArtifactByIdResponse.PolicyRules),
                    options => options.MapFrom(source => source.PolicyRules));
            CreateMap<Binding, BindingResponse>()
                .ForCtorParam(
                    nameof(BindingResponse.Id),
                    options => options.MapFrom(binding => binding.Id.Value))
                .ForCtorParam(
                    nameof(BindingResponse.ArtifactRevisionId),
                    options => options.MapFrom(binding => binding.ArtifactRevisionId.Value))
                .ForCtorParam(
                    nameof(BindingResponse.WorkspaceId),
                    options => options.MapFrom(binding => binding.WorkspaceId.Value))
                .ForCtorParam(
                    nameof(BindingResponse.OrganizationId),
                    options => options.MapFrom(binding => binding.OrganizationId == null
                        ? (Guid?)null
                        : binding.OrganizationId.Value))
                .ForCtorParam(
                    nameof(BindingResponse.ProjectId),
                    options => options.MapFrom(binding => binding.ProjectId == null
                        ? (Guid?)null
                        : binding.ProjectId.Value))
                .ForCtorParam(
                    nameof(BindingResponse.TaskId),
                    options => options.MapFrom(binding => binding.TaskId == null
                        ? (Guid?)null
                        : binding.TaskId.Value));

            CreateMap<Organization, OrganizationResponse>()
                .ForCtorParam(
                    nameof(OrganizationResponse.Id),
                    options => options.MapFrom(organization => organization.Id.Value));

            CreateMap<OrganizationMembership, OrganizationMembershipResponse>()
                .ForCtorParam(
                    nameof(OrganizationMembershipResponse.Id),
                    options => options.MapFrom(membership => membership.Id.Value))
                .ForCtorParam(
                    nameof(OrganizationMembershipResponse.OrganizationId),
                    options => options.MapFrom(membership => membership.OrganizationId.Value))
                .ForCtorParam(
                    nameof(OrganizationMembershipResponse.RoleTypeId),
                    options => options.MapFrom(membership => membership.Role.Id))
                .ForCtorParam(
                    nameof(OrganizationMembershipResponse.RoleTypeName),
                    options => options.MapFrom(membership => membership.Role.Name));
            CreateMap<Project, ProjectResponse>()
                .ForCtorParam(
                    nameof(ProjectResponse.Id),
                    options => options.MapFrom(project => project.Id.Value))
                .ForCtorParam(
                    nameof(ProjectResponse.WorkspaceId),
                    options => options.MapFrom(project => project.WorkspaceId.Value));

            CreateMap<ProjectTask, ProjectTaskResponse>()
                .ForCtorParam(
                    nameof(ProjectTaskResponse.Id),
                    options => options.MapFrom(task => task.Id.Value))
                .ForCtorParam(
                    nameof(ProjectTaskResponse.WorkspaceId),
                    options => options.MapFrom(task => task.WorkspaceId.Value))
                .ForCtorParam(
                    nameof(ProjectTaskResponse.ProjectId),
                    options => options.MapFrom(task => task.ProjectId.Value))
                .ForCtorParam(
                    nameof(ProjectTaskResponse.StatusTypeId),
                    options => options.MapFrom(task => task.Status.Id))
                .ForCtorParam(
                    nameof(ProjectTaskResponse.StatusTypeName),
                    options => options.MapFrom(task => task.Status.Name));

            CreateMap<MemoryMetadata, MemoryProvenanceResponse>()
                .ForCtorParam(
                    nameof(MemoryProvenanceResponse.SupersededMemoryId),
                    options => options.MapFrom(metadata => metadata.SupersededMemoryId == null
                        ? (Guid?)null
                        : metadata.SupersededMemoryId.Value));

            CreateMap<MemoryMetadata, RememberMemoryResponse>()
                .ForCtorParam(
                    nameof(RememberMemoryResponse.MemoryId),
                    options => options.MapFrom(metadata => metadata.Id.Value))
                .ForCtorParam(
                    nameof(RememberMemoryResponse.ArtifactId),
                    options => options.MapFrom(metadata => metadata.ArtifactId.Value))
                .ForCtorParam(
                    nameof(RememberMemoryResponse.RevisionId),
                    options => options.MapFrom(metadata => metadata.ArtifactRevisionId.Value));
            CreateMap<MemorySearchRecord, MemorySearchItemResponse>()
                .ForCtorParam(
                    nameof(MemorySearchItemResponse.MemoryId),
                    options => options.MapFrom(source => source.Metadata.Id.Value))
                .ForCtorParam(
                    nameof(MemorySearchItemResponse.ArtifactId),
                    options => options.MapFrom(source => source.Artifact.Id.Value))
                .ForCtorParam(
                    nameof(MemorySearchItemResponse.RevisionId),
                    options => options.MapFrom(source => source.Revision.Id.Value))
                .ForCtorParam(
                    nameof(MemorySearchItemResponse.Title),
                    options => options.MapFrom(source => source.Artifact.Title.Value))
                .ForCtorParam(
                    nameof(MemorySearchItemResponse.Content),
                    options => options.MapFrom(source => source.Revision.Content.Value))
                .ForCtorParam(
                    nameof(MemorySearchItemResponse.CategoryTypeId),
                    options => options.MapFrom(source => source.Metadata.CategoryType.Id))
                .ForCtorParam(
                    nameof(MemorySearchItemResponse.CategoryTypeName),
                    options => options.MapFrom(source => source.Metadata.CategoryType.Name))
                .ForCtorParam(
                    nameof(MemorySearchItemResponse.Confidence),
                    options => options.MapFrom(source => source.Metadata.Confidence))
                .ForCtorParam(
                    nameof(MemorySearchItemResponse.Provenance),
                    options => options.MapFrom(source => source.Metadata));
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

        private static GetImportByIdResponse MapGetImportByIdResponse(
            GetImportByIdMappingSource source)
        {
            return new GetImportByIdResponse(
                source.ImportJob.Id.Value,
                source.ImportJob.SourceId.Value,
                source.ImportJob.WorkspaceId.Value,
                source.ImportJob.Status.Id,
                source.ImportJob.Status.Name,
                source.ImportJob.Stage.Name,
                source.LatestJob?.Attempt ?? 0,
                source.LatestJob?.Status.ToString(),
                source.LatestJob?.FailureCategory?.ToString(),
                source.IsTerminal,
                source.ImportJob.RequestedAtUtc,
                source.ImportJob.StartedAtUtc,
                source.ImportJob.CompletedAtUtc,
                source.ImportJob.ArtifactId?.Value,
                source.ImportJob.ArtifactRevisionId?.Value,
                source.ImportJob.Failure?.Code,
                source.ImportJob.Failure?.Reason);
        }
    }
}
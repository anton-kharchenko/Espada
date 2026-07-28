using AutoMapper;
using Espada.Api.Contracts.Models;
using Espada.Api.Contracts.Requests.Imports;
using Espada.Api.Contracts.Responses.Billing;
using Espada.Api.Extensions;
using Espada.Api.WebConsole;
using Espada.Api.WebConsole.Mappings;
using Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision;
using Espada.Application.UseCases.Artifacts.Commands.CreateArtifact;
using Espada.Application.UseCases.Bindings.Commands.SetBinding;
using Espada.Application.UseCases.Context.Queries.BuildContext;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Application.UseCases.Memories.Commands.RememberMemory;
using Espada.Application.UseCases.Projects.Commands.CreateProject;
using Espada.Application.UseCases.ProjectTasks.Commands.CreateProjectTask;
using Espada.Application.UseCases.Sources.Commands.RegisterSource;
using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Application.UseCases.Workspaces.Common;
using Espada.Billing.Enums;
using Espada.Billing.Models;
using Espada.Billing.UseCases.Checkout;
using Espada.Domain.Enums;
using ConsoleBuildContextRequest =
    Espada.Api.WebConsole.Requests.ConsoleBuildContextRequest;
using ConsoleCreateProjectRequest =
    Espada.Api.WebConsole.Requests.ConsoleCreateProjectRequest;
using ConsoleCreateProjectTaskRequest =
    Espada.Api.WebConsole.Requests.ConsoleCreateProjectTaskRequest;
using ConsoleSetBindingRequest =
    Espada.Api.WebConsole.Requests.ConsoleSetBindingRequest;

namespace Espada.Api.Mappings
{
    public sealed class ApiMappingProfile : Profile
    {
        public ApiMappingProfile()
        {
            CreateMap<CreateWorkspaceMappingSource, CreateWorkspaceCommand>()
                .ConvertUsing(source => MapCreateWorkspaceCommand(source));

            CreateMap<CreateCheckoutMappingSource, CreateCheckoutCommand>()
                .ConvertUsing(source => new CreateCheckoutCommand(
                    source.WorkspaceId,
                    Enum.Parse<CloudBillingPlanType>(
                        source.Request.Plan,
                        ignoreCase: true),
                    source.IdempotencyKey));

            CreateMap<BillingStatusSnapshot, BillingStatusResponse>()
                .ForCtorParam(
                    nameof(BillingStatusResponse.Plan),
                    options => options.MapFrom(source => source.Plan.ToString()))
                .ForCtorParam(
                    nameof(BillingStatusResponse.AccessState),
                    options => options.MapFrom(source => source.AccessState.ToString()));

            CreateMap<StripeWebhookReceipt, StripeWebhookReceiptResponse>();

            CreateMap<RegisterSourceMappingSource, RegisterSourceCommand>()
                .ConvertUsing(source => new RegisterSourceCommand(
                    source.WorkspaceId,
                    source.Request.Name,
                    source.Request.Definition!));

            CreateMap<RequestImportMappingSource, RequestImportCommand>()
                .ConvertUsing(source => MapRequestImportCommand(source));

            CreateMap<WorkspaceResponse, ConsoleWorkspaceResponse>();

            CreateMap<
                    WorkspaceRequestMappingSource<ConsoleCreateProjectRequest>,
                    CreateProjectCommand>()
                .ForCtorParam(
                    nameof(CreateProjectCommand.WorkspaceId),
                    options => options.MapFrom(source => source.WorkspaceId))
                .ForCtorParam(
                    nameof(CreateProjectCommand.Name),
                    options => options.MapFrom(source => source.Request.Name))
                .ForCtorParam(
                    nameof(CreateProjectCommand.CanonicalRemoteUri),
                    options => options.MapFrom(source =>
                        source.Request.CanonicalRemoteUri))
                .ForCtorParam(
                    nameof(CreateProjectCommand.LocalAliases),
                    options => options.MapFrom(source =>
                        source.Request.LocalAliases));

            CreateMap<
                    WorkspaceRequestMappingSource<
                        ConsoleCreateProjectTaskRequest>,
                    CreateProjectTaskCommand>()
                .ForCtorParam(
                    nameof(CreateProjectTaskCommand.WorkspaceId),
                    options => options.MapFrom(source => source.WorkspaceId))
                .ForCtorParam(
                    nameof(CreateProjectTaskCommand.ProjectId),
                    options => options.MapFrom(source =>
                        source.Request.ProjectId))
                .ForCtorParam(
                    nameof(CreateProjectTaskCommand.Title),
                    options => options.MapFrom(source =>
                        source.Request.Title));

            CreateMap<
                    WorkspaceRequestMappingSource<ConsoleSetBindingRequest>,
                    SetBindingCommand>()
                .ForCtorParam(
                    nameof(SetBindingCommand.WorkspaceId),
                    options => options.MapFrom(source => source.WorkspaceId))
                .ForCtorParam(
                    nameof(SetBindingCommand.ArtifactId),
                    options => options.MapFrom(source =>
                        source.Request.ArtifactId))
                .ForCtorParam(
                    nameof(SetBindingCommand.BindingId),
                    options => options.MapFrom(source =>
                        source.Request.BindingId))
                .ForCtorParam(
                    nameof(SetBindingCommand.OrganizationId),
                    options => options.MapFrom(source =>
                        source.Request.OrganizationId))
                .ForCtorParam(
                    nameof(SetBindingCommand.ProjectId),
                    options => options.MapFrom(source =>
                        source.Request.ProjectId))
                .ForCtorParam(
                    nameof(SetBindingCommand.RepositoryCanonicalUri),
                    options => options.MapFrom(source =>
                        source.Request.RepositoryCanonicalUri))
                .ForCtorParam(
                    nameof(SetBindingCommand.RepositoryRelativePathPrefix),
                    options => options.MapFrom(source =>
                        source.Request.RepositoryRelativePathPrefix))
                .ForCtorParam(
                    nameof(SetBindingCommand.Branch),
                    options => options.MapFrom(source =>
                        source.Request.Branch))
                .ForCtorParam(
                    nameof(SetBindingCommand.TaskId),
                    options => options.MapFrom(source =>
                        source.Request.TaskId))
                .ForCtorParam(
                    nameof(SetBindingCommand.Agent),
                    options => options.MapFrom(source =>
                        source.Request.Agent));

            CreateMap<
                    WorkspaceRequestMappingSource<ConsoleBuildContextRequest>,
                    BuildContextQuery>()
                .ForCtorParam(
                    nameof(BuildContextQuery.WorkspaceId),
                    options => options.MapFrom(source => source.WorkspaceId))
                .ForCtorParam(
                    nameof(BuildContextQuery.ProjectId),
                    options => options.MapFrom(source =>
                        source.Request.ProjectId))
                .ForCtorParam(
                    nameof(BuildContextQuery.TaskId),
                    options => options.MapFrom(source =>
                        source.Request.TaskId))
                .ForCtorParam(
                    nameof(BuildContextQuery.RepositoryRelativePath),
                    options => options.MapFrom(source =>
                        source.Request.RepositoryRelativePath))
                .ForCtorParam(
                    nameof(BuildContextQuery.Branch),
                    options => options.MapFrom(source =>
                        source.Request.Branch))
                .ForCtorParam(
                    nameof(BuildContextQuery.Agent),
                    options => options.MapFrom(source =>
                        source.Request.Agent))
                .ForCtorParam(
                    nameof(BuildContextQuery.TokenBudget),
                    options => options.MapFrom(source =>
                        source.Request.TokenBudget));

            CreateMap<CreateArtifactMappingSource, CreateArtifactCommand>()
                .ForCtorParam(
                    nameof(CreateArtifactCommand.WorkspaceId),
                    options => options.MapFrom(source => source.WorkspaceId))
                .ForCtorParam(
                    nameof(CreateArtifactCommand.Title),
                    options => options.MapFrom(source =>
                        source.Request.Title))
                .ForCtorParam(
                    nameof(CreateArtifactCommand.TypeId),
                    options => options.MapFrom(source =>
                        source.Request.TypeId))
                .ForCtorParam(
                    nameof(CreateArtifactCommand.Content),
                    options => options.MapFrom(source =>
                        source.Request.Content))
                .ForCtorParam(
                    nameof(CreateArtifactCommand.KindTypeId),
                    options => options.MapFrom(source =>
                        source.Request.KindTypeId))
                .ForCtorParam(
                    nameof(CreateArtifactCommand.InstructionRules),
                    options => options.MapFrom(source =>
                        source.Request.InstructionRules))
                .ForCtorParam(
                    nameof(CreateArtifactCommand.PolicyRules),
                    options => options.MapFrom(source =>
                        source.Request.PolicyRules))
                .ForCtorParam(
                    nameof(CreateArtifactCommand.AllowPolicyMutation),
                    options => options.MapFrom(source =>
                        source.AllowPolicyMutation));

            CreateMap<
                    ReviseArtifactMappingSource,
                    AddArtifactRevisionCommand>()
                .ForCtorParam(
                    nameof(AddArtifactRevisionCommand.WorkspaceId),
                    options => options.MapFrom(source => source.WorkspaceId))
                .ForCtorParam(
                    nameof(AddArtifactRevisionCommand.ArtifactId),
                    options => options.MapFrom(source => source.ArtifactId))
                .ForCtorParam(
                    nameof(AddArtifactRevisionCommand.Content),
                    options => options.MapFrom(source =>
                        source.Request.Content))
                .ForCtorParam(
                    nameof(AddArtifactRevisionCommand.InstructionRules),
                    options => options.MapFrom(source =>
                        source.Request.InstructionRules))
                .ForCtorParam(
                    nameof(AddArtifactRevisionCommand.PolicyRules),
                    options => options.MapFrom(source =>
                        source.Request.PolicyRules))
                .ForCtorParam(
                    nameof(AddArtifactRevisionCommand.AllowPolicyMutation),
                    options => options.MapFrom(source =>
                        source.AllowPolicyMutation))
                .ForCtorParam(
                    nameof(AddArtifactRevisionCommand.RequiredKindTypeId),
                    options => options.MapFrom(source =>
                        source.RequiredKindTypeId));

            CreateMap<RememberMemoryMappingSource, RememberMemoryCommand>()
                .ForCtorParam(
                    nameof(RememberMemoryCommand.WorkspaceId),
                    options => options.MapFrom(source => source.WorkspaceId))
                .ForCtorParam(
                    nameof(RememberMemoryCommand.Title),
                    options => options.MapFrom(source =>
                        source.Request.Title))
                .ForCtorParam(
                    nameof(RememberMemoryCommand.Content),
                    options => options.MapFrom(source =>
                        source.Request.Content))
                .ForCtorParam(
                    nameof(RememberMemoryCommand.CategoryTypeId),
                    options => options.MapFrom(source =>
                        source.Request.CategoryTypeId))
                .ForCtorParam(
                    nameof(RememberMemoryCommand.Confidence),
                    options => options.MapFrom(source =>
                        source.Request.Confidence))
                .ForCtorParam(
                    nameof(RememberMemoryCommand.ClientIdentity),
                    options => options.MapFrom(source =>
                        source.ClientIdentity))
                .ForCtorParam(
                    nameof(RememberMemoryCommand.SessionIdentity),
                    options => options.MapFrom(source =>
                        source.SessionIdentity))
                .ForCtorParam(
                    nameof(RememberMemoryCommand.SupersededMemoryId),
                    options => options.MapFrom(source =>
                        source.Request.SupersededMemoryId));
        }

        private static RequestImportCommand MapRequestImportCommand(
            RequestImportMappingSource source)
        {
            ImportOptionsRequest options = source.Request.Options
                                           ?? new ImportOptionsRequest();
            return new RequestImportCommand(
                source.WorkspaceId,
                source.Request.SourceId,
                source.IdempotencyKey,
                new ImportOptions(
                    options.EmbeddingModel,
                    options.ChunkingStrategy,
                    options.MaxCharacters,
                    options.OverlapCharacters,
                    options.SemanticThreshold,
                    options.Separators,
                    options.CodeLanguage));
        }

        private static CreateWorkspaceCommand MapCreateWorkspaceCommand(
            CreateWorkspaceMappingSource source)
        {
            return new CreateWorkspaceCommand(
                source.Request.Name,
                source.Request.TypeId.ToEnumeration<WorkspaceType>()
                ?? throw new InvalidOperationException(
                    $"Workspace type ID '{source.Request.TypeId}' passed validation but could not be resolved."),
                source.Request.OrganizationId,
                source.IdentityIssuer,
                source.IdentitySubject);
        }
    }
}
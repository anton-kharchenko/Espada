using AutoMapper;
using Espada.AgentAdapters.Context;
using Espada.Api.Authentication;
using Espada.Api.Authentication.Constants;
using Espada.Api.Contracts.Models;
using Espada.Api.Contracts.Requests.Imports;
using Espada.Api.Contracts.Requests.Sources;
using Espada.Api.Contracts.Requests.Workspaces;
using Espada.Api.WebConsole;
using Espada.Api.WebConsole.Mappings;
using Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision;
using Espada.Application.UseCases.Artifacts.Commands.CreateArtifact;
using Espada.Application.UseCases.Artifacts.Queries.GetArtifactById;
using Espada.Application.UseCases.Artifacts.Queries.ListArtifactRevisions;
using Espada.Application.UseCases.Artifacts.Queries.ListArtifacts;
using Espada.Application.UseCases.Bindings.Commands.RemoveBinding;
using Espada.Application.UseCases.Bindings.Commands.SetBinding;
using Espada.Application.UseCases.Bindings.Common;
using Espada.Application.UseCases.Bindings.Queries.ListBindings;
using Espada.Application.UseCases.Context.Queries.BuildContext;
using Espada.Application.UseCases.Imports.Commands.CancelImport;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Application.UseCases.Imports.Queries.ListImports;
using Espada.Application.UseCases.Memories.Commands.RememberMemory;
using Espada.Application.UseCases.Memories.Queries.SearchMemory;
using Espada.Application.UseCases.Projects.Commands.CreateProject;
using Espada.Application.UseCases.Projects.Common;
using Espada.Application.UseCases.Projects.Queries.ListProjects;
using Espada.Application.UseCases.ProjectTasks.Commands.ArchiveProjectTask;
using Espada.Application.UseCases.ProjectTasks.Commands.CompleteProjectTask;
using Espada.Application.UseCases.ProjectTasks.Commands.CreateProjectTask;
using Espada.Application.UseCases.ProjectTasks.Queries.ListWorkspaceTasks;
using Espada.Application.UseCases.Sources.Commands.RegisterSource;
using Espada.Application.UseCases.Sources.Queries.ListSources;
using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConsoleBuildContextRequest =
    Espada.Api.WebConsole.Requests.ConsoleBuildContextRequest;
using ConsoleCreateArtifactRequest =
    Espada.Api.WebConsole.Requests.ConsoleCreateArtifactRequest;
using ConsoleCreateProjectRequest =
    Espada.Api.WebConsole.Requests.ConsoleCreateProjectRequest;
using ConsoleCreateProjectTaskRequest =
    Espada.Api.WebConsole.Requests.ConsoleCreateProjectTaskRequest;
using ConsoleRememberMemoryRequest =
    Espada.Api.WebConsole.Requests.ConsoleRememberMemoryRequest;
using ConsoleReviseArtifactRequest =
    Espada.Api.WebConsole.Requests.ConsoleReviseArtifactRequest;
using ConsoleSetBindingRequest =
    Espada.Api.WebConsole.Requests.ConsoleSetBindingRequest;

namespace Espada.Api.Extensions
{
    internal static class WebConsoleWorkspaceEndpointRouteBuilderExtensions
    {
        private const string WebConsoleClientIdentity =
            "espada:web-console";

        public static RouteGroupBuilder MapWebConsoleWorkspaceEndpoints(
            this RouteGroupBuilder protectedBff)
        {
            protectedBff.MapPost(
                    "/workspaces",
                    CreateWorkspaceAsync)
                .Produces<CreateWorkspaceResponse>(
                    StatusCodes.Status201Created);

            RouteGroupBuilder workspace = protectedBff
                .MapGroup("/workspaces/{workspaceId:guid}")
                .AddEndpointFilter<WebConsoleWorkspaceFilter>();

            workspace.MapGet("/projects", ListProjectsAsync)
                .Produces<ListProjectsResponse>();
            workspace.MapPost("/projects", CreateProjectAsync)
                .Produces<ProjectResponse>(
                    StatusCodes.Status201Created);

            workspace.MapGet("/tasks", ListTasksAsync)
                .Produces<ListWorkspaceTasksResponse>();
            workspace.MapPost("/tasks", CreateTaskAsync)
                .Produces<ProjectTaskResponse>(
                    StatusCodes.Status201Created);
            workspace.MapPost(
                    "/tasks/{taskId:guid}/complete",
                    CompleteTaskAsync)
                .Produces<ProjectTaskResponse>();
            workspace.MapPost(
                    "/tasks/{taskId:guid}/archive",
                    ArchiveTaskAsync)
                .Produces<ProjectTaskResponse>();

            workspace.MapGet("/artifacts", ListArtifactsAsync)
                .Produces<ListArtifactsResponse>();
            workspace.MapPost("/artifacts", CreateArtifactAsync)
                .Produces<CreateArtifactResponse>(
                    StatusCodes.Status201Created);
            workspace.MapGet(
                    "/artifacts/{artifactId:guid}",
                    GetArtifactAsync)
                .Produces<GetArtifactByIdResponse>();
            workspace.MapGet(
                    "/artifacts/{artifactId:guid}/revisions",
                    ListArtifactRevisionsAsync)
                .Produces<ListArtifactRevisionsResponse>();
            workspace.MapPost(
                    "/artifacts/{artifactId:guid}/revisions",
                    ReviseArtifactAsync)
                .Produces<AddArtifactRevisionResponse>(
                    StatusCodes.Status201Created);

            workspace.MapGet("/instructions", ListInstructionsAsync)
                .Produces<ListArtifactsResponse>();
            workspace.MapPost("/instructions", CreateInstructionAsync)
                .Produces<CreateArtifactResponse>(
                    StatusCodes.Status201Created);
            workspace.MapPost(
                    "/instructions/{artifactId:guid}/revisions",
                    ReviseInstructionAsync)
                .Produces<AddArtifactRevisionResponse>(
                    StatusCodes.Status201Created);

            workspace.MapGet("/policies", ListPoliciesAsync)
                .Produces<ListArtifactsResponse>();
            RouteGroupBuilder policyAdministration = workspace
                .MapGroup("/policies")
                .AddEndpointFilter<WebConsoleOwnerFilter>();
            policyAdministration.MapPost(
                    string.Empty,
                    CreatePolicyAsync)
                .Produces<CreateArtifactResponse>(
                    StatusCodes.Status201Created);
            policyAdministration.MapPost(
                    "/{artifactId:guid}/revisions",
                    RevisePolicyAsync)
                .Produces<AddArtifactRevisionResponse>(
                    StatusCodes.Status201Created);

            workspace.MapGet("/bindings", ListBindingsAsync)
                .Produces<ListBindingsResponse>();
            workspace.MapPost("/bindings", SetBindingAsync)
                .Produces<BindingResponse>();
            workspace.MapDelete(
                "/bindings/{bindingId:guid}",
                RemoveBindingAsync);

            workspace.MapPost("/memories", RememberMemoryAsync)
                .Produces<RememberMemoryResponse>(
                    StatusCodes.Status201Created);
            workspace.MapGet("/memories/search", SearchMemoryAsync)
                .Produces<SearchMemoryResponse>();

            workspace.MapGet("/sources", ListSourcesAsync)
                .Produces<ListSourcesResponse>();
            workspace.MapPost("/sources", RegisterSourceAsync)
                .Produces<RegisterSourceResponse>(
                    StatusCodes.Status201Created);

            workspace.MapGet("/imports", ListImportsAsync)
                .Produces<ListImportsResponse>();
            workspace.MapPost("/imports", RequestImportAsync)
                .Produces<RequestImportResponse>(
                    StatusCodes.Status202Accepted);
            workspace.MapPost(
                "/imports/{importJobId:guid}/cancel",
                CancelImportAsync);

            workspace.MapPost("/context", BuildContextAsync)
                .Produces<ConsoleContextBuildResponse>();

            return protectedBff;
        }

        private static async Task<IResult> CreateWorkspaceAsync(
            CreateWorkspaceRequest request,
            ClaimsPrincipal user,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            string? issuer = user.FindFirstValue(
                WebConsoleAuthenticationConstants.IdentityIssuerClaim);
            string? subject = user.FindFirstValue(
                WebConsoleAuthenticationConstants.IdentitySubjectClaim);
            if (string.IsNullOrWhiteSpace(issuer)
                || string.IsNullOrWhiteSpace(subject))
            {
                return Results.Unauthorized();
            }

            CreateWorkspaceCommand command =
                mapper.Map<CreateWorkspaceCommand>(
                    new CreateWorkspaceMappingSource(
                        request,
                        issuer,
                        subject));
            DomainResult<CreateWorkspaceResponse> result =
                await mediator.Send(command, cancellationToken);

            return WebConsoleResults.From(
                result,
                StatusCodes.Status201Created);
        }

        private static async Task<IResult> ListProjectsAsync(
            Guid workspaceId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            DomainResult<ListProjectsResponse> result =
                await mediator.Send(
                    new ListProjectsQuery(workspaceId),
                    cancellationToken);
            return WebConsoleResults.From(result);
        }

        private static async Task<IResult> CreateProjectAsync(
            Guid workspaceId,
            ConsoleCreateProjectRequest request,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            CreateProjectCommand command =
                mapper.Map<CreateProjectCommand>(
                    new WorkspaceRequestMappingSource<
                        ConsoleCreateProjectRequest>(
                        workspaceId,
                        request));
            DomainResult<ProjectResponse> result =
                await mediator.Send(command, cancellationToken);
            return WebConsoleResults.From(
                result,
                StatusCodes.Status201Created);
        }

        private static async Task<IResult> ListTasksAsync(
            Guid workspaceId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            DomainResult<ListWorkspaceTasksResponse> result =
                await mediator.Send(
                    new ListWorkspaceTasksQuery(workspaceId),
                    cancellationToken);
            return WebConsoleResults.From(result);
        }

        private static async Task<IResult> CreateTaskAsync(
            Guid workspaceId,
            ConsoleCreateProjectTaskRequest request,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            CreateProjectTaskCommand command =
                mapper.Map<CreateProjectTaskCommand>(
                    new WorkspaceRequestMappingSource<
                        ConsoleCreateProjectTaskRequest>(
                        workspaceId,
                        request));
            DomainResult<ProjectTaskResponse> result =
                await mediator.Send(command, cancellationToken);
            return WebConsoleResults.From(
                result,
                StatusCodes.Status201Created);
        }

        private static async Task<IResult> CompleteTaskAsync(
            Guid workspaceId,
            Guid taskId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            DomainResult<ProjectTaskResponse> result =
                await mediator.Send(
                    new CompleteProjectTaskCommand(
                        workspaceId,
                        taskId),
                    cancellationToken);
            return WebConsoleResults.From(result);
        }

        private static async Task<IResult> ArchiveTaskAsync(
            Guid workspaceId,
            Guid taskId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            DomainResult<ProjectTaskResponse> result =
                await mediator.Send(
                    new ArchiveProjectTaskCommand(
                        workspaceId,
                        taskId),
                    cancellationToken);
            return WebConsoleResults.From(result);
        }

        private static Task<IResult> ListArtifactsAsync(
            Guid workspaceId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            return ListArtifactsByKindAsync(
                workspaceId,
                null,
                mediator,
                cancellationToken);
        }

        private static Task<IResult> ListInstructionsAsync(
            Guid workspaceId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            return ListArtifactsByKindAsync(
                workspaceId,
                ArtifactKindType.Instruction.Id,
                mediator,
                cancellationToken);
        }

        private static Task<IResult> ListPoliciesAsync(
            Guid workspaceId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            return ListArtifactsByKindAsync(
                workspaceId,
                ArtifactKindType.Policy.Id,
                mediator,
                cancellationToken);
        }

        private static async Task<IResult> ListArtifactsByKindAsync(
            Guid workspaceId,
            int? kindTypeId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            DomainResult<ListArtifactsResponse> result =
                await mediator.Send(
                    new ListArtifactsQuery(
                        workspaceId,
                        kindTypeId),
                    cancellationToken);
            return WebConsoleResults.From(result);
        }

        private static Task<IResult> CreateArtifactAsync(
            Guid workspaceId,
            ConsoleCreateArtifactRequest request,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            if (request.KindTypeId == ArtifactKindType.Policy.Id)
            {
                return Task.FromResult(
                    WebConsoleResults.Forbidden(
                        "Use the owner-only policy endpoint to create policies."));
            }

            return CreateArtifactCoreAsync(
                workspaceId,
                request,
                false,
                mediator,
                mapper,
                cancellationToken);
        }

        private static Task<IResult> CreateInstructionAsync(
            Guid workspaceId,
            ConsoleCreateArtifactRequest request,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            return CreateArtifactCoreAsync(
                workspaceId,
                request with
                {
                    KindTypeId = ArtifactKindType.Instruction.Id,
                    PolicyRules = null
                },
                false,
                mediator,
                mapper,
                cancellationToken);
        }

        private static Task<IResult> CreatePolicyAsync(
            Guid workspaceId,
            ConsoleCreateArtifactRequest request,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            return CreateArtifactCoreAsync(
                workspaceId,
                request with
                {
                    KindTypeId = ArtifactKindType.Policy.Id,
                    InstructionRules = null
                },
                true,
                mediator,
                mapper,
                cancellationToken);
        }

        private static async Task<IResult> CreateArtifactCoreAsync(
            Guid workspaceId,
            ConsoleCreateArtifactRequest request,
            bool allowPolicyMutation,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            CreateArtifactCommand command =
                mapper.Map<CreateArtifactCommand>(
                    new CreateArtifactMappingSource(
                        workspaceId,
                        request,
                        allowPolicyMutation));
            DomainResult<CreateArtifactResponse> result =
                await mediator.Send(command, cancellationToken);
            return WebConsoleResults.From(
                result,
                StatusCodes.Status201Created);
        }

        private static async Task<IResult> GetArtifactAsync(
            Guid workspaceId,
            Guid artifactId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            DomainResult<GetArtifactByIdResponse> result =
                await mediator.Send(
                    new GetArtifactByIdQuery(
                        workspaceId,
                        artifactId),
                    cancellationToken);
            return WebConsoleResults.From(result);
        }

        private static async Task<IResult> ListArtifactRevisionsAsync(
            Guid workspaceId,
            Guid artifactId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            DomainResult<ListArtifactRevisionsResponse> result =
                await mediator.Send(
                    new ListArtifactRevisionsQuery(
                        workspaceId,
                        artifactId),
                    cancellationToken);
            return WebConsoleResults.From(result);
        }

        private static Task<IResult> ReviseArtifactAsync(
            Guid workspaceId,
            Guid artifactId,
            ConsoleReviseArtifactRequest request,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            return ReviseArtifactCoreAsync(
                workspaceId,
                artifactId,
                request,
                false,
                null,
                mediator,
                mapper,
                cancellationToken);
        }

        private static Task<IResult> ReviseInstructionAsync(
            Guid workspaceId,
            Guid artifactId,
            ConsoleReviseArtifactRequest request,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            return ReviseArtifactCoreAsync(
                workspaceId,
                artifactId,
                request with { PolicyRules = null },
                false,
                ArtifactKindType.Instruction.Id,
                mediator,
                mapper,
                cancellationToken);
        }

        private static Task<IResult> RevisePolicyAsync(
            Guid workspaceId,
            Guid artifactId,
            ConsoleReviseArtifactRequest request,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            return ReviseArtifactCoreAsync(
                workspaceId,
                artifactId,
                request with { InstructionRules = null },
                true,
                ArtifactKindType.Policy.Id,
                mediator,
                mapper,
                cancellationToken);
        }

        private static async Task<IResult> ReviseArtifactCoreAsync(
            Guid workspaceId,
            Guid artifactId,
            ConsoleReviseArtifactRequest request,
            bool allowPolicyMutation,
            int? requiredKindTypeId,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            AddArtifactRevisionCommand command =
                mapper.Map<AddArtifactRevisionCommand>(
                    new ReviseArtifactMappingSource(
                        workspaceId,
                        artifactId,
                        request,
                        allowPolicyMutation,
                        requiredKindTypeId));
            DomainResult<AddArtifactRevisionResponse> result =
                await mediator.Send(command, cancellationToken);
            return WebConsoleResults.From(
                result,
                StatusCodes.Status201Created);
        }

        private static async Task<IResult> ListBindingsAsync(
            Guid workspaceId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            DomainResult<ListBindingsResponse> result =
                await mediator.Send(
                    new ListBindingsQuery(workspaceId),
                    cancellationToken);
            return WebConsoleResults.From(result);
        }

        private static async Task<IResult> SetBindingAsync(
            Guid workspaceId,
            ConsoleSetBindingRequest request,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            SetBindingCommand command =
                mapper.Map<SetBindingCommand>(
                    new WorkspaceRequestMappingSource<
                        ConsoleSetBindingRequest>(
                        workspaceId,
                        request));
            DomainResult<BindingResponse> result =
                await mediator.Send(command, cancellationToken);
            return WebConsoleResults.From(result);
        }

        private static async Task<IResult> RemoveBindingAsync(
            Guid workspaceId,
            Guid bindingId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            DomainResult result = await mediator.Send(
                new RemoveBindingCommand(workspaceId, bindingId),
                cancellationToken);
            return WebConsoleResults.From(result);
        }

        private static async Task<IResult> RememberMemoryAsync(
            Guid workspaceId,
            ConsoleRememberMemoryRequest request,
            ClaimsPrincipal user,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            string? sessionIdentity = user.FindFirstValue(
                WebConsoleAuthenticationConstants.SessionIdentityClaim);
            if (string.IsNullOrWhiteSpace(sessionIdentity))
            {
                return Results.Unauthorized();
            }

            RememberMemoryCommand command =
                mapper.Map<RememberMemoryCommand>(
                    new RememberMemoryMappingSource(
                        workspaceId,
                        request,
                        WebConsoleClientIdentity,
                        sessionIdentity));
            DomainResult<RememberMemoryResponse> result =
                await mediator.Send(command, cancellationToken);
            return WebConsoleResults.From(
                result,
                StatusCodes.Status201Created);
        }

        private static async Task<IResult> SearchMemoryAsync(
            Guid workspaceId,
            [FromQuery(Name = "q")] string query,
            [FromQuery] int topK,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            int requestedTopK = topK == 0 ? 10 : topK;
            DomainResult<SearchMemoryResponse> result =
                await mediator.Send(
                    new SearchMemoryQuery(
                        workspaceId,
                        query,
                        TopK: requestedTopK),
                    cancellationToken);
            return WebConsoleResults.From(result);
        }

        private static async Task<IResult> ListSourcesAsync(
            Guid workspaceId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            DomainResult<ListSourcesResponse> result =
                await mediator.Send(
                    new ListSourcesQuery(workspaceId),
                    cancellationToken);
            return WebConsoleResults.From(result);
        }

        private static async Task<IResult> RegisterSourceAsync(
            Guid workspaceId,
            RegisterSourceRequest request,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            if (request.Definition is null)
            {
                return Results.BadRequest(
                    new
                    {
                        code = "invalid_argument",
                        message = "A source definition is required."
                    });
            }

            RegisterSourceCommand command =
                mapper.Map<RegisterSourceCommand>(
                    new RegisterSourceMappingSource(
                        workspaceId,
                        request));
            DomainResult<RegisterSourceResponse> result =
                await mediator.Send(command, cancellationToken);
            return WebConsoleResults.From(
                result,
                StatusCodes.Status201Created);
        }

        private static async Task<IResult> ListImportsAsync(
            Guid workspaceId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            DomainResult<ListImportsResponse> result =
                await mediator.Send(
                    new ListImportsQuery(workspaceId),
                    cancellationToken);
            return WebConsoleResults.From(result);
        }

        private static async Task<IResult> RequestImportAsync(
            Guid workspaceId,
            RequestImportRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return Results.BadRequest(
                    new
                    {
                        code = "invalid_argument",
                        message = "The Idempotency-Key header is required."
                    });
            }

            RequestImportCommand command =
                mapper.Map<RequestImportCommand>(
                    new RequestImportMappingSource(
                        workspaceId,
                        idempotencyKey,
                        request));
            DomainResult<RequestImportResponse> result =
                await mediator.Send(command, cancellationToken);
            return WebConsoleResults.From(
                result,
                StatusCodes.Status202Accepted);
        }

        private static async Task<IResult> CancelImportAsync(
            Guid workspaceId,
            Guid importJobId,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            DomainResult result =
                await mediator.Send(
                    new CancelImportCommand(
                        workspaceId,
                        importJobId),
                    cancellationToken);
            return WebConsoleResults.From(result);
        }

        private static async Task<IResult> BuildContextAsync(
            Guid workspaceId,
            ConsoleBuildContextRequest request,
            IMediator mediator,
            IMapper mapper,
            CancellationToken cancellationToken)
        {
            BuildContextQuery query =
                mapper.Map<BuildContextQuery>(
                    new WorkspaceRequestMappingSource<
                        ConsoleBuildContextRequest>(
                        workspaceId,
                        request));
            DomainResult<BuildContextResponse> result =
                await mediator.Send(query, cancellationToken);
            if (result.IsFailure)
            {
                return WebConsoleResults.From(result);
            }

            AgentContextProjection projection =
                AgentContextProjectionRenderer.Render(result.Value);
            return Results.Ok(
                new ConsoleContextBuildResponse(
                    result.Value,
                    projection));
        }
    }
}

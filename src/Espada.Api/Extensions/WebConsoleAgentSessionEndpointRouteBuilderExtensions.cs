using Espada.Api.Contracts.Requests.AgentSessions;
using Espada.Api.Contracts.Responses.AgentSessions;
using Espada.Application.Contracts.Agents;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Models.Agents;
using Espada.Application.UseCases.AgentSessions.Commands.StartAgentSessions;
using Espada.Domain.Aggregates;
using Espada.Domain.Entities;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Devices;
using Espada.LocalSetup.Contracts;
using MediatR;
using System.Text.Json;

namespace Espada.Api.Extensions
{
    internal static class WebConsoleAgentSessionEndpointRouteBuilderExtensions
    {
        public static RouteGroupBuilder MapAgentSessionEndpoints(this RouteGroupBuilder workspace)
        {
            workspace.MapGet("/agent-sessions/options", OptionsAsync);
            workspace.MapGet("/agent-sessions", ListAsync);
            workspace.MapPost("/agent-sessions", StartAsync)
                .Produces<StartAgentSessionsResponse>(StatusCodes.Status201Created);
            workspace.MapGet("/agent-sessions/{sessionId:guid}/events", EventsAsync);
            workspace.MapGet("/agent-sessions/{sessionId:guid}/events/stream", StreamAsync);
            workspace.MapGet("/agent-sessions/{sessionId:guid}/approvals", ApprovalsAsync);
            workspace.MapPost("/agent-sessions/{sessionId:guid}/approvals/{approvalId:guid}/decision", DecideAsync);
            workspace.MapPost("/agent-sessions/{sessionId:guid}/cancel", CancelAsync);
            workspace.MapPost("/agent-sessions/{sessionId:guid}/apply", ApplyAsync);
            workspace.MapDelete("/agent-sessions/{sessionId:guid}/worktree", RemoveWorktreeAsync);
            return workspace;
        }

        private static async Task<IResult> StartAsync(Guid workspaceId, StartAgentSessionsRequest request,
            IMediator mediator, CancellationToken cancellationToken)
        {
            DomainResult<StartAgentSessionsResponse> result = await mediator.Send(new StartAgentSessionsCommand(
                workspaceId, request.ProjectId, request.DeviceId, request.Prompt, request.AgentProfileIds),
                cancellationToken);
            return WebConsoleResults.From(result, StatusCodes.Status201Created);
        }

        private static async Task<IResult> OptionsAsync(Guid workspaceId, LocalDeviceIdentityStore identityStore,
            IAgentProfileRepository profiles, IAgentInstallationRepository installations,
            CancellationToken cancellationToken)
        {
            Guid deviceId = identityStore.GetOrCreate();
            IReadOnlyList<AgentProfile> workspaceProfiles = await profiles.ListByWorkspaceIdAsync(
                WorkspaceId.Create(workspaceId), cancellationToken);
            IReadOnlyList<AgentInstallation> deviceInstallations = await installations.ListByDeviceIdAsync(
                DeviceId.Create(deviceId), cancellationToken);
            AgentOptionResponse[] agents = Enumeration.GetAll<AgentVendorType>()
                .OrderBy(vendor => vendor.Id)
                .Select(vendor =>
                {
                    AgentProfile? profile = workspaceProfiles.FirstOrDefault(item => item.Vendor == vendor);
                    AgentInstallation? installation =
                        deviceInstallations.FirstOrDefault(item => item.Vendor == vendor);
                    return new AgentOptionResponse(vendor.Id, vendor.Name, profile?.Id.Value,
                        installation is not null, installation?.IsAuthenticated == true);
                })
                .ToArray();
            return Results.Ok(new AgentOptionsResponse(deviceId, agents));
        }
        private static async Task<IResult> ListAsync(Guid workspaceId, IAgentSessionRepository sessions,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<AgentSession> values = await sessions.ListByWorkspaceIdAsync(
                WorkspaceId.Create(workspaceId), cancellationToken);
            return Results.Ok(values.Select(ToResponse));
        }

        private static async Task<IResult> EventsAsync(Guid workspaceId, Guid sessionId, long after,
            IAgentSessionRepository sessions, IAgentSessionEventRepository events,
            CancellationToken cancellationToken)
        {
            AgentSession? session = await FindSessionAsync(workspaceId, sessionId, sessions, cancellationToken);
            if (session is null)
            {
                return Results.NotFound();
            }

            IReadOnlyList<AgentSessionEvent> values =
                await events.ListBySessionIdAsync(session.Id, after, cancellationToken);
            return Results.Ok(values.Select(ToResponse));
        }

        private static async Task StreamAsync(Guid workspaceId, Guid sessionId, long? after,
            HttpContext context, IAgentSessionRepository sessions, IAgentSessionEventRepository events,
            CancellationToken cancellationToken)
        {
            AgentSession? session = await FindSessionAsync(workspaceId, sessionId, sessions, cancellationToken);
            if (session is null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            context.Response.Headers.ContentType = "text/event-stream";
            context.Response.Headers.CacheControl = "no-cache";
            long sequence = after.GetValueOrDefault();
            while (!cancellationToken.IsCancellationRequested)
            {
                IReadOnlyList<AgentSessionEvent> values =
                    await events.ListBySessionIdAsync(session.Id, sequence, cancellationToken);
                foreach (AgentSessionEvent sessionEvent in values)
                {
                    AgentSessionEventResponse response = ToResponse(sessionEvent);
                    await context.Response.WriteAsync($"id: {response.Sequence}\n", cancellationToken);
                    await context.Response.WriteAsync($"data: {JsonSerializer.Serialize(response)}\n\n",
                        cancellationToken);
                    sequence = response.Sequence;
                }

                await context.Response.Body.FlushAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            }
        }
        private static async Task<IResult> ApprovalsAsync(Guid workspaceId, Guid sessionId,
            IAgentSessionRepository sessions, IAgentApprovalRepository approvals,
            CancellationToken cancellationToken)
        {
            AgentSession? session = await FindSessionAsync(workspaceId, sessionId, sessions, cancellationToken);
            if (session is null)
            {
                return Results.NotFound();
            }

            IReadOnlyList<AgentApproval> values =
                await approvals.ListBySessionIdAsync(session.Id, cancellationToken);
            return Results.Ok(values.Select(ToResponse));
        }

        private static async Task<IResult> DecideAsync(Guid workspaceId, Guid sessionId, Guid approvalId,
            AgentApprovalDecisionRequest request, IAgentSessionRepository sessions,
            IAgentApprovalRepository approvals, IAgentSessionExecutionQueue executionQueue,
            CancellationToken cancellationToken)
        {
            AgentSession? session = await FindSessionAsync(workspaceId, sessionId, sessions, cancellationToken);
            AgentApproval? approval = await approvals.GetByIdAsync(AgentApprovalId.Create(approvalId),
                cancellationToken);
            if (session is null || approval is null || approval.AgentSessionId != session.Id)
            {
                return Results.NotFound();
            }

            bool accepted = await executionQueue.DecideApprovalAsync(approval.Id, request.Approved,
                cancellationToken);
            return accepted ? Results.Ok(new { accepted = true }) : Results.Conflict();
        }

        private static async Task<IResult> CancelAsync(Guid workspaceId, Guid sessionId,
            IAgentSessionRepository sessions, IAgentSessionExecutionQueue executionQueue,
            CancellationToken cancellationToken)
        {
            AgentSession? session = await FindSessionAsync(workspaceId, sessionId, sessions, cancellationToken);
            if (session is null)
            {
                return Results.NotFound();
            }

            return await executionQueue.CancelAsync(session.Id, cancellationToken)
                ? Results.Accepted()
                : Results.Conflict();
        }

        private static async Task<IResult> ApplyAsync(Guid workspaceId, Guid sessionId,
            IAgentSessionRepository sessions, IProjectRepository projects, IAgentWorktreeService worktreeService,
            CancellationToken cancellationToken)
        {
            AgentSession? session = await FindSessionAsync(workspaceId, sessionId, sessions, cancellationToken);
            if (session is null)
            {
                return Results.NotFound();
            }

            Project? project = await projects.GetByIdAsync(session.ProjectId, cancellationToken);
            string? root = project?.LocalAliases.FirstOrDefault(Directory.Exists);
            if (project is null || root is null)
            {
                return Results.NotFound();
            }

            DomainResult result = await worktreeService.ApplyAsync(project,
                new AgentWorktree(Path.GetFullPath(root), session.BranchName, session.WorktreePath),
                cancellationToken);
            return result.IsFailure ? WebConsoleResults.From(result) : Results.NoContent();
        }

        private static async Task<IResult> RemoveWorktreeAsync(Guid workspaceId, Guid sessionId,
            IAgentSessionRepository sessions, IProjectRepository projects, IAgentWorktreeService worktreeService,
            CancellationToken cancellationToken)
        {
            AgentSession? session = await FindSessionAsync(workspaceId, sessionId, sessions, cancellationToken);
            if (session is null)
            {
                return Results.NotFound();
            }

            if (session.FinishedAtUtc is null)
            {
                return Results.Conflict(new { message = "Stop the session before removing its worktree." });
            }

            Project? project = await projects.GetByIdAsync(session.ProjectId, cancellationToken);
            string? root = project?.LocalAliases.FirstOrDefault(Directory.Exists);
            if (project is null || root is null)
            {
                return Results.NotFound();
            }

            DomainResult result = await worktreeService.RemoveAsync(project,
                new AgentWorktree(Path.GetFullPath(root), session.BranchName, session.WorktreePath),
                cancellationToken);
            return result.IsFailure ? WebConsoleResults.From(result) : Results.NoContent();
        }

        private static async Task<AgentSession?> FindSessionAsync(Guid workspaceId, Guid sessionId,
            IAgentSessionRepository sessions, CancellationToken cancellationToken)
        {
            AgentSession? session = await sessions.GetByIdAsync(AgentSessionId.Create(sessionId), cancellationToken);
            return session?.WorkspaceId.Value == workspaceId ? session : null;
        }

        private static AgentSessionResponse ToResponse(AgentSession session)
        {
            return new AgentSessionResponse(session.Id.Value, session.ProjectId.Value, session.AgentProfileId.Value,
                session.Prompt, session.BranchName, session.Status.Name, session.CreatedAtUtc, session.FinishedAtUtc);
        }

        private static AgentSessionEventResponse ToResponse(AgentSessionEvent sessionEvent)
        {
            return new AgentSessionEventResponse(sessionEvent.Id.Value, sessionEvent.Sequence, sessionEvent.Type.Name,
                sessionEvent.PayloadJson, sessionEvent.OccurredAtUtc);
        }

        private static AgentApprovalResponse ToResponse(AgentApproval approval)
        {
            return new AgentApprovalResponse(approval.Id.Value, approval.AgentSessionId.Value, approval.ToolName,
                approval.ArgumentsJson, approval.Status.Name, approval.RequestedAtUtc, approval.DecidedAtUtc);
        }
    }
}
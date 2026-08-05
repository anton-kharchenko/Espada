using Espada.Api.Contracts.Requests.AgentSessions;
using Espada.Api.Contracts.Responses.AgentSessions;
using Espada.Application.Contracts.Agents;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Models.Agents;
using Espada.Application.UseCases.AgentSessions.Commands.StartAgentSessions;
using Espada.Domain.Aggregates;
using Espada.Domain.Entities;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Espada.Api.Controllers
{
    [Route("api/v{version:apiVersion}/workspaces/{workspaceId:guid}/agent-sessions")]
    public sealed class AgentSessionsController(
        IMediator mediator,
        IAgentSessionRepository sessionRepository,
        IAgentSessionEventRepository eventRepository,
        IAgentApprovalRepository approvalRepository,
        IProjectRepository projectRepository,
        IAgentSessionExecutionQueue executionQueue,
        IAgentWorktreeService worktreeService) : BaseController
    {
        [HttpPost]
        [ProducesResponseType(typeof(StartAgentSessionsResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> Start([FromRoute] Guid workspaceId,
            [FromBody] StartAgentSessionsRequest request, CancellationToken cancellationToken)
        {
            DomainResult<StartAgentSessionsResponse> result = await mediator.Send(new StartAgentSessionsCommand(
                workspaceId, request.ProjectId, request.DeviceId, request.Prompt, request.AgentProfileIds),
                cancellationToken);
            return result.IsFailure
                ? HandleError(result.Error)
                : StatusCode(StatusCodes.Status201Created, result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<AgentSessionResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromRoute] Guid workspaceId, CancellationToken cancellationToken)
        {
            IReadOnlyList<AgentSession> sessions = await sessionRepository.ListByWorkspaceIdAsync(
                WorkspaceId.Create(workspaceId), cancellationToken);
            return Ok(sessions.Select(ToResponse));
        }

        [HttpGet("{sessionId:guid}/events")]
        [ProducesResponseType(typeof(IReadOnlyList<AgentSessionEventResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Events([FromRoute] Guid workspaceId, [FromRoute] Guid sessionId,
            [FromQuery] long after = 0, CancellationToken cancellationToken = default)
        {
            AgentSession? session = await FindSessionAsync(workspaceId, sessionId, cancellationToken);
            if (session is null)
            {
                return NotFound();
            }

            IReadOnlyList<AgentSessionEvent> events = await eventRepository.ListBySessionIdAsync(session.Id, after,
                cancellationToken);
            return Ok(events.Select(ToResponse));
        }

        [HttpGet("{sessionId:guid}/events/stream")]
        public async Task Stream([FromRoute] Guid workspaceId, [FromRoute] Guid sessionId,
            [FromQuery] long after = 0, CancellationToken cancellationToken = default)
        {
            AgentSession? session = await FindSessionAsync(workspaceId, sessionId, cancellationToken);
            if (session is null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            Response.Headers.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";
            long sequence = after;
            while (!cancellationToken.IsCancellationRequested)
            {
                IReadOnlyList<AgentSessionEvent> events = await eventRepository.ListBySessionIdAsync(
                    session.Id, sequence, cancellationToken);
                foreach (AgentSessionEvent sessionEvent in events)
                {
                    AgentSessionEventResponse response = ToResponse(sessionEvent);
                    await Response.WriteAsync($"id: {response.Sequence}\n", cancellationToken);
                    await Response.WriteAsync($"event: {response.Type}\n", cancellationToken);
                    await Response.WriteAsync($"data: {JsonSerializer.Serialize(response)}\n\n",
                        cancellationToken);
                    sequence = response.Sequence;
                }

                await Response.Body.FlushAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            }
        }

        [HttpGet("{sessionId:guid}/approvals")]
        [ProducesResponseType(typeof(IReadOnlyList<AgentApprovalResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Approvals([FromRoute] Guid workspaceId, [FromRoute] Guid sessionId,
            CancellationToken cancellationToken)
        {
            AgentSession? session = await FindSessionAsync(workspaceId, sessionId, cancellationToken);
            if (session is null)
            {
                return NotFound();
            }

            IReadOnlyList<AgentApproval> approvals =
                await approvalRepository.ListBySessionIdAsync(session.Id, cancellationToken);
            return Ok(approvals.Select(ToResponse));
        }

        [HttpPost("{sessionId:guid}/approvals/{approvalId:guid}/decision")]
        public async Task<IActionResult> Decide([FromRoute] Guid workspaceId, [FromRoute] Guid sessionId,
            [FromRoute] Guid approvalId, [FromBody] AgentApprovalDecisionRequest request,
            CancellationToken cancellationToken)
        {
            AgentSession? session = await FindSessionAsync(workspaceId, sessionId, cancellationToken);
            AgentApproval? approval = await approvalRepository.GetByIdAsync(AgentApprovalId.Create(approvalId),
                cancellationToken);
            if (session is null || approval is null || approval.AgentSessionId != session.Id)
            {
                return NotFound();
            }

            bool accepted = await executionQueue.DecideApprovalAsync(approval.Id, request.Approved,
                cancellationToken);
            return accepted ? Accepted() : Conflict();
        }

        [HttpPost("{sessionId:guid}/cancel")]
        public async Task<IActionResult> Cancel([FromRoute] Guid workspaceId, [FromRoute] Guid sessionId,
            CancellationToken cancellationToken)
        {
            AgentSession? session = await FindSessionAsync(workspaceId, sessionId, cancellationToken);
            if (session is null)
            {
                return NotFound();
            }

            return await executionQueue.CancelAsync(session.Id, cancellationToken) ? Accepted() : Conflict();
        }

        [HttpPost("{sessionId:guid}/apply")]
        public async Task<IActionResult> Apply([FromRoute] Guid workspaceId, [FromRoute] Guid sessionId,
            CancellationToken cancellationToken)
        {
            AgentSession? session = await FindSessionAsync(workspaceId, sessionId, cancellationToken);
            if (session is null)
            {
                return NotFound();
            }

            Project? project = await projectRepository.GetByIdAsync(session.ProjectId, cancellationToken);
            string? root = project?.LocalAliases.FirstOrDefault(Directory.Exists);
            if (project is null || root is null)
            {
                return NotFound();
            }

            DomainResult result = await worktreeService.ApplyAsync(project,
                new AgentWorktree(Path.GetFullPath(root), session.BranchName, session.WorktreePath),
                cancellationToken);
            return result.IsFailure ? HandleError(result.Error) : NoContent();
        }

        [HttpDelete("{sessionId:guid}/worktree")]
        public async Task<IActionResult> RemoveWorktree([FromRoute] Guid workspaceId, [FromRoute] Guid sessionId,
            CancellationToken cancellationToken)
        {
            AgentSession? session = await FindSessionAsync(workspaceId, sessionId, cancellationToken);
            if (session is null)
            {
                return NotFound();
            }

            if (session.FinishedAtUtc is null)
            {
                return Conflict(new { message = "Stop the session before removing its worktree." });
            }

            Project? project = await projectRepository.GetByIdAsync(session.ProjectId, cancellationToken);
            string? root = project?.LocalAliases.FirstOrDefault(Directory.Exists);
            if (project is null || root is null)
            {
                return NotFound();
            }

            DomainResult result = await worktreeService.RemoveAsync(project,
                new AgentWorktree(Path.GetFullPath(root), session.BranchName, session.WorktreePath),
                cancellationToken);
            return result.IsFailure ? HandleError(result.Error) : NoContent();
        }

        private async Task<AgentSession?> FindSessionAsync(Guid workspaceId, Guid sessionId,
            CancellationToken cancellationToken)
        {
            AgentSession? session = await sessionRepository.GetByIdAsync(AgentSessionId.Create(sessionId),
                cancellationToken);
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
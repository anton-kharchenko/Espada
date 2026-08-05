using Espada.AgentAdapters.Models;
using Espada.AgentAdapters.Processes;
using Espada.Application.Contracts.Agents;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.Models.Agents;
using Espada.Domain.Aggregates;
using Espada.Domain.Entities;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;

namespace Espada.AgentAdapters.Execution
{
    public sealed class AgentSessionExecutionService(
        IServiceScopeFactory scopeFactory,
        IEnumerable<IAgentProcessClient> processClients) : BackgroundService, IAgentSessionExecutionQueue
    {
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _activeSessions = new();
        private readonly ConcurrentDictionary<Guid, PendingAgentApproval> _pendingApprovals = new();
        private readonly IReadOnlyDictionary<int, IAgentProcessClient> _processClients =
            processClients.ToDictionary(client => client.VendorId);
        private readonly Channel<AgentSessionExecution> _queue =
            Channel.CreateBounded<AgentSessionExecution>(new BoundedChannelOptions(32)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });

        public ValueTask QueueAsync(AgentSessionExecution execution,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(execution);
            return _queue.Writer.WriteAsync(execution, cancellationToken);
        }

        public async Task<bool> DecideApprovalAsync(AgentApprovalId approvalId, bool approved,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(approvalId);
            cancellationToken.ThrowIfCancellationRequested();
            if (_pendingApprovals.TryGetValue(approvalId.Value, out PendingAgentApproval? pending))
            {
                return pending.Completion.TrySetResult(approved);
            }

            using IServiceScope scope = scopeFactory.CreateScope();
            IAgentApprovalRepository approvals =
                scope.ServiceProvider.GetRequiredService<IAgentApprovalRepository>();
            AgentApproval? approval = await approvals.GetByIdAsync(approvalId, cancellationToken);
            if (approval is null || !approval.Status.Equals(AgentApprovalStatusType.Pending))
            {
                return false;
            }

            IClockService clock = scope.ServiceProvider.GetRequiredService<IClockService>();
            AgentSession? session = await scope.ServiceProvider.GetRequiredService<IAgentSessionRepository>()
                .GetByIdAsync(approval.AgentSessionId, cancellationToken);
            if (session is null || approval.Decide(approved, clock.UtcNow).IsFailure)
            {
                return false;
            }

            if (session.Status.Equals(AgentSessionStatusType.WaitingForApproval)
                && session.ResumeAfterApproval(clock.UtcNow).IsFailure)
            {
                return false;
            }

            await AddEventAsync(scope, session.Id, AgentSessionEventType.Status,
                JsonSerializer.Serialize(new
                {
                    status = session.Status.Name,
                    approvalId = approval.Id.Value,
                    approved
                }), cancellationToken);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(cancellationToken);
            return true;
        }

        public Task<bool> CancelAsync(AgentSessionId sessionId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sessionId);
            cancellationToken.ThrowIfCancellationRequested();
            if (!_activeSessions.TryGetValue(sessionId.Value, out CancellationTokenSource? source))
            {
                return Task.FromResult(false);
            }

            source.Cancel();
            return Task.FromResult(true);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ParallelOptions options = new()
            {
                CancellationToken = stoppingToken,
                MaxDegreeOfParallelism = 4
            };
            await Parallel.ForEachAsync(_queue.Reader.ReadAllAsync(stoppingToken), options,
                async (execution, cancellationToken) => await ExecuteSessionAsync(execution, cancellationToken));
        }

        private async Task ExecuteSessionAsync(AgentSessionExecution execution,
            CancellationToken stoppingToken)
        {
            if (!_processClients.TryGetValue(execution.VendorId, out IAgentProcessClient? client))
            {
                await FinishAsync(execution.SessionId, false, false, stoppingToken);
                return;
            }

            using CancellationTokenSource sessionCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            if (!_activeSessions.TryAdd(execution.SessionId, sessionCancellation))
            {
                return;
            }

            try
            {
                await StartAsync(execution.SessionId, sessionCancellation.Token);
                await client.RunAsync(new AgentProcessRequest(execution.SessionId, execution.ExecutablePath,
                        execution.WorktreePath, execution.Prompt),
                    (processEvent, cancellationToken) =>
                        AppendEventAsync(execution.SessionId, processEvent.Type, processEvent.PayloadJson,
                            cancellationToken),
                    (approval, cancellationToken) =>
                        RequestApprovalAsync(execution.SessionId, approval, cancellationToken),
                    sessionCancellation.Token);
                await FinishAsync(execution.SessionId, true, false, sessionCancellation.Token);
            }
            catch (OperationCanceledException) when (sessionCancellation.IsCancellationRequested)
            {
                await FinishAsync(execution.SessionId, false, true, CancellationToken.None);
            }
            catch
            {
                await FinishAsync(execution.SessionId, false, false, CancellationToken.None);
            }
            finally
            {
                _activeSessions.TryRemove(execution.SessionId, out _);
            }
        }

        private async Task StartAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            IAgentSessionRepository sessions = scope.ServiceProvider.GetRequiredService<IAgentSessionRepository>();
            AgentSession? session = await sessions.GetByIdAsync(AgentSessionId.Create(sessionId), cancellationToken);
            if (session is null)
            {
                throw new InvalidOperationException($"Agent session '{sessionId}' was not found.");
            }

            IClockService clock = scope.ServiceProvider.GetRequiredService<IClockService>();
            DomainResult transition = session.Start(clock.UtcNow);
            if (transition.IsFailure)
            {
                throw new InvalidOperationException(transition.Error.Description);
            }

            await AddEventAsync(scope, session.Id, AgentSessionEventType.Status,
                JsonSerializer.Serialize(new { status = session.Status.Name }), cancellationToken);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(cancellationToken);
        }

        private async Task<bool> RequestApprovalAsync(Guid sessionIdValue, AgentProcessApprovalRequest request,
            CancellationToken cancellationToken)
        {
            AgentApprovalId approvalId = AgentApprovalId.New();
            PendingAgentApproval pending = new(new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously));
            if (!_pendingApprovals.TryAdd(approvalId.Value, pending))
            {
                throw new InvalidOperationException("Could not register the agent approval.");
            }

            try
            {
                using (IServiceScope scope = scopeFactory.CreateScope())
                {
                    IAgentSessionRepository sessions =
                        scope.ServiceProvider.GetRequiredService<IAgentSessionRepository>();
                    AgentSession? session = await sessions.GetByIdAsync(AgentSessionId.Create(sessionIdValue),
                        cancellationToken);
                    if (session is null)
                    {
                        throw new InvalidOperationException($"Agent session '{sessionIdValue}' was not found.");
                    }

                    IClockService clock = scope.ServiceProvider.GetRequiredService<IClockService>();
                    DomainResult transition = session.WaitForApproval(clock.UtcNow);
                    if (transition.IsFailure)
                    {
                        throw new InvalidOperationException(transition.Error.Description);
                    }

                    AgentSessionEvent requestEvent = await AddEventAsync(scope, session.Id,
                        AgentSessionEventType.ApprovalRequest,
                        JsonSerializer.Serialize(new
                        {
                            approvalId = approvalId.Value,
                            request.ToolName,
                            request.ArgumentsJson
                        }), cancellationToken);
                    AgentApproval approval = AgentApproval.Create(approvalId, session.Id, requestEvent.Id,
                        request.ToolName, request.ArgumentsJson, clock.UtcNow).Value;
                    await scope.ServiceProvider.GetRequiredService<IAgentApprovalRepository>()
                        .AddAsync(approval, cancellationToken);
                    await scope.ServiceProvider.GetRequiredService<IUnitOfWork>()
                        .SaveChangesAsync(cancellationToken);
                }

                bool approved = await pending.Completion.Task.WaitAsync(cancellationToken);
                using IServiceScope decisionScope = scopeFactory.CreateScope();
                IClockService decisionClock = decisionScope.ServiceProvider.GetRequiredService<IClockService>();
                AgentApproval? storedApproval = await decisionScope.ServiceProvider
                    .GetRequiredService<IAgentApprovalRepository>()
                    .GetByIdAsync(approvalId, cancellationToken);
                AgentSession? storedSession = await decisionScope.ServiceProvider
                    .GetRequiredService<IAgentSessionRepository>()
                    .GetByIdAsync(AgentSessionId.Create(sessionIdValue), cancellationToken);
                if (storedApproval is null || storedSession is null)
                {
                    throw new InvalidOperationException("The pending approval could not be loaded.");
                }

                DomainResult approvalResult = storedApproval.Decide(approved, decisionClock.UtcNow);
                DomainResult sessionResult = storedSession.ResumeAfterApproval(decisionClock.UtcNow);
                if (approvalResult.IsFailure || sessionResult.IsFailure)
                {
                    throw new InvalidOperationException("The pending approval could not be completed.");
                }

                await AddEventAsync(decisionScope, storedSession.Id, AgentSessionEventType.Status,
                    JsonSerializer.Serialize(new
                    {
                        status = storedSession.Status.Name,
                        approvalId = approvalId.Value,
                        approved
                    }), cancellationToken);
                await decisionScope.ServiceProvider.GetRequiredService<IUnitOfWork>()
                    .SaveChangesAsync(cancellationToken);
                return approved;
            }
            finally
            {
                _pendingApprovals.TryRemove(approvalId.Value, out _);
            }
        }

        private async Task AppendEventAsync(Guid sessionId, AgentSessionEventType type, string payloadJson,
            CancellationToken cancellationToken)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            await AddEventAsync(scope, AgentSessionId.Create(sessionId), type, payloadJson, cancellationToken);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(cancellationToken);
        }

        private static async Task<AgentSessionEvent> AddEventAsync(IServiceScope scope, AgentSessionId sessionId,
            AgentSessionEventType type, string payloadJson, CancellationToken cancellationToken)
        {
            IAgentSessionEventRepository events =
                scope.ServiceProvider.GetRequiredService<IAgentSessionEventRepository>();
            long sequence = await events.GetNextSequenceAsync(sessionId, cancellationToken);
            IClockService clock = scope.ServiceProvider.GetRequiredService<IClockService>();
            AgentSessionEvent sessionEvent = AgentSessionEvent.Create(AgentSessionEventId.New(), sessionId, sequence,
                type, payloadJson, clock.UtcNow).Value;
            await events.AddAsync(sessionEvent, cancellationToken);
            return sessionEvent;
        }

        private async Task FinishAsync(Guid sessionIdValue, bool completed, bool cancelled,
            CancellationToken cancellationToken)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            IAgentSessionRepository sessions = scope.ServiceProvider.GetRequiredService<IAgentSessionRepository>();
            AgentSession? session = await sessions.GetByIdAsync(AgentSessionId.Create(sessionIdValue),
                cancellationToken);
            if (session is null || session.FinishedAtUtc is not null)
            {
                return;
            }

            IClockService clock = scope.ServiceProvider.GetRequiredService<IClockService>();
            DomainResult transition = cancelled
                ? session.Cancel(clock.UtcNow)
                : completed
                    ? session.Complete(clock.UtcNow)
                    : session.Fail(clock.UtcNow);
            if (transition.IsFailure)
            {
                return;
            }

            await AddEventAsync(scope, session.Id, AgentSessionEventType.Status,
                JsonSerializer.Serialize(new { status = session.Status.Name }), cancellationToken);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(cancellationToken);
        }
    }
}
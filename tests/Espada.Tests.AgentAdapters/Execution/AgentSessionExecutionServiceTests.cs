using Espada.AgentAdapters.Execution;
using Espada.AgentAdapters.Models;
using Espada.AgentAdapters.Processes;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.Models.Agents;
using Espada.Domain.Aggregates;
using Espada.Domain.Entities;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Espada.Tests.AgentAdapters.Execution
{
    public sealed class AgentSessionExecutionServiceTests
    {
        [Fact]
        public async Task ExecuteSessions_ShouldKeepParallelApprovalAndCancellationTimelinesIndependent()
        {
            AgentSession[] sessions =
            [
                CreateSession("approve"),
                CreateSession("deny"),
                CreateSession("cancel")
            ];
            AgentSessionRepository sessionRepository = new(sessions);
            AgentSessionEventRepository eventRepository = new();
            AgentApprovalRepository approvalRepository = new();
            FakeAgentProcessClient processClient = new(sessions.Length);
            IServiceScopeFactory scopeFactory = new ServiceScopeFactory(new Dictionary<Type, object>
            {
                [typeof(IClockService)] = new IncrementingClock(),
                [typeof(IAgentSessionRepository)] = sessionRepository,
                [typeof(IAgentSessionEventRepository)] = eventRepository,
                [typeof(IAgentApprovalRepository)] = approvalRepository,
                [typeof(IUnitOfWork)] = new UnitOfWork()
            });
            AgentSessionExecutionService service = new(scopeFactory, [processClient]);

            await service.StartAsync(TestContext.Current.CancellationToken);
            try
            {
                foreach (AgentSession session in sessions)
                {
                    await service.QueueAsync(new AgentSessionExecution(session.Id.Value, AgentVendorType.Codex.Id,
                        "fake-agent", session.WorktreePath, session.Prompt), TestContext.Current.CancellationToken);
                }

                await WaitUntilAsync(() => approvalRepository.Count == 2
                                               && sessions.Single(item => item.Prompt == "cancel").Status
                                                   .Equals(AgentSessionStatusType.Running));

                AgentSession cancelled = sessions.Single(item => item.Prompt == "cancel");
                Assert.True(await service.CancelAsync(cancelled.Id, TestContext.Current.CancellationToken));
                AgentApproval approved = approvalRepository.Items.Single(item =>
                    sessionRepository.Get(item.AgentSessionId).Prompt == "approve");
                AgentApproval denied = approvalRepository.Items.Single(item =>
                    sessionRepository.Get(item.AgentSessionId).Prompt == "deny");
                Assert.True(await service.DecideApprovalAsync(approved.Id, true,
                    TestContext.Current.CancellationToken));
                Assert.True(await service.DecideApprovalAsync(denied.Id, false,
                    TestContext.Current.CancellationToken));

                await WaitUntilAsync(() => sessions.All(item => item.FinishedAtUtc is not null));

                Assert.Equal(AgentSessionStatusType.Completed,
                    sessions.Single(item => item.Prompt == "approve").Status);
                Assert.Equal(AgentSessionStatusType.Completed,
                    sessions.Single(item => item.Prompt == "deny").Status);
                Assert.Equal(AgentSessionStatusType.Cancelled, cancelled.Status);
                Assert.True(processClient.ApprovalResults[approved.AgentSessionId.Value]);
                Assert.False(processClient.ApprovalResults[denied.AgentSessionId.Value]);
                Assert.All(sessions, session =>
                {
                    AgentSessionEvent[] timeline = eventRepository.List(session.Id);
                    Assert.NotEmpty(timeline);
                    Assert.All(timeline, item => Assert.Equal(session.Id, item.AgentSessionId));
                    Assert.Equal(Enumerable.Range(1, timeline.Length).Select(value => (long)value),
                        timeline.Select(item => item.Sequence));
                });
            }
            finally
            {
                await service.StopAsync(CancellationToken.None);
                service.Dispose();
            }
        }

        private static AgentSession CreateSession(string prompt)
        {
            return AgentSession.Create(AgentSessionId.New(), WorkspaceId.New(), ProjectId.New(),
                AgentProfileId.New(), DeviceId.New(), prompt, $"espada/codex/{prompt}",
                Path.Join("worktrees", prompt), DateTimeOffset.UtcNow).Value;
        }

        private static async Task WaitUntilAsync(Func<bool> condition)
        {
            using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
            while (!condition())
            {
                await Task.Delay(10, timeout.Token);
            }
        }

        private sealed class FakeAgentProcessClient(int sessionCount) : IAgentProcessClient
        {
            private readonly TaskCompletionSource _allStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private int _started;

            public int VendorId => AgentVendorType.Codex.Id;

            public ConcurrentDictionary<Guid, bool> ApprovalResults { get; } = new();

            public async Task RunAsync(AgentProcessRequest request,
                Func<AgentProcessEvent, CancellationToken, Task> onEvent,
                Func<AgentProcessApprovalRequest, CancellationToken, Task<bool>> onApproval,
                CancellationToken cancellationToken = default)
            {
                if (Interlocked.Increment(ref _started) == sessionCount)
                {
                    _allStarted.TrySetResult();
                }

                await _allStarted.Task.WaitAsync(cancellationToken);
                await onEvent(new AgentProcessEvent(AgentSessionEventType.AssistantOutput,
                    $$"""{"sessionId":"{{request.SessionId}}"}"""), cancellationToken);
                if (request.Prompt == "cancel")
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                    return;
                }

                bool approved = await onApproval(new AgentProcessApprovalRequest("git.apply", "{}"),
                    cancellationToken);
                ApprovalResults[request.SessionId] = approved;
                await onEvent(new AgentProcessEvent(AgentSessionEventType.ToolResult,
                    $$"""{"approved":{{approved.ToString().ToLowerInvariant()}}}"""), cancellationToken);
            }
        }

        private sealed class AgentSessionRepository(IEnumerable<AgentSession> sessions) : IAgentSessionRepository
        {
            private readonly ConcurrentDictionary<Guid, AgentSession> _items =
                new(sessions.ToDictionary(item => item.Id.Value));

            public Task AddAsync(AgentSession session, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _items[session.Id.Value] = session;
                return Task.CompletedTask;
            }

            public Task<AgentSession?> GetByIdAsync(AgentSessionId sessionId,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _items.TryGetValue(sessionId.Value, out AgentSession? session);
                return Task.FromResult(session);
            }

            public Task<IReadOnlyList<AgentSession>> ListByWorkspaceIdAsync(WorkspaceId workspaceId,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult<IReadOnlyList<AgentSession>>(
                    _items.Values.Where(item => item.WorkspaceId == workspaceId).ToArray());
            }

            public AgentSession Get(AgentSessionId sessionId)
            {
                return _items[sessionId.Value];
            }
        }

        private sealed class AgentSessionEventRepository : IAgentSessionEventRepository
        {
            private readonly object _lock = new();
            private readonly List<AgentSessionEvent> _items = [];

            public Task AddAsync(AgentSessionEvent sessionEvent, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                lock (_lock)
                {
                    _items.Add(sessionEvent);
                }

                return Task.CompletedTask;
            }

            public Task<long> GetNextSequenceAsync(AgentSessionId sessionId,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                lock (_lock)
                {
                    return Task.FromResult(_items.LongCount(item => item.AgentSessionId == sessionId) + 1);
                }
            }

            public Task<IReadOnlyList<AgentSessionEvent>> ListBySessionIdAsync(AgentSessionId sessionId,
                long afterSequence = 0, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult<IReadOnlyList<AgentSessionEvent>>(List(sessionId)
                    .Where(item => item.Sequence > afterSequence).ToArray());
            }

            public AgentSessionEvent[] List(AgentSessionId sessionId)
            {
                lock (_lock)
                {
                    return _items.Where(item => item.AgentSessionId == sessionId)
                        .OrderBy(item => item.Sequence).ToArray();
                }
            }
        }

        private sealed class AgentApprovalRepository : IAgentApprovalRepository
        {
            private readonly ConcurrentDictionary<Guid, AgentApproval> _items = new();

            public int Count => _items.Count;

            public IReadOnlyCollection<AgentApproval> Items => _items.Values.ToArray();

            public Task AddAsync(AgentApproval approval, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _items[approval.Id.Value] = approval;
                return Task.CompletedTask;
            }

            public Task<AgentApproval?> GetByIdAsync(AgentApprovalId approvalId,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _items.TryGetValue(approvalId.Value, out AgentApproval? approval);
                return Task.FromResult(approval);
            }

            public Task<IReadOnlyList<AgentApproval>> ListBySessionIdAsync(AgentSessionId sessionId,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult<IReadOnlyList<AgentApproval>>(
                    _items.Values.Where(item => item.AgentSessionId == sessionId).ToArray());
            }
        }

        private sealed class ServiceScopeFactory(IReadOnlyDictionary<Type, object> services) : IServiceScopeFactory
        {
            public IServiceScope CreateScope()
            {
                return new ServiceScope(services);
            }
        }

        private sealed class ServiceScope(IReadOnlyDictionary<Type, object> services) : IServiceScope
        {
            public IServiceProvider ServiceProvider { get; } = new DictionaryServiceProvider(services);

            public void Dispose()
            {
            }
        }

        private sealed class DictionaryServiceProvider(IReadOnlyDictionary<Type, object> services)
            : IServiceProvider
        {
            public object? GetService(Type serviceType)
            {
                return serviceType == typeof(IServiceProvider)
                    ? this
                    : services.GetValueOrDefault(serviceType);
            }
        }
        private sealed class IncrementingClock : IClockService
        {
            private long _ticks = DateTimeOffset.UtcNow.Ticks;

            public DateTimeOffset UtcNow => new(Interlocked.Increment(ref _ticks), TimeSpan.Zero);
        }

        private sealed class UnitOfWork : IUnitOfWork
        {
            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(0);
            }
        }
    }
}
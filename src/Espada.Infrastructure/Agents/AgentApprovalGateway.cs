using Espada.Application.Contracts.Agents;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Entities;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Espada.Infrastructure.Agents
{
    internal sealed class AgentApprovalGateway(IServiceScopeFactory scopeFactory) : IAgentApprovalGateway
    {
        public async Task<bool> RequestAsync(AgentSessionId sessionId, string toolName, string argumentsJson,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sessionId);
            AgentApprovalId approvalId = AgentApprovalId.New();
            await using (AsyncServiceScope scope = scopeFactory.CreateAsyncScope())
            {
                IAgentSessionRepository sessions =
                    scope.ServiceProvider.GetRequiredService<IAgentSessionRepository>();
                AgentSession session = await sessions.GetByIdAsync(sessionId, cancellationToken)
                                       ?? throw new InvalidOperationException($"Agent session '{sessionId}' was not found.");
                IClockService clock = scope.ServiceProvider.GetRequiredService<IClockService>();
                DomainResult transition = session.WaitForApproval(clock.UtcNow);
                if (transition.IsFailure)
                {
                    throw new InvalidOperationException(transition.Error.Description);
                }

                IAgentSessionEventRepository events =
                    scope.ServiceProvider.GetRequiredService<IAgentSessionEventRepository>();
                long sequence = await events.GetNextSequenceAsync(sessionId, cancellationToken);
                AgentSessionEvent requestEvent = AgentSessionEvent.Create(AgentSessionEventId.New(), sessionId,
                    sequence, AgentSessionEventType.ApprovalRequest,
                    JsonSerializer.Serialize(new { approvalId = approvalId.Value, toolName, argumentsJson }),
                    clock.UtcNow).Value;
                await events.AddAsync(requestEvent, cancellationToken);
                AgentApproval approval = AgentApproval.Create(approvalId, sessionId, requestEvent.Id, toolName,
                    argumentsJson, clock.UtcNow).Value;
                await scope.ServiceProvider.GetRequiredService<IAgentApprovalRepository>()
                    .AddAsync(approval, cancellationToken);
                await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(cancellationToken);
            }

            while (true)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
                await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
                AgentApproval? approval = await scope.ServiceProvider.GetRequiredService<IAgentApprovalRepository>()
                    .GetByIdAsync(approvalId, cancellationToken);
                if (approval is null)
                {
                    throw new InvalidOperationException("The approval request was removed before a decision.");
                }

                if (!approval.Status.Equals(AgentApprovalStatusType.Pending))
                {
                    return approval.Status.Equals(AgentApprovalStatusType.Approved);
                }
            }
        }
    }
}
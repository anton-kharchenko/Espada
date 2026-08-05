using Espada.AgentAdapters.Models;

namespace Espada.AgentAdapters.Processes
{
    public interface IAgentProcessClient
    {
        int VendorId { get; }

        Task RunAsync(AgentProcessRequest request,
            Func<AgentProcessEvent, CancellationToken, Task> onEvent,
            Func<AgentProcessApprovalRequest, CancellationToken, Task<bool>> onApproval,
            CancellationToken cancellationToken = default);
    }
}
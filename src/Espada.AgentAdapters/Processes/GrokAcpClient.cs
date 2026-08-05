using Espada.AgentAdapters.Models;
using Espada.Domain.Enums;

namespace Espada.AgentAdapters.Processes
{
    public sealed class GrokAcpClient : IAgentProcessClient
    {
        private static readonly AcpClient Client = new(AgentVendorType.Grok.Id, ["agent", "stdio"]);

        public int VendorId => AgentVendorType.Grok.Id;

        public Task RunAsync(AgentProcessRequest request,
            Func<AgentProcessEvent, CancellationToken, Task> onEvent,
            Func<AgentProcessApprovalRequest, CancellationToken, Task<bool>> onApproval,
            CancellationToken cancellationToken = default)
        {
            return Client.RunAsync(request, onEvent, onApproval, cancellationToken);
        }
    }
}
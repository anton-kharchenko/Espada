using Espada.AgentAdapters.Models;
using Espada.Domain.Enums;

namespace Espada.AgentAdapters.Processes
{
    public sealed class GeminiAcpClient : IAgentProcessClient
    {
        private static readonly AcpClient Client = new(AgentVendorType.Gemini.Id, ["--acp"]);

        public int VendorId => AgentVendorType.Gemini.Id;

        public Task RunAsync(AgentProcessRequest request,
            Func<AgentProcessEvent, CancellationToken, Task> onEvent,
            Func<AgentProcessApprovalRequest, CancellationToken, Task<bool>> onApproval,
            CancellationToken cancellationToken = default)
        {
            return Client.RunAsync(request, onEvent, onApproval, cancellationToken);
        }
    }
}
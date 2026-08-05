using Espada.Domain.Enums;

namespace Espada.Application.Models.Agents
{
    public sealed record AgentSessionExecution(Guid SessionId, int VendorId, string ExecutablePath,
        string WorktreePath, string Prompt)
    {
        public AgentVendorType Vendor => Domain.SeedWork.Enumeration.FromId<AgentVendorType>(VendorId);
    }
}
namespace Espada.Api.Contracts.Responses.AgentSessions
{
    public sealed record AgentOptionResponse(int VendorId, string Vendor, Guid? AgentProfileId, bool IsInstalled,
        bool IsAuthenticated);
}
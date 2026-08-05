namespace Espada.LocalSetup.Contracts.Responses
{
    public sealed record LocalSetupAgentPreview(
        int VendorId,
        string Vendor,
        bool IsInstalled,
        bool IsAuthenticated,
        string? ExecutablePath,
        string? Version);
}
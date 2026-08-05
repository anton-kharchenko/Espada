namespace Espada.Api.LocalSetup.Models
{
    internal sealed record LocalSetupAgentPreview(
        int VendorId,
        string Vendor,
        bool IsInstalled,
        bool IsAuthenticated,
        string? ExecutablePath,
        string? Version);
}

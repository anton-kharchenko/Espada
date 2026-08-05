namespace Espada.Application.UseCases.LocalSetup.Commands.CommitLocalSetup
{
    public sealed record LocalSetupAgentInput(
        int VendorId,
        string ExecutablePath,
        string? Version,
        bool IsAuthenticated);
}

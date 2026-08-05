using Espada.LocalSetup.Contracts.Responses;

namespace Espada.LocalSetup.Models
{
    internal sealed record GitRepositorySnapshot(
        string Root,
        string? CanonicalRemoteUri,
        IReadOnlyList<LocalSetupInstructionPreview> Instructions);
}
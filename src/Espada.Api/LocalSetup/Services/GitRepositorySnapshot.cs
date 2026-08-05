using Espada.Api.LocalSetup.Models;

namespace Espada.Api.LocalSetup.Services
{
    internal sealed record GitRepositorySnapshot(
        string Root,
        string? CanonicalRemoteUri,
        IReadOnlyList<LocalSetupInstructionPreview> Instructions);
}

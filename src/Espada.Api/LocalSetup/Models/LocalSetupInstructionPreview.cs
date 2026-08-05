namespace Espada.Api.LocalSetup.Models
{
    internal sealed record LocalSetupInstructionPreview(
        string RelativePath,
        string Agent,
        string ContentHash,
        string Content);
}

namespace Espada.LocalSetup.Contracts.Responses
{
    public sealed record LocalSetupInstructionPreview(
        string RelativePath,
        string Agent,
        string ContentHash,
        string Content);
}
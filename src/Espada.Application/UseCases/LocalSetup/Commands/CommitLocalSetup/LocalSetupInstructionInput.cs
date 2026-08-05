namespace Espada.Application.UseCases.LocalSetup.Commands.CommitLocalSetup
{
    public sealed record LocalSetupInstructionInput(string RelativePath, string Content, string? Agent);
}

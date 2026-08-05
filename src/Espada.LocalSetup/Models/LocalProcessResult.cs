namespace Espada.LocalSetup.Models
{
    internal sealed record LocalProcessResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public bool Succeeded => ExitCode == 0;
    }
}
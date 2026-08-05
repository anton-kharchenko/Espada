namespace Espada.Daemon.Runtime
{
    public sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public bool Succeeded => ExitCode == 0;
    }
}

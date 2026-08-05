namespace Espada.Daemon.Runtime
{
    public sealed record ProcessCommand(
        string Executable,
        IReadOnlyList<string> Arguments,
        string? WorkingDirectory = null,
        IReadOnlyDictionary<string, string?>? Environment = null);
}

using System.Diagnostics;

namespace Espada.Api.LocalSetup.Services
{
    internal static class LocalProcess
    {
        public static async Task<LocalProcessResult> RunAsync(string executable, IReadOnlyList<string> arguments,
            string? workingDirectory, TimeSpan timeout, CancellationToken cancellationToken)
        {
            ProcessStartInfo startInfo = new(executable)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory
            };
            foreach (string argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            using Process process = Process.Start(startInfo)
                ?? throw new InvalidOperationException($"{executable} could not be started.");
            Task<string> output = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> error = process.StandardError.ReadToEndAsync(cancellationToken);
            using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);
            timeoutSource.CancelAfter(timeout);
            try
            {
                await process.WaitForExitAsync(timeoutSource.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                process.Kill(true);
                await process.WaitForExitAsync(CancellationToken.None);
                return new LocalProcessResult(-1, await output, "Process timed out.");
            }

            return new LocalProcessResult(process.ExitCode, await output, await error);
        }
    }
}

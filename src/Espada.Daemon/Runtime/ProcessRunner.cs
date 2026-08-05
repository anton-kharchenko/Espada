using System.Diagnostics;

namespace Espada.Daemon.Runtime
{
    public sealed class ProcessRunner
    {
        public async Task<ProcessResult> RunAsync(ProcessCommand command, CancellationToken cancellationToken)
        {
            using Process process = new();
            process.StartInfo = CreateStartInfo(command);
            if (!process.Start())
            {
                throw new InvalidOperationException("Process could not be started.");
            }

            Task<string> output = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> error = process.StandardError.ReadToEndAsync(cancellationToken);
            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                }

                throw;
            }

            return new ProcessResult(process.ExitCode, await output, await error);
        }

        public ManagedChildProcess StartLongRunning(ProcessCommand command, string logDirectory, string logName)
        {
            Process process = new() { StartInfo = CreateStartInfo(command), EnableRaisingEvents = true };
            StreamWriter log = RotatingLogFile.Open(logDirectory, logName);
            TextWriter synchronizedLog = TextWriter.Synchronized(log);
            try
            {
                if (!process.Start())
                {
                    throw new InvalidOperationException("Process could not be started.");
                }

                Task output = PumpAsync(process.StandardOutput, synchronizedLog, "stdout");
                Task error = PumpAsync(process.StandardError, synchronizedLog, "stderr");
                return new ManagedChildProcess(process, output, error, log);
            }
            catch
            {
                log.Dispose();
                process.Dispose();
                throw;
            }
        }

        private static ProcessStartInfo CreateStartInfo(ProcessCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrWhiteSpace(command.Executable);
            ProcessStartInfo startInfo = new()
            {
                FileName = command.Executable,
                WorkingDirectory = command.WorkingDirectory ?? AppContext.BaseDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            foreach (string argument in command.Arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            if (command.Environment is not null)
            {
                foreach ((string key, string? value) in command.Environment)
                {
                    startInfo.Environment[key] = value;
                }
            }

            return startInfo;
        }

        private static async Task PumpAsync(StreamReader reader, TextWriter writer, string streamName)
        {
            while (await reader.ReadLineAsync() is { } line)
            {
                await writer.WriteLineAsync($"{DateTimeOffset.UtcNow:O} [{streamName}] {line}");
            }
        }
    }
}

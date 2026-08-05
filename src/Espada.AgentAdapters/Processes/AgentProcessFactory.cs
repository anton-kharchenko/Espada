using System.Diagnostics;

namespace Espada.AgentAdapters.Processes
{
    internal static class AgentProcessFactory
    {
        public static Process Start(string executablePath, string workingDirectory, IReadOnlyList<string> arguments,
            IReadOnlyDictionary<string, string>? environment = null)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = executablePath,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            foreach (string argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            if (environment is not null)
            {
                foreach ((string name, string value) in environment)
                {
                    startInfo.Environment[name] = value;
                }
            }

            Process process = new() { StartInfo = startInfo };
            process.Start();
            return process;
        }
    }
}
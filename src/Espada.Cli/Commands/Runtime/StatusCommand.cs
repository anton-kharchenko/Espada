using Espada.Cli.Constants;
using Espada.Cli.Infrastructure;
using System.CommandLine;

namespace Espada.Cli.Commands.Runtime
{
    internal static class StatusCommand
    {
        public static Command Create(Option<bool> jsonOption)
        {
            Command command = new("status", "Show local Espada runtime status.");
            command.SetAction(async (parseResult, cancellationToken) =>
            {
                LocalRuntimeClient runtime = new();
                bool healthy = await runtime.IsHealthyAsync(cancellationToken);
                if (parseResult.GetValue(jsonOption))
                {
                    CliJson.Write(new { status = healthy ? "healthy" : "stopped", state = runtime.ReadState() });
                }
                else
                {
                    Console.WriteLine(healthy ? "Espada is running." : "Espada is stopped.");
                }

                return healthy ? CliExitCodesConstants.Success : CliExitCodesConstants.DaemonUnavailable;
            });
            return command;
        }
    }
}
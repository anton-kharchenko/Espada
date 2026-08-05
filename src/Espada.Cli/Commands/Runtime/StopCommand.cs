using Espada.Cli.Constants;
using Espada.Cli.Infrastructure;
using System.CommandLine;

namespace Espada.Cli.Commands.Runtime
{
    internal static class StopCommand
    {
        public static Command Create(Option<bool> jsonOption)
        {
            Command command = new("stop", "Stop the local Espada runtime.");
            command.SetAction(async (parseResult, cancellationToken) =>
            {
                LocalRuntimeClient runtime = new();
                if (!await runtime.IsHealthyAsync(cancellationToken))
                {
                    if (parseResult.GetValue(jsonOption))
                    {
                        CliJson.Write(new { status = "stopped" });
                    }
                    else
                    {
                        Console.WriteLine("Espada is not running.");
                    }

                    return CliExitCodesConstants.Success;
                }

                try
                {
                    await runtime.StopAsync(cancellationToken);
                    if (parseResult.GetValue(jsonOption))
                    {
                        CliJson.Write(new { status = "stopping" });
                    }
                    else
                    {
                        Console.WriteLine("Espada is stopping.");
                    }

                    return CliExitCodesConstants.Success;
                }
                catch (Exception exception)
                {
                    await Console.Error.WriteLineAsync(exception.Message);
                    return CliExitCodesConstants.DaemonUnavailable;
                }
            });
            return command;
        }
    }
}
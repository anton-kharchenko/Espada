using Espada.Cli.Constants;
using Espada.Cli.Infrastructure;
using System.CommandLine;

namespace Espada.Cli.Commands.Runtime
{
    internal static class StartCommand
    {
        public static Command Create(Option<bool> jsonOption)
        {
            Command command = new("start", "Start the local Espada runtime.");
            command.SetAction(async (parseResult, cancellationToken) =>
            {
                try
                {
                    LocalRuntimeClient runtime = new();
                    await runtime.StartAsync(cancellationToken);
                    if (parseResult.GetValue(jsonOption))
                    {
                        CliJson.Write(new { status = "healthy", state = runtime.ReadState() });
                    }
                    else
                    {
                        Console.WriteLine("Espada is running.");
                    }

                    return CliExitCodesConstants.Success;
                }
                catch (Exception exception)
                {
                    await Console.Error.WriteLineAsync(exception.Message);
                    return CliExitCodesConstants.Failure;
                }
            });
            return command;
        }
    }
}
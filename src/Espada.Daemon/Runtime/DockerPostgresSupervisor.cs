using System.Globalization;

namespace Espada.Daemon.Runtime
{
    public sealed class DockerPostgresSupervisor
    {
        private readonly LocalRuntimeOptions _options;
        private readonly ProcessRunner _processRunner;

        public DockerPostgresSupervisor(LocalRuntimeOptions options, ProcessRunner processRunner)
        {
            _options = options;
            _processRunner = processRunner;
        }

        public async Task EnsureDockerAvailableAsync(CancellationToken cancellationToken)
        {
            ProcessResult result;
            try
            {
                result = await RunDockerAsync(["version", "--format", "{{.Server.Version}}"], cancellationToken);
            }
            catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or FileNotFoundException)
            {
                throw new InvalidOperationException("Docker Engine is required but the docker CLI was not found.",
                    exception);
            }

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Docker Engine is required and must be running before Espada can start.");
            }
        }

        public async Task StartAsync(LocalRuntimePaths paths, CancellationToken cancellationToken)
        {
            bool exists = await ContainerExistsAsync(cancellationToken);
            if (exists)
            {
                await ValidateExistingContainerAsync(cancellationToken);
                if (!await ContainerIsRunningAsync(cancellationToken))
                {
                    LoopbackPort.EnsureAvailable(_options.PostgresPort);
                    await RequireSuccessAsync(["start", _options.PostgresContainerName], cancellationToken,
                        "PostgreSQL container could not be started.");
                }
            }
            else
            {
                LoopbackPort.EnsureAvailable(_options.PostgresPort);
                string mount = $"type=bind,source={paths.PasswordFile},target=/run/secrets/postgres-password,readonly";
                await RequireSuccessAsync(
                    [
                        "run",
                        "--detach",
                        "--name", _options.PostgresContainerName,
                        "--restart", "no",
                        "--publish", $"127.0.0.1:{_options.PostgresPort}:5432",
                        "--volume", $"{_options.PostgresVolumeName}:/var/lib/postgresql/data",
                        "--mount", mount,
                        "--env", "POSTGRES_USER=espada",
                        "--env", "POSTGRES_DB=Espada",
                        "--env", "POSTGRES_PASSWORD_FILE=/run/secrets/postgres-password",
                        _options.PostgresImage
                    ],
                    cancellationToken,
                    "PostgreSQL container could not be created.");
            }

            await WaitUntilReadyAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!await ContainerExistsAsync(cancellationToken) ||
                !await ContainerIsRunningAsync(cancellationToken))
            {
                return;
            }

            await RequireSuccessAsync(
                ["stop", "--time", _options.ShutdownTimeoutSeconds.ToString(CultureInfo.InvariantCulture),
                    _options.PostgresContainerName],
                cancellationToken,
                "PostgreSQL container could not be stopped.");
        }

        private async Task ValidateExistingContainerAsync(CancellationToken cancellationToken)
        {
            ProcessResult image = await RunDockerAsync(
                ["container", "inspect", "--format", "{{.Config.Image}}", _options.PostgresContainerName],
                cancellationToken);
            if (!image.Succeeded || !string.Equals(image.StandardOutput.Trim(), _options.PostgresImage,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Container {_options.PostgresContainerName} exists with an unexpected image.");
            }

            ProcessResult port = await RunDockerAsync(
                ["port", _options.PostgresContainerName, "5432/tcp"], cancellationToken);
            if (!port.Succeeded || !port.StandardOutput.Trim().EndsWith(
                    $":{_options.PostgresPort}", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Container {_options.PostgresContainerName} is not mapped to configured port " +
                    $"{_options.PostgresPort}.");
            }
        }

        private async Task<bool> ContainerExistsAsync(CancellationToken cancellationToken)
        {
            ProcessResult result = await RunDockerAsync(
                ["container", "inspect", _options.PostgresContainerName], cancellationToken);
            return result.Succeeded;
        }

        private async Task<bool> ContainerIsRunningAsync(CancellationToken cancellationToken)
        {
            ProcessResult result = await RunDockerAsync(
                ["container", "inspect", "--format", "{{.State.Running}}", _options.PostgresContainerName],
                cancellationToken);
            return result.Succeeded && string.Equals(result.StandardOutput.Trim(), "true",
                StringComparison.OrdinalIgnoreCase);
        }

        private async Task WaitUntilReadyAsync(CancellationToken cancellationToken)
        {
            DateTimeOffset deadline = DateTimeOffset.UtcNow.AddSeconds(_options.StartupTimeoutSeconds);
            while (DateTimeOffset.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ProcessResult result = await RunDockerAsync(
                    ["exec", _options.PostgresContainerName, "pg_isready", "--username", "espada",
                        "--dbname", "Espada"],
                    cancellationToken);
                if (result.Succeeded)
                {
                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            }

            throw new TimeoutException("PostgreSQL container did not become ready before the startup timeout.");
        }

        private async Task RequireSuccessAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken,
            string message)
        {
            ProcessResult result = await RunDockerAsync(arguments, cancellationToken);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"{message} {Sanitize(result.StandardError)}");
            }
        }

        private Task<ProcessResult> RunDockerAsync(IReadOnlyList<string> arguments,
            CancellationToken cancellationToken)
        {
            return _processRunner.RunAsync(new ProcessCommand(_options.DockerExecutable, arguments),
                cancellationToken);
        }

        private static string Sanitize(string value)
        {
            string normalized = value.Replace("\r", " ", StringComparison.Ordinal)
                .Replace("\n", " ", StringComparison.Ordinal).Trim();
            return normalized.Length <= 500 ? normalized : normalized[..500];
        }
    }
}

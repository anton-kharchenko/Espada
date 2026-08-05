using Espada.AgentAdapters.Models;
using Espada.Domain.Enums;
using System.Diagnostics;
using System.Text.Json;

namespace Espada.AgentAdapters.Processes
{
    public sealed class ClaudeProcessClient : IAgentProcessClient
    {
        public int VendorId => AgentVendorType.Claude.Id;

        public async Task RunAsync(AgentProcessRequest request,
            Func<AgentProcessEvent, CancellationToken, Task> onEvent,
            Func<AgentProcessApprovalRequest, CancellationToken, Task<bool>> onApproval,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(onEvent);
            ArgumentNullException.ThrowIfNull(onApproval);
            using Process process = AgentProcessFactory.Start(request.ExecutablePath, request.WorkingDirectory,
                ["-p", request.Prompt, "--output-format", "stream-json", "--verbose",
                    "--permission-prompt-tool", "mcp__espada__session_request_approval"],
                new Dictionary<string, string>
                {
                    ["ESPADA_AGENT_SESSION_ID"] = request.SessionId.ToString("D")
                });
            process.StandardInput.Close();
            Task<string> standardError = process.StandardError.ReadToEndAsync(cancellationToken);
            while (await process.StandardOutput.ReadLineAsync(cancellationToken) is { } line)
            {
                if (!TryParse(line, out JsonDocument document))
                {
                    continue;
                }

                using (document)
                {
                    foreach (AgentProcessEvent processEvent in
                             AgentJsonEventNormalizer.NormalizeClaude(document.RootElement))
                    {
                        await onEvent(processEvent, cancellationToken);
                    }
                }
            }

            await process.WaitForExitAsync(cancellationToken);
            await standardError;
            if (process.ExitCode != 0)
            {
                await onEvent(new AgentProcessEvent(AgentSessionEventType.Error,
                    JsonSerializer.Serialize(new { message = $"Claude exited with code {process.ExitCode}." })),
                    cancellationToken);
                throw new InvalidOperationException($"Claude exited with code {process.ExitCode}.");
            }
        }

        private static bool TryParse(string line, out JsonDocument document)
        {
            try
            {
                document = JsonDocument.Parse(line);
                return true;
            }
            catch (JsonException)
            {
                document = null!;
                return false;
            }
        }
    }
}
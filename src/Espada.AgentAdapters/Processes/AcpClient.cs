using Espada.AgentAdapters.Models;
using Espada.Domain.Enums;
using System.Diagnostics;
using System.Text.Json;

namespace Espada.AgentAdapters.Processes
{
    internal sealed class AcpClient(int vendorId, IReadOnlyList<string> arguments)
    {
        public async Task RunAsync(AgentProcessRequest request,
            Func<AgentProcessEvent, CancellationToken, Task> onEvent,
            Func<AgentProcessApprovalRequest, CancellationToken, Task<bool>> onApproval,
            CancellationToken cancellationToken)
        {
            using Process process = AgentProcessFactory.Start(request.ExecutablePath, request.WorkingDirectory,
                arguments);
            Task<string> standardError = process.StandardError.ReadToEndAsync(cancellationToken);
            await WriteAsync(process, new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "initialize",
                @params = new
                {
                    protocolVersion = 1,
                    clientCapabilities = new { },
                    clientInfo = new { name = "espada", version = "1.0" }
                }
            }, cancellationToken);

            bool promptStarted = false;
            while (await process.StandardOutput.ReadLineAsync(cancellationToken) is { } line)
            {
                if (!TryParse(line, out JsonDocument document))
                {
                    continue;
                }

                using (document)
                {
                    JsonElement root = document.RootElement;
                    if (root.TryGetProperty("method", out JsonElement methodElement))
                    {
                        string method = methodElement.GetString() ?? string.Empty;
                        JsonElement parameters = root.TryGetProperty("params", out JsonElement value)
                            ? value
                            : default;
                        if (root.TryGetProperty("id", out JsonElement requestId)
                            && method.Equals("session/request_permission", StringComparison.Ordinal))
                        {
                            bool approved = await onApproval(new AgentProcessApprovalRequest("tool_call",
                                parameters.ValueKind == JsonValueKind.Undefined ? "{}" : parameters.GetRawText()),
                                cancellationToken);
                            string optionId = SelectPermissionOption(parameters, approved);
                            await WriteAsync(process, new
                            {
                                jsonrpc = "2.0",
                                id = requestId,
                                result = new { outcome = new { outcome = "selected", optionId } }
                            }, cancellationToken);
                            continue;
                        }

                        if (method.Equals("session/update", StringComparison.Ordinal)
                            && parameters.TryGetProperty("update", out JsonElement update))
                        {
                            foreach (AgentProcessEvent processEvent in AgentJsonEventNormalizer.NormalizeAcp(update))
                            {
                                await onEvent(processEvent, cancellationToken);
                            }
                        }

                        continue;
                    }

                    if (!root.TryGetProperty("id", out JsonElement responseId)
                        || !responseId.TryGetInt32(out int id))
                    {
                        continue;
                    }

                    if (root.TryGetProperty("error", out JsonElement error))
                    {
                        throw new InvalidOperationException($"ACP request {id} failed: {error.GetRawText()}");
                    }

                    if (!root.TryGetProperty("result", out JsonElement result))
                    {
                        continue;
                    }

                    if (id == 1)
                    {
                        await WriteAsync(process, new
                        {
                            jsonrpc = "2.0",
                            id = 2,
                            method = "session/new",
                            @params = new { cwd = request.WorkingDirectory, mcpServers = Array.Empty<object>() }
                        }, cancellationToken);
                    }
                    else if (id == 2 && result.TryGetProperty("sessionId", out JsonElement sessionId))
                    {
                        await WriteAsync(process, new
                        {
                            jsonrpc = "2.0",
                            id = 3,
                            method = "session/prompt",
                            @params = new
                            {
                                sessionId = sessionId.GetString(),
                                prompt = new[] { new { type = "text", text = request.Prompt } }
                            }
                        }, cancellationToken);
                        promptStarted = true;
                    }
                    else if (id == 3)
                    {
                        break;
                    }
                }
            }

            if (!process.HasExited)
            {
                process.StandardInput.Close();
            }

            await process.WaitForExitAsync(cancellationToken);
            await standardError;
            if (!promptStarted || process.ExitCode != 0)
            {
                string vendor = Domain.SeedWork.Enumeration.FromId<AgentVendorType>(vendorId).Name;
                await onEvent(new AgentProcessEvent(AgentSessionEventType.Error,
                    JsonSerializer.Serialize(new { message = $"{vendor} exited with code {process.ExitCode}." })),
                    cancellationToken);
                throw new InvalidOperationException($"{vendor} exited with code {process.ExitCode}.");
            }
        }

        private static string SelectPermissionOption(JsonElement parameters, bool approved)
        {
            if (parameters.TryGetProperty("options", out JsonElement options)
                && options.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement option in options.EnumerateArray())
                {
                    string kind = option.TryGetProperty("kind", out JsonElement kindElement)
                        ? kindElement.GetString() ?? string.Empty
                        : string.Empty;
                    bool matches = approved
                        ? kind.Contains("allow", StringComparison.OrdinalIgnoreCase)
                        : kind.Contains("reject", StringComparison.OrdinalIgnoreCase);
                    if (matches && option.TryGetProperty("optionId", out JsonElement optionId))
                    {
                        return optionId.GetString() ?? string.Empty;
                    }
                }
            }

            return approved ? "allow-once" : "reject-once";
        }

        private static async Task WriteAsync(Process process, object message, CancellationToken cancellationToken)
        {
            await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(message).AsMemory(), cancellationToken);
            await process.StandardInput.FlushAsync(cancellationToken);
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
using Espada.AgentAdapters.Models;
using Espada.Domain.Enums;
using System.Diagnostics;
using System.Text.Json;

namespace Espada.AgentAdapters.Processes
{
    public sealed class CodexAppServerClient : IAgentProcessClient
    {
        public int VendorId => AgentVendorType.Codex.Id;

        public async Task RunAsync(AgentProcessRequest request,
            Func<AgentProcessEvent, CancellationToken, Task> onEvent,
            Func<AgentProcessApprovalRequest, CancellationToken, Task<bool>> onApproval,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(onEvent);
            ArgumentNullException.ThrowIfNull(onApproval);
            using Process process = AgentProcessFactory.Start(request.ExecutablePath, request.WorkingDirectory,
                ["app-server"]);
            Task<string> standardError = process.StandardError.ReadToEndAsync(cancellationToken);
            await WriteAsync(process, new
            {
                id = 1,
                method = "initialize",
                @params = new
                {
                    clientInfo = new { name = "espada", version = "1.0" },
                    capabilities = new { }
                }
            }, cancellationToken);

            bool turnStarted = false;
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
                            && method.EndsWith("/requestApproval", StringComparison.Ordinal))
                        {
                            string toolName = method.Contains("fileChange", StringComparison.Ordinal)
                                ? "file_change"
                                : "command_execution";
                            bool approved = await onApproval(
                                new AgentProcessApprovalRequest(toolName,
                                    parameters.ValueKind == JsonValueKind.Undefined ? "{}" : parameters.GetRawText()),
                                cancellationToken);
                            await WriteAsync(process, new
                            {
                                id = requestId,
                                result = new { decision = approved ? "accept" : "decline" }
                            }, cancellationToken);
                            continue;
                        }

                        if (parameters.ValueKind != JsonValueKind.Undefined)
                        {
                            foreach (AgentProcessEvent processEvent in
                                     AgentJsonEventNormalizer.NormalizeCodex(method, parameters))
                            {
                                await onEvent(processEvent, cancellationToken);
                            }
                        }

                        if (method.Equals("turn/completed", StringComparison.Ordinal))
                        {
                            break;
                        }

                        continue;
                    }

                    if (!root.TryGetProperty("id", out JsonElement responseId)
                        || !responseId.TryGetInt32(out int id)
                        || !root.TryGetProperty("result", out JsonElement result))
                    {
                        continue;
                    }

                    if (id == 1)
                    {
                        await WriteAsync(process, new { method = "initialized", @params = new { } }, cancellationToken);
                        await WriteAsync(process, new
                        {
                            id = 2,
                            method = "thread/start",
                            @params = new
                            {
                                cwd = request.WorkingDirectory,
                                approvalPolicy = "untrusted",
                                sandbox = "workspace-write"
                            }
                        }, cancellationToken);
                    }
                    else if (id == 2 && TryGetThreadId(result, out string? threadId))
                    {
                        await WriteAsync(process, new
                        {
                            id = 3,
                            method = "turn/start",
                            @params = new
                            {
                                threadId,
                                input = new[] { new { type = "text", text = request.Prompt } }
                            }
                        }, cancellationToken);
                        turnStarted = true;
                    }
                }
            }

            if (!process.HasExited)
            {
                process.StandardInput.Close();
            }

            await process.WaitForExitAsync(cancellationToken);
            await standardError;
            if (!turnStarted || process.ExitCode != 0)
            {
                await onEvent(new AgentProcessEvent(AgentSessionEventType.Error,
                    JsonSerializer.Serialize(new { message = $"Codex exited with code {process.ExitCode}." })),
                    cancellationToken);
                throw new InvalidOperationException($"Codex exited with code {process.ExitCode}.");
            }
        }

        private static async Task WriteAsync(Process process, object message, CancellationToken cancellationToken)
        {
            await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(message).AsMemory(), cancellationToken);
            await process.StandardInput.FlushAsync(cancellationToken);
        }

        private static bool TryGetThreadId(JsonElement result, out string? threadId)
        {
            threadId = null;
            if (result.TryGetProperty("thread", out JsonElement thread)
                && thread.TryGetProperty("id", out JsonElement nestedId))
            {
                threadId = nestedId.GetString();
            }
            else if (result.TryGetProperty("threadId", out JsonElement directId))
            {
                threadId = directId.GetString();
            }

            return !string.IsNullOrWhiteSpace(threadId);
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
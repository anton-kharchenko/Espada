using Espada.Cli.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Espada.Cli.Infrastructure
{
    internal sealed class LocalApiClient(LocalRuntimeClient runtime)
    {
        public async Task<CliHttpResult> SendAsync(HttpMethod method, string path, object? body,
            string? idempotencyKey, CancellationToken cancellationToken)
        {
            LocalRuntimeState state = runtime.ReadState()
                ?? throw new InvalidOperationException("Espada runtime state is unavailable.");
            using HttpClient client = new();
            using HttpRequestMessage request = new(method, new Uri($"http://127.0.0.1:{state.ApiPort}{path}"));
            request.Headers.Add("X-Espada-Api-Key", runtime.ReadApiKey());
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                request.Headers.Add("Idempotency-Key", idempotencyKey);
            }

            if (body is not null)
            {
                request.Content = new StringContent(JsonSerializer.Serialize(body, CliJson.Options), Encoding.UTF8,
                    "application/json");
            }

            using HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return new CliHttpResult((int)response.StatusCode, content);
        }
        public async Task<BootstrapLinkResponse> CreateSetupLinkAsync(string repositoryPath,
            CancellationToken cancellationToken)
        {
            LocalRuntimeState state = runtime.ReadState()
                ?? throw new InvalidOperationException("Espada runtime state is unavailable.");
            string returnUrl = $"/setup?path={Uri.EscapeDataString(Path.GetFullPath(repositoryPath))}";
            Uri endpoint = new(
                $"http://127.0.0.1:{state.ApiPort}/bff/auth/bootstrap-link?returnUrl={Uri.EscapeDataString(returnUrl)}");
            using HttpClient client = new();
            using HttpResponseMessage response = await client.PostAsync(endpoint, null, cancellationToken);
            response.EnsureSuccessStatusCode();
            BootstrapLinkResponse link = await response.Content.ReadFromJsonAsync<BootstrapLinkResponse>(
                CliJson.Options, cancellationToken)
                ?? throw new InvalidOperationException("Espada returned an invalid setup link.");
            Uri absolute = Uri.TryCreate(link.Url, UriKind.Absolute, out Uri? parsed)
                ? parsed
                : new Uri(endpoint, link.Url);
            return link with { Url = absolute.AbsoluteUri };
        }
    }
}
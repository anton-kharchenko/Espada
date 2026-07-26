using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Espada.Tests.Common.Http;

public sealed class HttpTestClient
{
    private readonly CancellationToken _cancellationToken;
    private readonly HttpClient _client;

    public HttpTestClient(HttpClient client, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(client);

        _client = client;
        _cancellationToken = cancellationToken;
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string route, TRequest request)
    {
        using HttpResponseMessage response = await _client.PostAsJsonAsync(route, request, _cancellationToken);

        await EnsureStatusAsync(response, HttpStatusCode.Created);

        return await ReadRequiredAsync<TResponse>(response);
    }

    public async Task<TResponse> PostAsync<TResponse>(string route)
    {
        using HttpResponseMessage response = await _client.PostAsync(route, content: null, _cancellationToken);

        await EnsureStatusAsync(response, HttpStatusCode.Created);

        return await ReadRequiredAsync<TResponse>(response);
    }

    public async Task<TResponse> GetAsync<TResponse>(string route)
    {
        using HttpResponseMessage response = await _client.GetAsync(route, _cancellationToken);

        await EnsureStatusAsync(response, HttpStatusCode.OK);

        return await ReadRequiredAsync<TResponse>(response);
    }

    public async Task AssertStatusAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        using (response)
        {
            await EnsureStatusAsync(response, expected);
        }
    }

    private async Task<TResponse> ReadRequiredAsync<TResponse>(HttpResponseMessage response)
    {
        TResponse? content = await response.Content.ReadFromJsonAsync<TResponse>(_cancellationToken);

        content.Should().NotBeNull();

        return content!;
    }

    private async Task EnsureStatusAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        string content = await response.Content.ReadAsStringAsync(_cancellationToken);

        response.StatusCode.Should().Be(expected, "response content was: {0}", content);
    }
}
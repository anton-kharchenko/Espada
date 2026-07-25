using System.Net;
using System.Text.Json;

namespace Espada.Tests.Api.Assertions;

internal static class HttpValidationAssertions
{
    public static async Task ShouldHaveStatusCodeAsync(this HttpResponseMessage response, HttpStatusCode expectedStatusCode)
    {
        if (response.StatusCode == expectedStatusCode)
        {
            return;
        }

        string body = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == expectedStatusCode,
            $"Expected status code '{expectedStatusCode}', but received '{response.StatusCode}'. Response body: {body}");
    }

    public static async Task ShouldContainValidationErrorAsync(this HttpResponseMessage response, string propertyName)
    {
        string body = await response.Content.ReadAsStringAsync();

        using JsonDocument document = JsonDocument.Parse(body);

        Assert.True(
            document.RootElement.TryGetProperty("errors", out JsonElement errors),
            $"Validation response does not contain an 'errors' property. Response body: {body}");

        Assert.True(
            errors.TryGetProperty(propertyName, out _),
            $"Validation error for '{propertyName}' was not found. Response body: {body}");
    }
}
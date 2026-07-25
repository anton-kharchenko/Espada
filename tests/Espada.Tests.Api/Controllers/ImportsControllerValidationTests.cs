using Espada.Tests.Api.Assertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Espada.Api.Contracts.Requests.Imports;
using Espada.Tests.Api.Fixtures;
using Espada.Tests.Api.TestData;

namespace Espada.Tests.Api.Controllers;

public sealed class ImportsControllerValidationTests(EspadaApiFactory factory) : IClassFixture<EspadaApiFactory>
{
    [Fact]
    public async Task Complete_WithEmptyArtifactId_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        CompleteImportRequest request = new()
        {
            ArtifactId = Guid.Empty,
            ArtifactRevisionId = TestIds.ArtifactRevisionId
        };

        HttpResponseMessage response = await client.PostAsJsonAsync(ApiRoutes.Imports.Complete(TestIds.WorkspaceId, TestIds.ImportJobId), request, cancellationToken: TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
        Assert.True(await HasValidationErrorAsync(response, nameof(CompleteImportRequest.ArtifactId)));
    }

    [Fact]
    public async Task Complete_WithEmptyArtifactRevisionId_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        CompleteImportRequest request = new()
        {
            ArtifactId = TestIds.ArtifactId,
            ArtifactRevisionId = Guid.Empty
        };

        HttpResponseMessage response = await client.PostAsJsonAsync(ApiRoutes.Imports.Complete(TestIds.WorkspaceId, TestIds.ImportJobId), request, cancellationToken: TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
        Assert.True(await HasValidationErrorAsync(response, nameof(CompleteImportRequest.ArtifactRevisionId)));
    }

    [Fact]
    public async Task Fail_WithEmptyFailureCode_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        FailImportRequest request = new()
        {
            FailureCode = " ",
            FailureReason = TestValues.ImportFailureReason
        };

        HttpResponseMessage response = await client.PostAsJsonAsync(ApiRoutes.Imports.Fail(TestIds.WorkspaceId, TestIds.ImportJobId), request, cancellationToken: TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
        Assert.True(await HasValidationErrorAsync(response, nameof(FailImportRequest.FailureCode)));
    }

    [Fact]
    public async Task Fail_WithEmptyFailureReason_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        FailImportRequest request = new()
        {
            FailureCode = TestValues.ImportFailureCode,
            FailureReason = " "
        };

        HttpResponseMessage response = await client.PostAsJsonAsync(ApiRoutes.Imports.Fail(TestIds.WorkspaceId, TestIds.ImportJobId), request, cancellationToken: TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
        Assert.True(await HasValidationErrorAsync(response, nameof(FailImportRequest.FailureReason)));
    }

    private static async Task<bool> HasValidationErrorAsync(HttpResponseMessage response, string propertyName)
    {
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        JsonElement errors = document.RootElement.GetProperty("errors");

        return errors.TryGetProperty(propertyName, out _);
    }
}
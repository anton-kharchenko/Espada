using System.Net;
using System.Net.Http.Json;
using Espada.Api.Contracts.Requests.ArtifactRevisions;
using Espada.Tests.Api.Assertions;
using Espada.Tests.Api.Fixtures;
using Espada.Tests.Api.TestData;

namespace Espada.Tests.Api.Controllers;

public sealed class ArtifactRevisionsControllerValidationTests(EspadaApiFactory factory) : IClassFixture<EspadaApiFactory>
{
    [Fact]
    public async Task Add_WithEmptyContent_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        AddArtifactRevisionRequest request = new()
        {
            Content = " "
        };

        HttpResponseMessage response = await client.PostAsJsonAsync(ApiRoutes.ArtifactRevisions.Add(TestIds.WorkspaceId, TestIds.ArtifactId), request, cancellationToken: TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
        await response.ShouldContainValidationErrorAsync(nameof(AddArtifactRevisionRequest.Content));
    }
}
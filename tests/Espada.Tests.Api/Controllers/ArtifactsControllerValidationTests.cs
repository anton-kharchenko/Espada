using Espada.Api.Contracts.Requests.Artifacts;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Tests.Api.Assertions;
using Espada.Tests.Api.Fixtures;
using Espada.Tests.Api.TestData;
using Espada.Tests.Api.TestData.Routes;
using System.Net;
using System.Net.Http.Json;

namespace Espada.Tests.Api.Controllers
{
    public sealed class ArtifactsControllerValidationTests(EspadaApiFactory factory) : IClassFixture<EspadaApiFactory>
    {
        [Fact]
        public async Task Create_WithUnsupportedTypeId_ShouldReturnBadRequest()
        {
            using HttpClient client = factory.CreateHttpsClient();

            CreateArtifactRequest request = new()
            {
                Title = TestValues.ArtifactTitle,
                TypeId = int.MaxValue,
                Content = TestValues.ArtifactContent
            };

            HttpResponseMessage response = await client.PostAsJsonAsync(ArtifactApiRoutes.Create(TestIds.WorkspaceId),
                request, TestContext.Current.CancellationToken);

            await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
            await response.ShouldContainValidationErrorAsync(nameof(CreateArtifactRequest.TypeId));
        }

        [Fact]
        public async Task Create_WithEmptyTitle_ShouldReturnBadRequest()
        {
            using HttpClient client = factory.CreateHttpsClient();
            ArtifactType artifactType = Enumeration.GetAll<ArtifactType>().First();

            CreateArtifactRequest request = new()
            {
                Title = " ",
                TypeId = artifactType.Id,
                Content = TestValues.ArtifactContent
            };

            HttpResponseMessage response = await client.PostAsJsonAsync(ArtifactApiRoutes.Create(TestIds.WorkspaceId),
                request, TestContext.Current.CancellationToken);

            await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
            await response.ShouldContainValidationErrorAsync(nameof(CreateArtifactRequest.Title));
        }

        [Fact]
        public async Task Create_WithEmptyContent_ShouldReturnBadRequest()
        {
            using HttpClient client = factory.CreateHttpsClient();
            ArtifactType artifactType = Enumeration.GetAll<ArtifactType>().First();

            CreateArtifactRequest request = new()
            {
                Title = TestValues.ArtifactTitle,
                TypeId = artifactType.Id,
                Content = " "
            };

            HttpResponseMessage response = await client.PostAsJsonAsync(ArtifactApiRoutes.Create(TestIds.WorkspaceId),
                request, TestContext.Current.CancellationToken);

            await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
            await response.ShouldContainValidationErrorAsync(nameof(CreateArtifactRequest.Content));
        }

        [Fact]
        public async Task Rename_WithEmptyTitle_ShouldReturnBadRequest()
        {
            using HttpClient client = factory.CreateHttpsClient();

            RenameArtifactRequest request = new() { Title = " " };

            HttpResponseMessage response = await client.PostAsJsonAsync(
                ArtifactApiRoutes.Rename(TestIds.WorkspaceId, TestIds.ArtifactId), request,
                TestContext.Current.CancellationToken);

            await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
            await response.ShouldContainValidationErrorAsync(nameof(RenameArtifactRequest.Title));
        }
    }
}
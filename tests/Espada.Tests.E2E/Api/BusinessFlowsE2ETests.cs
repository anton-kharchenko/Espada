using Espada.Api.Contracts.Requests.ArtifactRevisions;
using Espada.Api.Contracts.Requests.Artifacts;
using Espada.Api.Contracts.Requests.ChunkBatches;
using Espada.Api.Contracts.Requests.ChunkEmbeddings;
using Espada.Api.Contracts.Requests.Chunks;
using Espada.Api.Contracts.Requests.Imports;
using Espada.Api.Contracts.Requests.Sources;
using Espada.Api.Contracts.Requests.Workspaces;
using Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision;
using Espada.Application.UseCases.Artifacts.Commands.CreateArtifact;
using Espada.Application.UseCases.Artifacts.Queries.GetArtifactById;
using Espada.Application.UseCases.Artifacts.Queries.ListArtifactRevisions;
using Espada.Application.UseCases.ChunkEmbeddings.Commands.CreateChunkEmbedding;
using Espada.Application.UseCases.ChunkEmbeddings.Queries.GetChunkEmbeddingByChunkId;
using Espada.Application.UseCases.Chunks.Commands.CreateChunkBatch;
using Espada.Application.UseCases.Chunks.Commands.CreateChunks;
using Espada.Application.UseCases.Chunks.Queries.GetChunkById;
using Espada.Application.UseCases.Chunks.Queries.ListChunksByRevision;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Application.UseCases.Imports.Queries.GetImportById;
using Espada.Application.UseCases.Sources.Commands.RegisterSource;
using Espada.Application.UseCases.Sources.Common;
using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Application.UseCases.Workspaces.Common;
using Espada.Domain.Enums;
using Espada.Tests.Common.Http;
using Espada.Tests.E2E.Fixtures;
using Espada.Tests.E2E.TestData;
using System.Net;

namespace Espada.Tests.E2E.Api;

[Collection(E2ECollection.Name)]
public sealed class BusinessFlowsE2ETests(EspadaE2EFactory factory) : E2ETest(factory)
{
    [Fact]
    public async Task FullLifecycle_ShouldRoundTripThroughPostgreSql()
    {
        using HttpClient client = Factory.CreateClient();
        HttpTestClient http = new(client, TestContext.Current.CancellationToken);

        CreateWorkspaceResponse createdWorkspace = await http.PostAsync<CreateWorkspaceRequest, CreateWorkspaceResponse>(E2ERoutes.Workspaces, new CreateWorkspaceRequest
        {
            Name = BusinessFlowTestData.Lifecycle.WorkspaceName,
            TypeId = WorkspaceType.Personal.Id
        });
        WorkspaceResponse workspace = await http.GetAsync<WorkspaceResponse>(E2ERoutes.Workspace(createdWorkspace.WorkspaceId));
        Assert.Equal(BusinessFlowTestData.Lifecycle.WorkspaceName, workspace.Name);

        RegisterSourceResponse createdSource = await http.PostAsync<RegisterSourceRequest, RegisterSourceResponse>(E2ERoutes.Sources(workspace.Id), new RegisterSourceRequest
        {
            Name = BusinessFlowTestData.Lifecycle.SourceName,
            Locator = BusinessFlowTestData.Lifecycle.SourceLocator,
            TypeId = SourceType.WebPage.Id
        });
        SourceResponse source = await http.GetAsync<SourceResponse>(E2ERoutes.Source(workspace.Id, createdSource.SourceId));
        Assert.Equal(workspace.Id, source.WorkspaceId);

        RequestImportResponse requestedImport = await http.PostAsync<RequestImportResponse>(E2ERoutes.RequestImport(workspace.Id, source.Id));
        await http.AssertStatusAsync(await client.PostAsync(E2ERoutes.StartImport(workspace.Id, requestedImport.ImportJobId), null, TestContext.Current.CancellationToken), HttpStatusCode.NoContent);

        CreateArtifactResponse createdArtifact = await http.PostAsync<CreateArtifactRequest, CreateArtifactResponse>(E2ERoutes.Artifacts(workspace.Id), new CreateArtifactRequest
        {
            Title = BusinessFlowTestData.Lifecycle.InitialArtifactTitle,
            TypeId = ArtifactType.Markdown.Id,
            Content = BusinessFlowTestData.Lifecycle.InitialRevisionContent
        });

        await http.AssertStatusAsync(await client.PostAsJsonAsync(E2ERoutes.CompleteImport(workspace.Id, requestedImport.ImportJobId), new CompleteImportRequest
        {
            ArtifactId = createdArtifact.ArtifactId,
            ArtifactRevisionId = createdArtifact.ArtifactRevisionId
        }, TestContext.Current.CancellationToken), HttpStatusCode.NoContent);

        GetImportByIdResponse completedImport = await http.GetAsync<GetImportByIdResponse>(E2ERoutes.Import(workspace.Id, requestedImport.ImportJobId));
        Assert.Equal(ImportStatusType.Succeeded.Id, completedImport.StatusId);
        Assert.Equal(createdArtifact.ArtifactId, completedImport.ArtifactId);
        Assert.Equal(createdArtifact.ArtifactRevisionId, completedImport.ArtifactRevisionId);
        Assert.NotNull(completedImport.CompletedAtUtc);

        GetArtifactByIdResponse artifact = await http.GetAsync<GetArtifactByIdResponse>(E2ERoutes.Artifact(workspace.Id, createdArtifact.ArtifactId));
        Assert.Equal(createdArtifact.ArtifactRevisionId, artifact.CurrentRevisionId);

        AddArtifactRevisionResponse secondRevision = await http.PostAsync<AddArtifactRevisionRequest, AddArtifactRevisionResponse>(E2ERoutes.Revisions(workspace.Id, artifact.Id), new AddArtifactRevisionRequest
        {
            Content = BusinessFlowTestData.Lifecycle.SecondRevisionContent
        });

        ListArtifactRevisionsResponse revisions = await http.GetAsync<ListArtifactRevisionsResponse>(E2ERoutes.Revisions(workspace.Id, artifact.Id));
        Assert.Equal(2, revisions.Items.Count);
        Assert.Contains(revisions.Items, item => item.Id == secondRevision.ArtifactRevisionId);

        await http.AssertStatusAsync(await client.PostAsJsonAsync(E2ERoutes.RenameArtifact(workspace.Id, artifact.Id), new RenameArtifactRequest
        {
            Title = BusinessFlowTestData.Lifecycle.RenamedArtifactTitle
        }, TestContext.Current.CancellationToken), HttpStatusCode.NoContent);
        artifact = await http.GetAsync<GetArtifactByIdResponse>(E2ERoutes.Artifact(workspace.Id, artifact.Id));
        Assert.Equal(BusinessFlowTestData.Lifecycle.RenamedArtifactTitle, artifact.Title);

        CreateChunkBatchResponse batch = await http.PostAsync<CreateChunkBatchRequest, CreateChunkBatchResponse>(E2ERoutes.ChunkBatches(workspace.Id, artifact.Id, secondRevision.ArtifactRevisionId), new CreateChunkBatchRequest
        {
            StrategyId = ChunkingStrategyType.Recursive.Id,
            StrategyVersion = BusinessFlowTestData.Lifecycle.ChunkingStrategyVersion
        });
        CreateChunksResponse chunks = await http.PostAsync<CreateChunksRequest, CreateChunksResponse>(E2ERoutes.CreateChunks(workspace.Id, batch.ChunkBatchId), new CreateChunksRequest
        {
            Items =
            [
                new CreateChunkItemRequest
                {
                    Number = BusinessFlowTestData.Lifecycle.FirstChunkNumber,
                    Content = BusinessFlowTestData.Lifecycle.FirstChunkContent,
                    SourceStart = BusinessFlowTestData.Lifecycle.FirstChunkSourceStart,
                    SourceLength = BusinessFlowTestData.Lifecycle.FirstChunkSourceLength
                },
                new CreateChunkItemRequest
                {
                    Number = BusinessFlowTestData.Lifecycle.SecondChunkNumber,
                    Content = BusinessFlowTestData.Lifecycle.SecondChunkContent,
                    SourceStart = BusinessFlowTestData.Lifecycle.SecondChunkSourceStart,
                    SourceLength = BusinessFlowTestData.Lifecycle.SecondChunkSourceLength
                }
            ]
        });
        Assert.Equal(BusinessFlowTestData.Lifecycle.ExpectedChunkCount, chunks.ChunkCount);

        ListChunksByRevisionResponse listedChunks = await http.GetAsync<ListChunksByRevisionResponse>(E2ERoutes.ChunksByRevision(workspace.Id, secondRevision.ArtifactRevisionId));
        Assert.Equal(BusinessFlowTestData.Lifecycle.ExpectedChunkCount, listedChunks.Items.Count);

        Guid chunkId = chunks.Items[0].Id;
        GetChunkByIdResponse chunk = await http.GetAsync<GetChunkByIdResponse>(E2ERoutes.Chunk(workspace.Id, chunkId));
        Assert.Equal(BusinessFlowTestData.Lifecycle.FirstChunkContent, chunk.Content);

        float[] vector = BusinessFlowTestData.Lifecycle.CreateEmbeddingVector();
        CreateChunkEmbeddingResponse createdEmbedding = await http.PostAsync<CreateChunkEmbeddingRequest, CreateChunkEmbeddingResponse>(E2ERoutes.Embedding(workspace.Id, chunkId), new CreateChunkEmbeddingRequest
        {
            ModelIdentifier = BusinessFlowTestData.Lifecycle.EmbeddingModelIdentifier,
            ModelVersion = BusinessFlowTestData.Lifecycle.EmbeddingModelVersion,
            Vector = vector
        });
        GetChunkEmbeddingByChunkIdResponse embedding = await http.GetAsync<GetChunkEmbeddingByChunkIdResponse>(E2ERoutes.Embedding(workspace.Id, chunkId));
        Assert.Equal(createdEmbedding.ChunkEmbeddingId, embedding.Id);
        Assert.Equal(vector, embedding.Vector);

        await http.AssertStatusAsync(await client.PostAsync(E2ERoutes.ArchiveArtifact(workspace.Id, artifact.Id), null, TestContext.Current.CancellationToken), HttpStatusCode.NoContent);
        artifact = await http.GetAsync<GetArtifactByIdResponse>(E2ERoutes.Artifact(workspace.Id, artifact.Id));
        Assert.Equal(ArtifactStatusType.Archived.Id, artifact.StatusId);
    }

    [Fact]
    public async Task InvalidResourcesAndTransitions_ShouldReturnContractStatusCodes()
    {
        using HttpClient client = Factory.CreateClient();
        HttpTestClient http = new(client, TestContext.Current.CancellationToken);

        await http.AssertStatusAsync(await client.GetAsync(E2ERoutes.Workspace(Guid.NewGuid()), TestContext.Current.CancellationToken), HttpStatusCode.NotFound);

        CreateWorkspaceResponse firstWorkspace = await CreateWorkspaceAsync(http, BusinessFlowTestData.InvalidTransitions.FirstWorkspaceName);
        CreateWorkspaceResponse secondWorkspace = await CreateWorkspaceAsync(http, BusinessFlowTestData.InvalidTransitions.SecondWorkspaceName);
        await http.AssertStatusAsync(await client.GetAsync(E2ERoutes.Source(firstWorkspace.WorkspaceId, Guid.NewGuid()), TestContext.Current.CancellationToken), HttpStatusCode.NotFound);

        RegisterSourceResponse source = await http.PostAsync<RegisterSourceRequest, RegisterSourceResponse>(E2ERoutes.Sources(firstWorkspace.WorkspaceId), new RegisterSourceRequest
        {
            Name = BusinessFlowTestData.InvalidTransitions.SourceName,
            Locator = BusinessFlowTestData.InvalidTransitions.SourceLocator,
            TypeId = SourceType.WebPage.Id
        });

        await http.AssertStatusAsync(await client.GetAsync(E2ERoutes.Source(secondWorkspace.WorkspaceId, source.SourceId), TestContext.Current.CancellationToken), HttpStatusCode.NotFound);

        RequestImportResponse requestedImport = await http.PostAsync<RequestImportResponse>(E2ERoutes.RequestImport(firstWorkspace.WorkspaceId, source.SourceId));
        await http.AssertStatusAsync(await client.PostAsJsonAsync(E2ERoutes.CompleteImport(firstWorkspace.WorkspaceId, requestedImport.ImportJobId), new CompleteImportRequest
        {
            ArtifactId = Guid.NewGuid(),
            ArtifactRevisionId = Guid.NewGuid()
        }, TestContext.Current.CancellationToken), HttpStatusCode.Conflict);

        CreateArtifactResponse artifact = await http.PostAsync<CreateArtifactRequest, CreateArtifactResponse>(E2ERoutes.Artifacts(firstWorkspace.WorkspaceId), new CreateArtifactRequest
        {
            Title = BusinessFlowTestData.InvalidTransitions.ArtifactTitle,
            TypeId = ArtifactType.Markdown.Id,
            Content = BusinessFlowTestData.InvalidTransitions.InitialRevisionContent
        });
        await http.AssertStatusAsync(await client.PostAsync(E2ERoutes.ArchiveArtifact(firstWorkspace.WorkspaceId, artifact.ArtifactId), null, TestContext.Current.CancellationToken), HttpStatusCode.NoContent);
        await http.AssertStatusAsync(await client.PostAsync(E2ERoutes.ArchiveArtifact(firstWorkspace.WorkspaceId, artifact.ArtifactId), null, TestContext.Current.CancellationToken), HttpStatusCode.Conflict);
        await http.AssertStatusAsync(await client.PostAsJsonAsync(E2ERoutes.Revisions(firstWorkspace.WorkspaceId, artifact.ArtifactId), new AddArtifactRevisionRequest
        {
            Content = BusinessFlowTestData.InvalidTransitions.RejectedRevisionContent
        }, TestContext.Current.CancellationToken), HttpStatusCode.Conflict);

        await http.AssertStatusAsync(await client.PostAsJsonAsync(E2ERoutes.Workspaces, new CreateWorkspaceRequest(), TestContext.Current.CancellationToken), HttpStatusCode.BadRequest);
    }

    private static Task<CreateWorkspaceResponse> CreateWorkspaceAsync(HttpTestClient http, string name) =>
        http.PostAsync<CreateWorkspaceRequest, CreateWorkspaceResponse>(E2ERoutes.Workspaces, new CreateWorkspaceRequest
        {
            Name = name,
            TypeId = WorkspaceType.Personal.Id
        });
}
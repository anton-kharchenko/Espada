using AutoMapper;
using Espada.Application.Contracts.Embedding;
using Espada.Application.Models;
using Espada.Application.UseCases.Context.Queries.SearchWorkspaceContext;
using Espada.Daemon.Models;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Protocol.Mcp.Contracts.Requests;
using Espada.Protocol.Mcp.Contracts.Responses;
using Espada.Protocol.Mcp.Service;
using MediatR;

namespace Espada.Daemon.Services;

internal sealed class LocalContextSearchToolService(IEmbeddingGeneratorService embeddingGeneratorService, IMediator mediator, IMapper mapper) : IContextSearchToolService
{
    public async Task<ContextSearchResponse> SearchAsync(ContextSearchRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.WorkspaceId == Guid.Empty)
        {
            throw new ArgumentException("Workspace ID cannot be empty.", nameof(request));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(request.QueryText);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ModelIdentifier);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ModelVersion);

        if (request.TopK is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "TopK must be between 1 and 100.");
        }

        if (request.MinimumSimilarity is < -1d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "MinimumSimilarity must be between -1 and 1.");
        }

        if (request.MinimumArtifactPriority is < ContextPriority.Minimum or > ContextPriority.Maximum || request.MinimumSourcePriority is < ContextPriority.Minimum or > ContextPriority.Maximum)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Minimum priorities must be between -100 and 100.");
        }

        GeneratedEmbedding embedding = await embeddingGeneratorService.GenerateAsync(request.ModelIdentifier, request.ModelVersion, request.QueryText, cancellationToken);

        SearchWorkspaceContextQuery query = mapper.Map<SearchWorkspaceContextQuery>(new ContextSearchMappingSource(request, embedding.Vector));
        DomainResult<SearchWorkspaceContextResponse> result = await mediator.Send(query, cancellationToken);

        return result.IsFailure ? throw new InvalidOperationException($"{result.Error.Code}: {result.Error.Description}") : mapper.Map<ContextSearchResponse>(result.Value);
    }
}
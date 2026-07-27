using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.UseCases.Sources.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Sources.Queries.GetSourceById
{
    internal sealed class GetSourceByIdQueryHandler(ISourceRepository sourceRepository, IMapper mapper) : IQueryHandler<GetSourceByIdQuery, SourceResponse>
    {
        public async Task<DomainResult<SourceResponse>> Handle(GetSourceByIdQuery request, CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<SourceResponse>(WorkspaceApplicationErrors.InvalidId);
            }

            if (request.SourceId == Guid.Empty)
            {
                return DomainResult.Failure<SourceResponse>(SourceApplicationErrors.InvalidId);
            }

            SourceId sourceId = SourceId.Create(request.SourceId);

            Source? source = await sourceRepository.GetByIdAsync(sourceId, cancellationToken);

            if (source is null)
            {
                return DomainResult.Failure<SourceResponse>(SourceApplicationErrors.NotFound(request.SourceId));
            }

            if (source.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult.Failure<SourceResponse>(SourceApplicationErrors.NotFoundInWorkspace(request.SourceId, request.WorkspaceId));
            }

            SourceResponse response = mapper.Map<SourceResponse>(source);

            return DomainResult.Success(response);
        }
    }
}
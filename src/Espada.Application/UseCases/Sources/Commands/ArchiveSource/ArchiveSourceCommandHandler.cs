using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Sources.Commands.ArchiveSource
{
    internal sealed class ArchiveSourceCommandHandler(ISourceRepository sourceRepository, IUnitOfWork unitOfWork, IClockService clockService) : ICommandHandler<ArchiveSourceCommand>
    {
        public async Task<DomainResult> Handle(ArchiveSourceCommand request, CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure(WorkspaceApplicationErrors.InvalidId);
            }

            if (request.SourceId == Guid.Empty)
            {
                return DomainResult.Failure(SourceApplicationErrors.InvalidId);
            }

            SourceId sourceId = SourceId.Create(request.SourceId);

            Source? source = await sourceRepository.GetByIdAsync(sourceId, cancellationToken);

            if (source is null)
            {
                return DomainResult.Failure(SourceApplicationErrors.NotFound(request.SourceId));
            }

            if (source.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult.Failure(SourceApplicationErrors.NotFoundInWorkspace(request.SourceId, request.WorkspaceId));
            }

            DomainResult archiveResult = source.Archive(clockService.UtcNow);

            if (archiveResult.IsFailure)
            {
                return archiveResult;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DomainResult.Success();
        }
    }
}
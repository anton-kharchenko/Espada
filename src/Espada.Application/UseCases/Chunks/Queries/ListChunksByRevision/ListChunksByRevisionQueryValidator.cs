using FluentValidation;

namespace Espada.Application.UseCases.Chunks.Queries.ListChunksByRevision;

internal sealed class ListChunksByRevisionQueryValidator : AbstractValidator<ListChunksByRevisionQuery>
{
    public ListChunksByRevisionQueryValidator()
    {
        RuleFor(query => query.WorkspaceId).NotEmpty();
        RuleFor(query => query.ArtifactRevisionId).NotEmpty();
    }
}
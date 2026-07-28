using FluentValidation;

namespace Espada.Application.UseCases.Chunks.Queries.GetChunkById
{
    internal sealed class GetChunkByIdQueryValidator : AbstractValidator<GetChunkByIdQuery>
    {
        public GetChunkByIdQueryValidator()
        {
            RuleFor(query => query.WorkspaceId).NotEmpty();
            RuleFor(query => query.ChunkId).NotEmpty();
        }
    }
}
using FluentValidation;

namespace Espada.Application.UseCases.ChunkEmbeddings.Queries.GetChunkEmbeddingByChunkId
{
    internal sealed class GetChunkEmbeddingByChunkIdQueryValidator : AbstractValidator<GetChunkEmbeddingByChunkIdQuery>
    {
        public GetChunkEmbeddingByChunkIdQueryValidator()
        {
            RuleFor(query => query.WorkspaceId).NotEmpty();
            RuleFor(query => query.ChunkId).NotEmpty();
            RuleFor(query => query.ModelIdentifier).NotEmpty();
            RuleFor(query => query.ModelVersion).NotEmpty();
        }
    }
}
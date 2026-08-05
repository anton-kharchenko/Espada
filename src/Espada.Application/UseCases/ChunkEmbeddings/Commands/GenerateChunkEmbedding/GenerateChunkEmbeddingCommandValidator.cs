using FluentValidation;

namespace Espada.Application.UseCases.ChunkEmbeddings.Commands.GenerateChunkEmbedding
{
    internal sealed class GenerateChunkEmbeddingCommandValidator : AbstractValidator<GenerateChunkEmbeddingCommand>
    {
        public GenerateChunkEmbeddingCommandValidator()
        {
            RuleFor(command => command.WorkspaceId).NotEmpty();
            RuleFor(command => command.ChunkId).NotEmpty();
            RuleFor(command => command.ModelIdentifier).NotEmpty();
            RuleFor(command => command.ModelVersion).NotEmpty();
        }
    }
}
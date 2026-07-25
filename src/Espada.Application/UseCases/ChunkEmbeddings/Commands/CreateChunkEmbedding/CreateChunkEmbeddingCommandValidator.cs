using FluentValidation;

namespace Espada.Application.UseCases.ChunkEmbeddings.Commands.CreateChunkEmbedding;

internal sealed class CreateChunkEmbeddingCommandValidator : AbstractValidator<CreateChunkEmbeddingCommand>
{
    public CreateChunkEmbeddingCommandValidator()
    {
        RuleFor(command => command.WorkspaceId).NotEmpty();
        RuleFor(command => command.ChunkId).NotEmpty();
        RuleFor(command => command.ModelIdentifier).NotEmpty();
        RuleFor(command => command.ModelVersion).NotEmpty();
        RuleFor(command => command.Vector).NotNull().NotEmpty();
    }
}
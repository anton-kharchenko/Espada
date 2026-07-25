using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using FluentValidation;

namespace Espada.Application.UseCases.Chunks.Commands.CreateChunkBatch;

internal sealed class CreateChunkBatchCommandValidator : AbstractValidator<CreateChunkBatchCommand>
{
    public CreateChunkBatchCommandValidator()
    {
        RuleFor(command => command.WorkspaceId).NotEmpty();
        RuleFor(command => command.ArtifactId).NotEmpty();
        RuleFor(command => command.ArtifactRevisionId).NotEmpty();
        RuleFor(command => command.StrategyId).Must(strategyId => Enumeration.GetAll<ChunkingStrategyType>().Any(strategy => strategy.Id == strategyId));
        RuleFor(command => command.StrategyVersion).NotEmpty();
    }
}
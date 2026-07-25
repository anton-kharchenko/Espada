using FluentValidation;

namespace Espada.Application.UseCases.Chunks.Commands.CreateChunks;

internal sealed class CreateChunksCommandValidator : AbstractValidator<CreateChunksCommand>
{
    public CreateChunksCommandValidator()
    {
        RuleFor(command => command.WorkspaceId).NotEmpty();
        RuleFor(command => command.ChunkBatchId).NotEmpty();
        RuleFor(command => command.Items).NotNull().NotEmpty();

        RuleForEach(command => command.Items).ChildRules(item =>
        {
            item.RuleFor(value => value.Number).GreaterThan(0);
            item.RuleFor(value => value.Content).Must(content => !string.IsNullOrWhiteSpace(content));
            item.RuleFor(value => value).Must(value => value.SourceStart.HasValue == value.SourceLength.HasValue)
                .WithMessage("SourceStart and SourceLength must either both be provided or both be omitted.");
        });
    }
}
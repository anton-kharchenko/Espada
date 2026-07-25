using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using FluentValidation;

namespace Espada.Application.UseCases.Artifacts.Commands.CreateArtifact
{
    internal sealed class CreateArtifactCommandValidator : AbstractValidator<CreateArtifactCommand>
    {
        public CreateArtifactCommandValidator()
        {
            RuleFor(command => command.WorkspaceId)
                .NotEmpty();

            RuleFor(command => command.Title)
                .Must(title => !string.IsNullOrWhiteSpace(title))
                .MaximumLength(ArtifactTitle.MaxLength);

            RuleFor(command => command.TypeId)
                .Must(IsSupportedArtifactType);

            RuleFor(command => command.Content)
                .Must(content => !string.IsNullOrWhiteSpace(content));
        }

        private static bool IsSupportedArtifactType(int typeId) =>
            Enumeration
                .GetAll<ArtifactType>()
                .Any(type => type.Id == typeId);
    }
}
using FluentValidation;

namespace Espada.Application.UseCases.LocalSetup.Commands.CommitLocalSetup
{
    internal sealed class CommitLocalSetupCommandValidator : AbstractValidator<CommitLocalSetupCommand>
    {
        public CommitLocalSetupCommandValidator()
        {
            RuleFor(command => command.SetupId).NotEmpty();
            RuleFor(command => command.DeviceId).NotEmpty();
            RuleFor(command => command.WorkspaceName).NotEmpty().MaximumLength(200);
            RuleFor(command => command.ProjectName).NotEmpty().MaximumLength(200);
            RuleFor(command => command.RepositoryRoot).NotEmpty().MaximumLength(2048);
            RuleFor(command => command.InitialInstruction).NotEmpty();
            RuleFor(command => command.IdentityIssuer).NotEmpty();
            RuleFor(command => command.IdentitySubject).NotEmpty();
            RuleFor(command => command.DeviceName).NotEmpty().MaximumLength(200);
        }
    }
}

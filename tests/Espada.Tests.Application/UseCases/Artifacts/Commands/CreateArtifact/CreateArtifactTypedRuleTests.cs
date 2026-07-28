using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Artifacts.Commands.CreateArtifact;
using Espada.Application.UseCases.Artifacts.Common;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;

namespace Espada.Tests.Application.UseCases.Artifacts.Commands.CreateArtifact
{
    public sealed class CreateArtifactTypedRuleTests
    {
        [Fact]
        public async Task Handle_WithMemoryKind_ShouldRequireRememberCommand()
        {
            CreateArtifactHandlerFixture fixture = new();
            CreateArtifactCommandHandler handler = fixture.CreateHandler();
            CreateArtifactCommand command = CreateCommand(ArtifactKindType.Memory.Id);

            DomainResult<CreateArtifactResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            result.ShouldFailWith(ArtifactApplicationErrors.MemoryRequiresRememberCommand);
            fixture.UnitOfWork.SaveChangesCallCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithInstructionWithoutRules_ShouldFail()
        {
            CreateArtifactHandlerFixture fixture = new();
            fixture.GivenWorkspaceExists();
            CreateArtifactCommandHandler handler = fixture.CreateHandler();
            CreateArtifactCommand command = CreateCommand(ArtifactKindType.Instruction.Id);

            DomainResult<CreateArtifactResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Artifact.Rules.Required");
            fixture.UnitOfWork.SaveChangesCallCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithInstructionRule_ShouldPersistTypedRule()
        {
            CreateArtifactHandlerFixture fixture = new();
            fixture.GivenWorkspaceExists();
            CreateArtifactCommandHandler handler = fixture.CreateHandler();
            CreateArtifactCommand command = CreateCommand(
                ArtifactKindType.Instruction.Id,
                instructionRules: [new InstructionRuleInput("database.migrations", "Require rollback.", 50)]);

            DomainResult<CreateArtifactResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            result.ShouldSucceed();
            fixture.InstructionRuleRepository.AddedRules.Should().ContainSingle();
            fixture.PolicyRuleRepository.AddedRules.Should().BeEmpty();
            fixture.UnitOfWork.SaveChangesCallCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_WithPolicyRule_ShouldPersistEnforcement()
        {
            CreateArtifactHandlerFixture fixture = new();
            fixture.GivenWorkspaceExists();
            CreateArtifactCommandHandler handler = fixture.CreateHandler();
            CreateArtifactCommand command = CreateCommand(
                ArtifactKindType.Policy.Id,
                policyRules: [new PolicyRuleInput("git.main", "Do not push.", 100, PolicyEnforcementType.Hard.Id)]);

            DomainResult<CreateArtifactResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            result.ShouldSucceed();
            fixture.PolicyRuleRepository.AddedRules.Should().ContainSingle();
            fixture.PolicyRuleRepository.AddedRules[0].EnforcementType.Should().Be(PolicyEnforcementType.Hard);
        }

        [Fact]
        public async Task Handle_WithDuplicateNormalizedInstructionRuleKeys_ShouldFail()
        {
            CreateArtifactHandlerFixture fixture = new();
            fixture.GivenWorkspaceExists();
            CreateArtifactCommandHandler handler = fixture.CreateHandler();
            CreateArtifactCommand command = CreateCommand(
                ArtifactKindType.Instruction.Id,
                instructionRules:
                [
                    new InstructionRuleInput("database.migrations", "First.", 50),
                    new InstructionRuleInput(" database.migrations ", "Second.", 40)
                ]);

            DomainResult<CreateArtifactResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Artifact.RuleKey.Duplicate");
            fixture.InstructionRuleRepository.AddedRules.Should().BeEmpty();
            fixture.UnitOfWork.SaveChangesCallCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithDuplicateNormalizedPolicyRuleKeys_ShouldFail()
        {
            CreateArtifactHandlerFixture fixture = new();
            fixture.GivenWorkspaceExists();
            CreateArtifactCommandHandler handler = fixture.CreateHandler();
            CreateArtifactCommand command = CreateCommand(
                ArtifactKindType.Policy.Id,
                policyRules:
                [
                    new PolicyRuleInput("git.main", "First.", 100, PolicyEnforcementType.Hard.Id),
                    new PolicyRuleInput(" git.main ", "Second.", 90, PolicyEnforcementType.Hard.Id)
                ]);

            DomainResult<CreateArtifactResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Artifact.RuleKey.Duplicate");
            fixture.PolicyRuleRepository.AddedRules.Should().BeEmpty();
            fixture.UnitOfWork.SaveChangesCallCount.Should().Be(0);
        }
        private static CreateArtifactCommand CreateCommand(
            int kindTypeId,
            IReadOnlyList<InstructionRuleInput>? instructionRules = null,
            IReadOnlyList<PolicyRuleInput>? policyRules = null)
        {
            return new CreateArtifactCommand(
                TestIds.DefaultWorkspaceId.Value,
                TestValues.ArtifactTitle,
                ArtifactType.Markdown.Id,
                TestValues.ArtifactContent,
                kindTypeId,
                instructionRules,
                policyRules);
        }
    }
}
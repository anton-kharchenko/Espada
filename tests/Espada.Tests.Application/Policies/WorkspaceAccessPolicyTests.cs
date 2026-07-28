using Espada.Application.ApplicationErrors;
using Espada.Application.Models;
using Espada.Application.Policies;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;
using Espada.Application.Constants;

namespace Espada.Tests.Application.Policies
{
    public sealed class WorkspaceAccessPolicyTests
    {
        private const string IdentityIssuer = "https://identity.espada.local";
        private const string IdentitySubject = "user-1";
        private const string ClientId = "codex";

        [Fact]
        public void AuthorizeWorkspaceCreation_WithBootstrapScope_ShouldSucceed()
        {
            RequestPrincipalAccessorStub principalAccessor = new()
            {
                Principal = CreatePrincipal(
                    null,
                    ApplicationScopeConstants.WorkspaceCreate)
            };
            WorkspaceAccessPolicy policy = CreatePolicy(principalAccessor);

            DomainResult result = policy.AuthorizeWorkspaceCreation();

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task AuthorizeWorkspaceAsync_WithWorkspaceMismatch_ShouldNotReadRepositories()
        {
            RequestPrincipalAccessorStub principalAccessor = new()
            {
                Principal = CreatePrincipal(
                    TestIds.AnotherWorkspaceId.Value,
                    ApplicationScopeConstants.MemoryRead)
            };
            WorkspaceMembershipRepositorySpy membershipRepository = new();
            WorkspaceRepositorySpy workspaceRepository = new();
            WorkspaceAccessPolicy policy = CreatePolicy(
                principalAccessor,
                membershipRepository,
                workspaceRepository: workspaceRepository);

            DomainResult result = await policy.AuthorizeWorkspaceAsync(
                TestIds.DefaultWorkspaceId.Value,
                ApplicationScopeConstants.MemoryRead,
                CancellationToken.None);

            result.Error.Should().Be(AccessPolicyErrors.WorkspaceMismatch);
            membershipRepository.IsMemberCallCount.Should().Be(0);
            workspaceRepository.GetByIdCallCount.Should().Be(0);
        }

        [Fact]
        public async Task AuthorizeWorkspaceAsync_WithMissingScope_ShouldReturnForbidden()
        {
            RequestPrincipalAccessorStub principalAccessor = new()
            {
                Principal = CreatePrincipal(
                    TestIds.DefaultWorkspaceId.Value,
                    ApplicationScopeConstants.WorkspaceRead)
            };
            WorkspaceAccessPolicy policy = CreatePolicy(principalAccessor);

            DomainResult result = await policy.AuthorizeWorkspaceAsync(
                TestIds.DefaultWorkspaceId.Value,
                ApplicationScopeConstants.MemoryRead,
                CancellationToken.None);

            result.Error.Should().Be(
                AccessPolicyErrors.MissingScope(ApplicationScopeConstants.MemoryRead));
        }

        [Fact]
        public async Task AuthorizeWorkspaceAsync_WithOrganizationMembership_ShouldSucceed()
        {
            OrganizationId organizationId = OrganizationId.Create(
                Guid.Parse("99999999-9999-9999-9999-999999999991"));
            Organization organization = Organization.Create(
                organizationId,
                "Espada",
                TestDates.UtcNow).ShouldSucceed();
            OrganizationMembership organizationMembership =
                organization.CreateMembership(
                    OrganizationMembershipId.New(),
                    IdentityIssuer,
                    IdentitySubject,
                    OrganizationMembershipRoleType.Member,
                    TestDates.UtcNow).ShouldSucceed();
            Workspace workspace = new WorkspaceBuilder()
                .WithOrganizationId(organizationId)
                .BuildWithoutPendingEvents();
            RequestPrincipalAccessorStub principalAccessor = new()
            {
                Principal = CreatePrincipal(
                    workspace.Id.Value,
                    ApplicationScopeConstants.ContextRead)
            };
            WorkspaceMembershipRepositorySpy membershipRepository = new() { IsMember = true };
            OrganizationMembershipRepositorySpy organizationMembershipRepository =
                new() { MembershipToReturn = organizationMembership };
            WorkspaceRepositorySpy workspaceRepository = new() { WorkspaceToReturn = workspace };
            WorkspaceAccessPolicy policy = CreatePolicy(
                principalAccessor,
                membershipRepository,
                organizationMembershipRepository,
                workspaceRepository);

            DomainResult result = await policy.AuthorizeWorkspaceAsync(
                workspace.Id.Value,
                ApplicationScopeConstants.ContextRead,
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            organizationMembershipRepository.GetByIdentityCallCount.Should().Be(1);
        }

        private static RequestPrincipal CreatePrincipal(
            Guid? workspaceId,
            params string[] scopes)
        {
            return new RequestPrincipal(
                IdentityIssuer,
                IdentitySubject,
                ClientId,
                workspaceId,
                scopes.ToHashSet(StringComparer.Ordinal),
                60,
                true);
        }

        private static WorkspaceAccessPolicy CreatePolicy(
            RequestPrincipalAccessorStub principalAccessor,
            WorkspaceMembershipRepositorySpy? membershipRepository = null,
            OrganizationMembershipRepositorySpy?
                organizationMembershipRepository = null,
            WorkspaceRepositorySpy? workspaceRepository = null)
        {
            return new WorkspaceAccessPolicy(
                principalAccessor,
                membershipRepository ?? new WorkspaceMembershipRepositorySpy(),
                organizationMembershipRepository ??
                new OrganizationMembershipRepositorySpy(),
                workspaceRepository ?? new WorkspaceRepositorySpy());
        }
    }
}
using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Mappings;
using Espada.Application.UseCases.Workspaces.Common;
using Espada.Application.UseCases.Workspaces.Queries.ListAccessibleWorkspaces;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData.Builder;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Application.UseCases.Workspaces.Queries.ListAccessibleWorkspaces
{
    public sealed class ListAccessibleWorkspacesQueryHandlerTests
    {
        private const string Issuer = "espada:test";
        private const string Subject = "test-user";

        private readonly IMapper _mapper =
            new MapperConfiguration(
                options => options.AddProfile<ApplicationMappingProfile>(),
                NullLoggerFactory.Instance)
                .CreateMapper();

        [Fact]
        public async Task Handle_ShouldReturnMappedWorkspaces()
        {
            Workspace workspace = new WorkspaceBuilder()
                .BuildWithoutPendingEvents();
            WorkspaceMembershipRepositorySpy repository = new()
            {
                Workspaces = [workspace]
            };
            ListAccessibleWorkspacesQueryHandler handler =
                new(repository, _mapper);

            DomainResult<ListAccessibleWorkspacesResponse> result =
                await handler.Handle(
                    new ListAccessibleWorkspacesQuery(Issuer, Subject),
                    CancellationToken.None);

            ListAccessibleWorkspacesResponse response = result.ShouldSucceed();
            WorkspaceResponse item = response.Items.Should().ContainSingle().Subject;
            item.Id.Should().Be(workspace.Id.Value);
            item.Name.Should().Be(workspace.Name.Value);
        }

        [Fact]
        public async Task Handle_ShouldForwardTrimmedIdentityAndCancellation()
        {
            WorkspaceMembershipRepositorySpy repository = new();
            ListAccessibleWorkspacesQueryHandler handler =
                new(repository, _mapper);
            using CancellationTokenSource source = new();

            DomainResult<ListAccessibleWorkspacesResponse> result =
                await handler.Handle(
                    new ListAccessibleWorkspacesQuery(
                        $" {Issuer} ",
                        $" {Subject} "),
                    source.Token);

            result.ShouldSucceed();
            repository.ReceivedIssuer.Should().Be(Issuer);
            repository.ReceivedSubject.Should().Be(Subject);
            repository.ListWorkspacesCancellationToken.Should().Be(
                source.Token);
        }

        [Theory]
        [InlineData("", Subject)]
        [InlineData(Issuer, " ")]
        public async Task Handle_WithMissingIdentity_ShouldReturnUnauthorized(
            string issuer,
            string subject)
        {
            WorkspaceMembershipRepositorySpy repository = new();
            ListAccessibleWorkspacesQueryHandler handler =
                new(repository, _mapper);

            DomainResult<ListAccessibleWorkspacesResponse> result =
                await handler.Handle(
                    new ListAccessibleWorkspacesQuery(issuer, subject),
                    CancellationToken.None);

            result.ShouldFailWith(AccessPolicyErrors.Unauthorized);
            repository.ListWorkspacesCallCount.Should().Be(0);
        }
    }
}
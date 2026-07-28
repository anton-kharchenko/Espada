using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class WorkspaceBuilder
    {
        private DateTimeOffset _createdAtUtc = TestDates.UtcNow;
        private WorkspaceId _id = TestIds.DefaultWorkspaceId;

        private WorkspaceName _name = WorkspaceName.Create(TestValues.WorkspaceName).ShouldSucceed();

        private OrganizationId? _organizationId;

        private WorkspaceType _type = WorkspaceTypeTestData.Any;

        public WorkspaceBuilder WithId(WorkspaceId id)
        {
            _id = id;
            return this;
        }

        public WorkspaceBuilder WithName(string name)
        {
            _name = WorkspaceName.Create(name).ShouldSucceed();

            return this;
        }

        public WorkspaceBuilder WithType(WorkspaceType type)
        {
            _type = type;
            return this;
        }

        public WorkspaceBuilder CreatedAt(DateTimeOffset createdAtUtc)
        {
            _createdAtUtc = createdAtUtc;
            return this;
        }

        public WorkspaceBuilder WithOrganizationId(OrganizationId organizationId)
        {
            _organizationId = organizationId;
            return this;
        }

        public Workspace Build()
        {
            return Workspace.Create(
                _id,
                _name,
                _type,
                _organizationId,
                _createdAtUtc).ShouldSucceed();
        }

        public Workspace BuildWithoutPendingEvents()
        {
            Workspace workspace = Build();

            workspace.DequeueDomainEvents();

            return workspace;
        }

        public Workspace BuildArchivedWithoutPendingEvents(DateTimeOffset? archivedAtUtc = null)
        {
            Workspace workspace = BuildWithoutPendingEvents();

            workspace.Archive(archivedAtUtc ?? TestDates.WorkspaceArchivedAtUtc).ShouldSucceed();

            workspace.DequeueDomainEvents();

            return workspace;
        }
    }
}
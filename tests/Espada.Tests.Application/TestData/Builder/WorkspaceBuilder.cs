using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Assertions;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class WorkspaceBuilder
    {
        private WorkspaceId _id = TestIds.WorkspaceId;

        private WorkspaceName _name = WorkspaceName.Create(TestValues.WorkspaceName).ShouldSucceed();

        private WorkspaceType _type = WorkspaceTypeTestData.Any;

        private DateTimeOffset _createdAtUtc = TestDates.UtcNow;

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

        public Workspace Build() => Workspace.Create(_id, _name, _type, _createdAtUtc).ShouldSucceed();

        public Workspace BuildWithoutPendingEvents()
        {
            Workspace workspace = Build();

            workspace.DequeueDomainEvents();

            return workspace;
        }
    }
}
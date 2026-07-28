namespace Espada.Tests.Domain.TestData.Builders
{
    internal sealed class SourceBuilder
    {
        private DateTimeOffset _createdAtUtc = TestDates.SourceCreatedAtUtc;
        private SourceId _id = TestIds.DefaultSourceId;

        private SourceLocator _locator = SourceLocator.Create("https://example.com/docs").ShouldSucceed();

        private SourceName _name = SourceName.Create("Espada documentation").ShouldSucceed();

        private SourceType _type = SourceType.WebPage;

        private WorkspaceId _workspaceId = TestIds.DefaultWorkspaceId;

        public SourceBuilder WithId(SourceId id)
        {
            _id = id;
            return this;
        }

        public SourceBuilder InWorkspace(WorkspaceId workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public SourceBuilder WithName(string name)
        {
            _name = SourceName.Create(name).ShouldSucceed();
            return this;
        }

        public SourceBuilder WithType(SourceType type)
        {
            _type = type;
            return this;
        }

        public SourceBuilder WithLocator(string locator)
        {
            _locator = SourceLocator.Create(locator).ShouldSucceed();
            return this;
        }

        public SourceBuilder CreatedAt(DateTimeOffset createdAtUtc)
        {
            _createdAtUtc = createdAtUtc;
            return this;
        }

        public Source Build()
        {
            return Source.Create(_id, _workspaceId, _name, _type, _locator, _createdAtUtc).ShouldSucceed();
        }

        public Source BuildWithoutPendingEvents()
        {
            Source source = Build();
            source.DequeueDomainEvents();

            return source;
        }
    }
}
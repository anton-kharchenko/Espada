namespace Espada.Tests.Domain.TestData.Builders
{
    internal sealed class WorkspaceBuilder
    {
        private DateTimeOffset _createdAtUtc = new(2026, 7, 24, 10, 30, 0, TimeSpan.Zero);
        private WorkspaceId _id = TestIds.DefaultWorkspaceId;

        private WorkspaceName _name = CreateName("Espada Workspace");

        private OrganizationId? _organizationId;

        private WorkspaceType _type = WorkspaceType.Personal;

        public WorkspaceBuilder WithId(WorkspaceId id)
        {
            _id = id;
            return this;
        }

        public WorkspaceBuilder WithName(string name)
        {
            _name = CreateName(name);
            return this;
        }

        public WorkspaceBuilder WithName(WorkspaceName name)
        {
            _name = name;
            return this;
        }

        public WorkspaceBuilder WithType(WorkspaceType type)
        {
            _type = type;
            return this;
        }

        public WorkspaceBuilder WithOrganizationId(OrganizationId organizationId)
        {
            _organizationId = organizationId;
            return this;
        }

        public WorkspaceBuilder CreatedAt(DateTimeOffset createdAtUtc)
        {
            _createdAtUtc = createdAtUtc;
            return this;
        }

        public DomainResult<Workspace> BuildResult()
        {
            return Workspace.Create(_id, _name, _type, _organizationId, _createdAtUtc);
        }

        public Workspace Build()
        {
            DomainResult<Workspace> result = BuildResult();

            return result.IsFailure
                ? throw new InvalidOperationException(
                    $"WorkspaceBuilder produced an invalid workspace: {result.Error.Code} — {result.Error.Description}")
                : result.Value;
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

            DateTimeOffset effectiveArchivedAtUtc =
                archivedAtUtc ?? new DateTimeOffset(2026, 7, 26, 18, 0, 0, TimeSpan.Zero);

            DomainResult archiveResult = workspace.Archive(effectiveArchivedAtUtc);

            if (archiveResult.IsFailure)
            {
                throw new InvalidOperationException(
                    $"WorkspaceBuilder could not archive workspace: {archiveResult.Error.Code} — {archiveResult.Error.Description}");
            }

            workspace.DequeueDomainEvents();

            return workspace;
        }

        private static WorkspaceName CreateName(string value)
        {
            DomainResult<WorkspaceName> result = WorkspaceName.Create(value);

            return result.IsFailure
                ? throw new InvalidOperationException(
                    $"WorkspaceBuilder received an invalid name: {result.Error.Code} — {result.Error.Description}")
                : result.Value;
        }
    }
}
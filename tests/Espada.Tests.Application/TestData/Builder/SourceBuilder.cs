using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class SourceBuilder
    {
        private SourceId _id = TestIds.SourceId;

        private WorkspaceId _workspaceId = TestIds.WorkspaceId;

        private SourceName _name = CreateName(TestValues.SourceName);

        private SourceLocator _locator = CreateLocator(TestValues.SourceLocator);

        private SourceType _type = SourceTypeTestData.Any;

        private DateTimeOffset _createdAtUtc = TestDates.UtcNow;

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
            _name = CreateName(name);
            return this;
        }

        public SourceBuilder WithLocator(string locator)
        {
            _locator = CreateLocator(locator);
            return this;
        }

        public SourceBuilder WithType(SourceType type)
        {
            _type = type;
            return this;
        }

        public SourceBuilder CreatedAt(DateTimeOffset createdAtUtc)
        {
            _createdAtUtc = createdAtUtc;
            return this;
        }

        public DomainResult<Source> BuildResult() => Source.Create(_id, _workspaceId, _name, _type, _locator, _createdAtUtc);

        public Source Build()
        {
            DomainResult<Source> result = BuildResult();

            return result.IsFailure ? throw new InvalidOperationException($"SourceBuilder produced an invalid source: {result.Error.Code} — {result.Error.Description}") : result.Value;
        }

        public Source BuildWithoutPendingEvents()
        {
            Source source = Build();

            source.DequeueDomainEvents();

            return source;
        }

        private static SourceName CreateName(string value)
        {
            DomainResult<SourceName> result = SourceName.Create(value);

            return result.IsFailure ? throw new InvalidOperationException($"SourceBuilder received an invalid name: {result.Error.Code} — {result.Error.Description}") : result.Value;
        }

        private static SourceLocator CreateLocator(string value)
        {
            DomainResult<SourceLocator> result = SourceLocator.Create(value);

            return result.IsFailure ? throw new InvalidOperationException($"SourceBuilder received an invalid locator: {result.Error.Code} — {result.Error.Description}") : result.Value;
        }
        
        public Source BuildArchivedWithoutPendingEvents(DateTimeOffset? archivedAtUtc = null)
        {
            Source source = BuildWithoutPendingEvents();

            DomainResult archiveResult = source.Archive(archivedAtUtc ?? TestDates.SourceArchivedAtUtc);

            if (archiveResult.IsFailure)
            {
                throw new InvalidOperationException($"SourceBuilder could not archive source: {archiveResult.Error.Code} — {archiveResult.Error.Description}");
            }

            source.DequeueDomainEvents();

            return source;
        }
    }
}
using Espada.Application.Contracts.Repositories;
using Espada.Application.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class RepositoryWatchRegistrationStore(EspadaDbContext dbContext)
        : IRepositoryWatchRegistrationStore
    {
        public async Task<IReadOnlyList<RepositoryWatchRegistration>> ListAsync(
            CancellationToken cancellationToken = default)
        {
            Source[] sources = await dbContext.Sources.AsNoTracking()
                .Where(source => source.Type == SourceType.Repository && source.Status == SourceStatusType.Active)
                .ToArrayAsync(cancellationToken);
            List<RepositoryWatchRegistration> registrations = [];
            foreach (Source source in sources)
            {
                if (source.Definition is not RepositorySourceDefinition repository ||
                    !Guid.TryParse(repository.RepositoryIdentity, out Guid projectId))
                {
                    continue;
                }

                Project? project = await dbContext.Projects.AsNoTracking()
                    .SingleOrDefaultAsync(candidate => candidate.Id == Domain.ValueObjects.ProjectId.Create(projectId)
                                                       && candidate.WorkspaceId == source.WorkspaceId,
                        cancellationToken);
                string? root = project?.LocalAliases.FirstOrDefault(Directory.Exists);
                if (root is not null)
                {
                    registrations.Add(new RepositoryWatchRegistration(source.WorkspaceId.Value, source.Id.Value,
                        Path.GetFullPath(root)));
                }
            }

            return registrations;
        }
    }
}
using Espada.Db.Database;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace Espada.Db.Seeding;

internal static class DbSeeder
{
    public static async Task SeedAsync(SetupDbContext dbContext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        await UpsertAsync(dbContext.WorkspaceTypes, Enumeration.GetAll<WorkspaceType>().Select(value => new WorkspaceTypes { WorkspaceTypeId = value.Id, Name = value.Name }), model => model.WorkspaceTypeId, model => model.Name, (model, name) => model.Name = name, cancellationToken);
        await UpsertAsync(dbContext.WorkspaceStatusTypes, Enumeration.GetAll<WorkspaceStatusType>().Select(value => new WorkspaceStatusTypes { WorkspaceStatusTypeId = value.Id, Name = value.Name }), model => model.WorkspaceStatusTypeId, model => model.Name, (model, name) => model.Name = name, cancellationToken);
        await UpsertAsync(dbContext.SourceTypes, Enumeration.GetAll<SourceType>().Select(value => new SourceTypes { SourceTypeId = value.Id, Name = value.Name }), model => model.SourceTypeId, model => model.Name, (model, name) => model.Name = name, cancellationToken);
        await UpsertAsync(dbContext.SourceStatusTypes, Enumeration.GetAll<SourceStatusType>().Select(value => new SourceStatusTypes { SourceStatusTypeId = value.Id, Name = value.Name }), model => model.SourceStatusTypeId, model => model.Name, (model, name) => model.Name = name, cancellationToken);
        await UpsertAsync(dbContext.ImportStatusTypes, Enumeration.GetAll<ImportStatusType>().Select(value => new ImportStatusTypes { ImportStatusTypeId = value.Id, Name = value.Name }), model => model.ImportStatusTypeId, model => model.Name, (model, name) => model.Name = name, cancellationToken);
        await UpsertAsync(dbContext.ArtifactTypes, Enumeration.GetAll<ArtifactType>().Select(value => new ArtifactTypes { ArtifactTypeId = value.Id, Name = value.Name }), model => model.ArtifactTypeId, model => model.Name, (model, name) => model.Name = name, cancellationToken);
        await UpsertAsync(dbContext.ArtifactStatusTypes, Enumeration.GetAll<ArtifactStatusType>().Select(value => new ArtifactStatusTypes { ArtifactStatusTypeId = value.Id, Name = value.Name }), model => model.ArtifactStatusTypeId, model => model.Name, (model, name) => model.Name = name, cancellationToken);
        await UpsertAsync(dbContext.ChunkingStrategyTypes, Enumeration.GetAll<ChunkingStrategyType>().Select(value => new ChunkingStrategyTypes { ChunkingStrategyTypeId = value.Id, Name = value.Name }), model => model.ChunkingStrategyTypeId, model => model.Name, (model, name) => model.Name = name, cancellationToken);
        await UpsertAsync(dbContext.ChunkBatchStatusTypes, Enumeration.GetAll<ChunkBatchStatusType>().Select(value => new ChunkBatchStatusTypes { ChunkBatchStatusTypeId = value.Id, Name = value.Name }), model => model.ChunkBatchStatusTypeId, model => model.Name, (model, name) => model.Name = name, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task UpsertAsync<TModel>(
        DbSet<TModel> dbSet,
        IEnumerable<TModel> expectedModels,
        Func<TModel, int> idSelector,
        Func<TModel, string> nameSelector,
        Action<TModel, string> setName,
        CancellationToken cancellationToken)
        where TModel : class
    {
        Dictionary<int, TModel> existingModels = (await dbSet.ToListAsync(cancellationToken)).ToDictionary(idSelector);

        foreach (TModel expectedModel in expectedModels)
        {
            int id = idSelector(expectedModel);
            if (!existingModels.TryGetValue(id, out TModel? existingModel))
            {
                dbSet.Add(expectedModel);
                continue;
            }

            string expectedName = nameSelector(expectedModel);
            if (!StringComparer.Ordinal.Equals(nameSelector(existingModel), expectedName))
            {
                setName(existingModel, expectedName);
            }
        }
    }
}
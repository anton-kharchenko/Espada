using Espada.Domain.Aggregates;
using Espada.Domain.Entities;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Espada.Infrastructure.Sync
{
    internal static class SemanticSyncSnapshotFactory
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        public static async Task<IReadOnlyList<SemanticSyncSnapshot>> CreateAsync(EspadaDbContext dbContext,
            bool includeSessionTranscripts, CancellationToken cancellationToken)
        {
            List<SemanticSyncSnapshot> snapshots = [];
            ArtifactRevision[] revisions = await dbContext.ArtifactRevisions.AsNoTracking()
                .ToArrayAsync(cancellationToken);
            Dictionary<Guid, Guid> artifactWorkspaces = await dbContext.Artifacts.AsNoTracking()
                .ToDictionaryAsync(artifact => artifact.Id.Value, artifact => artifact.WorkspaceId.Value,
                    cancellationToken);
            Dictionary<Guid, InstructionRule[]> instructionRules = (await dbContext.InstructionRules.AsNoTracking()
                    .ToArrayAsync(cancellationToken))
                .GroupBy(rule => rule.ArtifactRevisionId.Value)
                .ToDictionary(group => group.Key, group => group.ToArray());
            Dictionary<Guid, PolicyRule[]> policyRules = (await dbContext.PolicyRules.AsNoTracking()
                    .ToArrayAsync(cancellationToken))
                .GroupBy(rule => rule.ArtifactRevisionId.Value)
                .ToDictionary(group => group.Key, group => group.ToArray());
            Dictionary<Guid, MemoryMetadata[]> memories = (await dbContext.MemoryMetadata.AsNoTracking()
                    .ToArrayAsync(cancellationToken))
                .GroupBy(memory => memory.ArtifactRevisionId.Value)
                .ToDictionary(group => group.Key, group => group.ToArray());

            foreach (Workspace workspace in await dbContext.Workspaces.AsNoTracking().ToArrayAsync(cancellationToken))
            {
                Add(snapshots, workspace.Id.Value, nameof(Workspace), workspace.Id.Value, "upsert",
                    Previous(workspace.Version), workspace.ArchivedAtUtc ?? workspace.CreatedAtUtc, new
                    {
                        workspaceId = workspace.Id.Value,
                        organizationId = workspace.OrganizationId?.Value,
                        name = workspace.Name.Value,
                        type = workspace.Type.Name,
                        status = workspace.Status.Name,
                        workspace.CreatedAtUtc,

                        workspace.ArchivedAtUtc,
                        version = workspace.Version
                    });
            }

            foreach (Project project in await dbContext.Projects.AsNoTracking().ToArrayAsync(cancellationToken))
            {
                Add(snapshots, project.WorkspaceId.Value, nameof(Project), project.Id.Value, "upsert",
                    Previous(project.Version), project.UpdatedAtUtc, new
                    {
                        projectId = project.Id.Value,
                        workspaceId = project.WorkspaceId.Value,
                        project.Name,
                        project.CanonicalRemoteUri,
                        project.CreatedAtUtc,
                        project.UpdatedAtUtc,
                        version = project.Version
                    });
            }

            foreach (ProjectTask task in await dbContext.Tasks.AsNoTracking().ToArrayAsync(cancellationToken))
            {
                Add(snapshots, task.WorkspaceId.Value, nameof(ProjectTask), task.Id.Value, "upsert",
                    Previous(task.Version), task.UpdatedAtUtc, new
                    {
                        taskId = task.Id.Value,
                        workspaceId = task.WorkspaceId.Value,
                        projectId = task.ProjectId.Value,
                        task.Title,
                        status = task.Status.Name,
                        task.CreatedAtUtc,
                        task.UpdatedAtUtc,
                        task.CompletedAtUtc,
                        task.ArchivedAtUtc,
                        version = task.Version
                    });
            }

            foreach (Source source in await dbContext.Sources.AsNoTracking().ToArrayAsync(cancellationToken))
            {
                Add(snapshots, source.WorkspaceId.Value, nameof(Source), source.Id.Value, "upsert",
                    Previous(source.Version), source.UpdatedAtUtc, new
                    {
                        sourceId = source.Id.Value,
                        workspaceId = source.WorkspaceId.Value,
                        name = source.Name.Value,
                        type = source.Type.Name,
                        definition = CreateSourceDefinition(source.Definition),
                        status = source.Status.Name,
                        priority = source.Priority.Value,
                        source.CreatedAtUtc,
                        source.UpdatedAtUtc,
                        source.ArchivedAtUtc,
                        version = source.Version
                    });
            }

            foreach (Artifact artifact in await dbContext.Artifacts.AsNoTracking().ToArrayAsync(cancellationToken))
            {
                Add(snapshots, artifact.WorkspaceId.Value, nameof(Artifact), artifact.Id.Value, "upsert",
                    Previous(artifact.Version), artifact.UpdatedAtUtc, new
                    {
                        artifactId = artifact.Id.Value,
                        workspaceId = artifact.WorkspaceId.Value,
                        title = artifact.Title.Value,
                        kind = artifact.KindType.Name,
                        type = artifact.Type.Name,
                        status = artifact.Status.Name,
                        priority = artifact.Priority.Value,
                        currentRevisionId = artifact.CurrentRevisionId?.Value,
                        currentRevisionNumber = artifact.CurrentRevisionNumber?.Value,
                        artifact.CreatedAtUtc,
                        artifact.UpdatedAtUtc,
                        artifact.ArchivedAtUtc,
                        version = artifact.Version
                    });
            }

            foreach (ArtifactRevision revision in revisions)
            {
                if (!artifactWorkspaces.TryGetValue(revision.ArtifactId.Value, out Guid workspaceId))
                {
                    continue;
                }

                Add(snapshots, workspaceId, nameof(ArtifactRevision), revision.Id.Value, "append", null,
                    revision.CreatedAtUtc, new
                    {
                        revisionId = revision.Id.Value,
                        artifactId = revision.ArtifactId.Value,
                        workspaceId,
                        kind = revision.KindType.Name,
                        number = revision.Number.Value,
                        content = revision.Content.Value,
                        contentHash = revision.ContentHash.Value,
                        revision.SizeInBytes,
                        revision.CreatedAtUtc,
                        instructionRules = instructionRules.GetValueOrDefault(revision.Id.Value, [])
                            .Select(rule => new
                            {
                                key = rule.RuleKey.Value,
                                rule.Text,
                                priority = rule.Priority.Value
                            }),
                        policyRules = policyRules.GetValueOrDefault(revision.Id.Value, [])
                            .Select(rule => new
                            {
                                key = rule.RuleKey.Value,
                                rule.Text,
                                priority = rule.Priority.Value,
                                enforcement = rule.EnforcementType.Name
                            }),
                        memories = memories.GetValueOrDefault(revision.Id.Value, [])
                            .Select(memory => new
                            {
                                memoryId = memory.Id.Value,
                                category = memory.CategoryType.Name,
                                memory.Confidence,
                                memory.UserConfirmed,
                                memory.ClientIdentity,
                                memory.SessionIdentity,
                                memory.CapturedAtUtc,
                                supersededMemoryId = memory.SupersededMemoryId?.Value
                            })
                    });
            }

            foreach (ChunkBatch batch in await dbContext.ChunkBatches.AsNoTracking()
                         .Where(batch => batch.Status == ChunkBatchStatusType.Succeeded)
                         .ToArrayAsync(cancellationToken))
            {
                Add(snapshots, batch.WorkspaceId.Value, nameof(ChunkBatch), batch.Id.Value, "append", null,
                    batch.CompletedAtUtc ?? batch.RequestedAtUtc, new
                    {
                        chunkBatchId = batch.Id.Value,
                        workspaceId = batch.WorkspaceId.Value,
                        artifactId = batch.ArtifactId.Value,
                        revisionId = batch.ArtifactRevisionId.Value,
                        strategy = batch.Strategy.Name,
                        strategyVersion = batch.StrategyVersion.Value,
                        status = batch.Status.Name,
                        batch.RequestedAtUtc,
                        batch.StartedAtUtc,
                        batch.CompletedAtUtc,
                        batch.ChunkCount
                    });
            }

            foreach (Binding binding in await dbContext.Bindings.AsNoTracking().ToArrayAsync(cancellationToken))
            {
                Add(snapshots, binding.WorkspaceId.Value, nameof(Binding), binding.Id.Value, "append", null,
                    binding.CreatedAtUtc, new
                    {
                        bindingId = binding.Id.Value,
                        revisionId = binding.ArtifactRevisionId.Value,
                        workspaceId = binding.WorkspaceId.Value,
                        organizationId = binding.OrganizationId?.Value,
                        projectId = binding.ProjectId?.Value,
                        binding.RepositoryCanonicalUri,
                        binding.RepositoryRelativePathPrefix,
                        binding.Branch,
                        taskId = binding.TaskId?.Value,
                        binding.Agent,
                        binding.CreatedAtUtc
                    });
            }

            foreach (AgentProfile profile in await dbContext.AgentProfiles.AsNoTracking()
                         .ToArrayAsync(cancellationToken))
            {
                Add(snapshots, profile.WorkspaceId.Value, nameof(AgentProfile), profile.Id.Value, "upsert",
                    Previous(profile.Version), profile.UpdatedAtUtc, new
                    {
                        profileId = profile.Id.Value,
                        workspaceId = profile.WorkspaceId.Value,
                        vendor = profile.Vendor.Name,
                        profile.Name,
                        profile.SettingsJson,
                        profile.CreatedAtUtc,
                        profile.UpdatedAtUtc,
                        version = profile.Version
                    });
            }

            AgentSession[] sessions = await dbContext.AgentSessions.AsNoTracking().ToArrayAsync(cancellationToken);
            foreach (AgentSession session in sessions)
            {
                Add(snapshots, session.WorkspaceId.Value, nameof(AgentSession), session.Id.Value, "upsert",
                    Previous(session.Version), session.UpdatedAtUtc, new
                    {
                        sessionId = session.Id.Value,
                        workspaceId = session.WorkspaceId.Value,
                        projectId = session.ProjectId.Value,
                        profileId = session.AgentProfileId.Value,
                        status = session.Status.Name,
                        session.CreatedAtUtc,
                        session.UpdatedAtUtc,
                        session.FinishedAtUtc,
                        version = session.Version
                    });
            }

            Dictionary<Guid, Guid> sessionWorkspaces = sessions.ToDictionary(session => session.Id.Value,
                session => session.WorkspaceId.Value);
            foreach (AgentSessionEvent sessionEvent in await dbContext.AgentSessionEvents.AsNoTracking()
                         .ToArrayAsync(cancellationToken))
            {
                if (!sessionWorkspaces.TryGetValue(sessionEvent.AgentSessionId.Value, out Guid workspaceId))
                {
                    continue;
                }

                Add(snapshots, workspaceId, nameof(AgentSessionEvent), sessionEvent.Id.Value, "append", null,
                    sessionEvent.OccurredAtUtc, new
                    {
                        eventId = sessionEvent.Id.Value,
                        sessionId = sessionEvent.AgentSessionId.Value,
                        sessionEvent.Sequence,
                        type = sessionEvent.Type.Name,
                        payloadJson = includeSessionTranscripts ? sessionEvent.PayloadJson : null,
                        sessionEvent.OccurredAtUtc
                    });
            }

            ImportStatusType[] terminalStatuses =
                [ImportStatusType.Succeeded, ImportStatusType.Failed, ImportStatusType.Cancelled];
            foreach (ImportJob import in await dbContext.ImportJobs.AsNoTracking()
                         .Where(import => terminalStatuses.Contains(import.Status))
                         .ToArrayAsync(cancellationToken))
            {
                Add(snapshots, import.WorkspaceId.Value, nameof(ImportJob), import.Id.Value, "append", null,
                    import.CompletedAtUtc ?? import.RequestedAtUtc, new
                    {
                        importJobId = import.Id.Value,
                        workspaceId = import.WorkspaceId.Value,
                        sourceId = import.SourceId.Value,
                        status = import.Status.Name,
                        artifactId = import.ArtifactId?.Value,
                        revisionId = import.ArtifactRevisionId?.Value,
                        chunkBatchId = import.ChunkBatchId?.Value,
                        import.RequestedAtUtc,
                        import.StartedAtUtc,
                        import.CompletedAtUtc,
                        failureCode = import.Failure?.Code
                    });
            }

            foreach (Chunk chunk in await dbContext.Chunks.AsNoTracking().ToArrayAsync(cancellationToken))
            {
                Add(snapshots, chunk.WorkspaceId.Value, nameof(Chunk), chunk.Id.Value, "append", null,
                    chunk.CreatedAtUtc, new
                    {
                        chunkId = chunk.Id.Value,
                        workspaceId = chunk.WorkspaceId.Value,
                        batchId = chunk.BatchId.Value,
                        artifactId = chunk.ArtifactId.Value,
                        revisionId = chunk.ArtifactRevisionId.Value,
                        number = chunk.Number.Value,
                        content = chunk.Content.Value,
                        contentHash = chunk.ContentHash.Value,
                        sourceStart = chunk.SourceSpan?.Start,
                        sourceLength = chunk.SourceSpan?.Length,
                        strategy = chunk.Strategy.Name,
                        strategyVersion = chunk.StrategyVersion.Value,
                        chunk.CreatedAtUtc
                    });
            }

            foreach (ChunkEmbedding embedding in await dbContext.ChunkEmbeddings.AsNoTracking()
                         .ToArrayAsync(cancellationToken))
            {
                float[]? vector = await dbContext.EmbeddingVectors.AsNoTracking()
                    .Where(record => record.ChunkEmbeddingId == embedding.Id)
                    .Select(record => record.Vector.ToArray())
                    .SingleOrDefaultAsync(cancellationToken);
                Add(snapshots, embedding.WorkspaceId.Value, nameof(ChunkEmbedding), embedding.Id.Value, "append", null,
                    embedding.CreatedAtUtc, new
                    {
                        chunkEmbeddingId = embedding.Id.Value,
                        workspaceId = embedding.WorkspaceId.Value,
                        chunkId = embedding.ChunkId.Value,
                        chunkContentHash = embedding.ChunkContentHash.Value,
                        model = embedding.Model.Identifier,
                        modelVersion = embedding.Model.Version,
                        dimensions = embedding.Dimensions.Value,
                        vector,
                        embedding.CreatedAtUtc
                    });
            }

            return snapshots;
        }

        private static object CreateSourceDefinition(SourceDefinition definition)
        {
            return definition switch
            {
                RepositorySourceDefinition repository => new
                {
                    type = "repository",
                    repository.RepositoryIdentity,
                    repository.CanonicalRemoteUri,
                    repository.ScanPolicy
                },
                FileSourceDefinition file => new
                {
                    type = "file",
                    file.Blob,
                    file.FileName,
                    file.MediaType,
                    deviceLocalFile = file.LocalPath is not null
                },
                WebPageSourceDefinition webPage => new { type = "webPage", webPage.Uri },
                PlainTextSourceDefinition plainText => new { type = "plainText", plainText.Title, plainText.Content },
                ConversationSourceDefinition conversation => new
                {
                    type = "conversation",
                    conversation.Title,
                    conversation.Messages
                },
                ConnectorSourceDefinition connector => new
                {
                    type = "connector",
                    connector.PluginId,
                    connector.Version,
                    connector.Resource
                },
                _ => new { type = definition.SourceType.Name }
            };
        }

        private static void Add(List<SemanticSyncSnapshot> snapshots, Guid workspaceId, string entityType,
            Guid entityId, string operation, uint? baseVersion, DateTimeOffset timestamp, object payload)
        {
            string payloadJson = JsonSerializer.Serialize(payload, SerializerOptions);
            snapshots.Add(new SemanticSyncSnapshot(workspaceId, entityType, entityId, operation, baseVersion,
                timestamp, $"{entityType}.v1", payloadJson,
                Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(payloadJson)))));
        }

        private static uint? Previous(uint version)
        {
            return version == 0 ? null : version - 1;
        }
    }
}
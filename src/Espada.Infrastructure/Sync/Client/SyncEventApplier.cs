using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Entities;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Infrastructure.Sync.Options;
using Espada.Protocol.Sync.Contracts;
using Espada.Protocol.Sync.Mappings;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Espada.Infrastructure.Sync.Client
{
    internal sealed class SyncEventApplier(
        IDeviceRepository devices,
        IWorkspaceRepository workspaces,
        IWorkspaceMembershipRepository memberships,
        IProjectRepository projects,
        ISourceRepository sources,
        IArtifactRepository artifacts,
        IArtifactRevisionRepository revisions,
        IChunkBatchRepository chunkBatches,
        IChunkRepository chunks,
        IChunkEmbeddingRepository chunkEmbeddings,
        IEmbeddingVectorStore embeddingVectors,
        IInstructionRuleRepository instructionRules,
        IPolicyRuleRepository policyRules,
        IMemoryMetadataRepository memories,
        IAgentProfileRepository agentProfiles,
        ISyncEventRepository syncEvents,
        IUnitOfWork unitOfWork,
        IOptions<LocalIdentityOptions> localIdentityOptions)
    {
        public async Task<int> ApplyAsync(IReadOnlyList<SyncEnvelope> envelopes,
            CancellationToken cancellationToken)
        {
            int applied = 0;
            foreach (SyncEnvelope envelope in envelopes.OrderBy(item => item.Sequence))
            {
                DomainResult<SyncEvent> mapped = SyncEnvelopeMapper.ToDomain(envelope);
                if (mapped.IsFailure)
                {
                    throw new InvalidDataException(mapped.Error.Description);
                }

                SyncEvent syncEvent = mapped.Value;
                if (await syncEvents.GetByIdAsync(syncEvent.Id, cancellationToken) is not null)
                {
                    continue;
                }

                await EnsureDeviceAsync(syncEvent, cancellationToken);
                await ApplyPayloadAsync(syncEvent, cancellationToken);
                await syncEvents.AddAsync(syncEvent, cancellationToken);
                applied++;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return applied;
        }

        private async Task ApplyPayloadAsync(SyncEvent syncEvent, CancellationToken cancellationToken)
        {
            using JsonDocument document = JsonDocument.Parse(syncEvent.PayloadJson);
            JsonElement payload = document.RootElement;
            switch (syncEvent.EntityType)
            {
                case nameof(Workspace):
                    await ApplyWorkspaceAsync(syncEvent, payload, cancellationToken);
                    break;
                case nameof(Project):
                    await ApplyProjectAsync(payload, cancellationToken);
                    break;
                case nameof(Source):
                    await ApplySourceAsync(payload, cancellationToken);
                    break;
                case nameof(Artifact):
                    await ApplyArtifactAsync(payload, cancellationToken);
                    break;
                case nameof(ArtifactRevision):
                    await ApplyRevisionAsync(payload, cancellationToken);
                    break;
                case nameof(ChunkBatch):
                    await ApplyChunkBatchAsync(payload, cancellationToken);
                    break;
                case nameof(Chunk):
                    await ApplyChunkAsync(payload, cancellationToken);
                    break;
                case nameof(ChunkEmbedding):
                    await ApplyChunkEmbeddingAsync(payload, cancellationToken);
                    break;
                case nameof(AgentProfile):
                    await ApplyAgentProfileAsync(payload, cancellationToken);
                    break;
            }

            if (await workspaces.GetByIdAsync(syncEvent.WorkspaceId, cancellationToken) is null)
            {
                throw new InvalidDataException(
                    $"Sync event '{syncEvent.Id}' references an unavailable workspace.");
            }
        }

        private async Task ApplyWorkspaceAsync(SyncEvent syncEvent, JsonElement payload,
            CancellationToken cancellationToken)
        {
            Workspace? workspace = await workspaces.GetByIdAsync(syncEvent.WorkspaceId, cancellationToken);
            if (workspace is null)
            {
                WorkspaceName name = WorkspaceName.Create(payload.GetProperty("name").GetString()).Value;
                WorkspaceType type = GetEnumeration<WorkspaceType>(payload.GetProperty("type").GetString());
                workspace = Workspace.Create(syncEvent.WorkspaceId, name, type, null,
                    payload.GetProperty("createdAtUtc").GetDateTimeOffset()).Value;
                await workspaces.AddAsync(workspace, cancellationToken);
            }

            LocalIdentityOptions identity = localIdentityOptions.Value;
            if (!await memberships.IsMemberAsync(workspace.Id, identity.LocalIdentityIssuer,
                    identity.LocalIdentitySubject, cancellationToken))
            {
                await memberships.AddAsync(WorkspaceMembership.CreateOwner(WorkspaceMembershipId.New(), workspace.Id,
                    identity.LocalIdentityIssuer, identity.LocalIdentitySubject, syncEvent.OccurredAtUtc),
                    cancellationToken);
            }
        }

        private async Task ApplyProjectAsync(JsonElement payload, CancellationToken cancellationToken)
        {
            ProjectId id = ProjectId.Create(payload.GetProperty("projectId").GetGuid());
            if (await projects.GetByIdAsync(id, cancellationToken) is not null)
            {
                return;
            }

            Project project = Project.Create(id,
                WorkspaceId.Create(payload.GetProperty("workspaceId").GetGuid()),
                payload.GetProperty("name").GetString(),
                payload.TryGetProperty("canonicalRemoteUri", out JsonElement remote)
                    && remote.ValueKind != JsonValueKind.Null ? remote.GetString() : null,
                [],
                payload.GetProperty("createdAtUtc").GetDateTimeOffset()).Value;
            await projects.AddAsync(project, cancellationToken);
        }

        private async Task ApplySourceAsync(JsonElement payload, CancellationToken cancellationToken)
        {
            SourceId id = SourceId.Create(payload.GetProperty("sourceId").GetGuid());
            if (await sources.GetByIdAsync(id, cancellationToken) is not null)
            {
                return;
            }

            SourceDefinition? definition = CreateSourceDefinition(payload.GetProperty("definition"));
            if (definition is null)
            {
                return;
            }

            Source source = Source.Create(id,
                WorkspaceId.Create(payload.GetProperty("workspaceId").GetGuid()),
                SourceName.Create(payload.GetProperty("name").GetString()).Value,
                definition,
                payload.GetProperty("createdAtUtc").GetDateTimeOffset()).Value;
            await sources.AddAsync(source, cancellationToken);
        }

        private async Task ApplyArtifactAsync(JsonElement payload, CancellationToken cancellationToken)
        {
            ArtifactId id = ArtifactId.Create(payload.GetProperty("artifactId").GetGuid());
            if (await artifacts.GetByIdAsync(id, cancellationToken) is not null)
            {
                return;
            }

            WorkspaceId workspaceId = WorkspaceId.Create(payload.GetProperty("workspaceId").GetGuid());
            ArtifactTitle title = ArtifactTitle.Create(payload.GetProperty("title").GetString()).Value;
            ArtifactKindType kind = GetEnumeration<ArtifactKindType>(payload.GetProperty("kind").GetString());
            ArtifactType type = GetEnumeration<ArtifactType>(payload.GetProperty("type").GetString());
            DateTimeOffset createdAtUtc = payload.GetProperty("createdAtUtc").GetDateTimeOffset();
            bool draft = payload.GetProperty("status").GetString() == ArtifactStatusType.Draft.Name;
            Artifact artifact = (draft
                ? Artifact.CreateDraft(id, workspaceId, title, kind, type, createdAtUtc)
                : Artifact.Create(id, workspaceId, title, kind, type, createdAtUtc)).Value;
            if (payload.TryGetProperty("priority", out JsonElement priority))
            {
                artifact.SetPriority(ContextPriority.Create(priority.GetInt32()).Value,
                    payload.GetProperty("updatedAtUtc").GetDateTimeOffset());
            }

            await artifacts.AddAsync(artifact, cancellationToken);
        }

        private async Task ApplyRevisionAsync(JsonElement payload, CancellationToken cancellationToken)
        {
            ArtifactRevisionId id = ArtifactRevisionId.Create(payload.GetProperty("revisionId").GetGuid());
            if (await revisions.GetByIdAsync(id, cancellationToken) is not null)
            {
                return;
            }

            ArtifactId artifactId = ArtifactId.Create(payload.GetProperty("artifactId").GetGuid());
            Artifact artifact = await artifacts.GetByIdAsync(artifactId, cancellationToken)
                                ?? throw new InvalidDataException($"Artifact '{artifactId}' is unavailable.");
            ArtifactRevision revision = artifact.CreateRevision(id,
                ArtifactContent.Create(payload.GetProperty("content").GetString()).Value,
                payload.GetProperty("createdAtUtc").GetDateTimeOffset()).Value;
            if (revision.Number.Value != payload.GetProperty("number").GetInt32()
                || revision.ContentHash.Value != payload.GetProperty("contentHash").GetString())
            {
                throw new InvalidDataException($"Artifact revision '{id}' failed its integrity check.");
            }

            await revisions.AddAsync(revision, cancellationToken);
            await ApplyInstructionRulesAsync(artifact, revision, payload, cancellationToken);
            await ApplyPolicyRulesAsync(artifact, revision, payload, cancellationToken);
            await ApplyMemoriesAsync(artifact, revision, payload, cancellationToken);
        }

        private async Task ApplyChunkBatchAsync(JsonElement payload,
            CancellationToken cancellationToken)
        {
            ChunkBatchId id = ChunkBatchId.Create(payload.GetProperty("chunkBatchId").GetGuid());
            if (await chunkBatches.GetByIdAsync(id, cancellationToken) is not null)
            {
                return;
            }

            if (payload.GetProperty("status").GetString() != ChunkBatchStatusType.Succeeded.Name)
            {
                return;
            }

            ChunkBatch batch = ChunkBatch.Request(id,
                WorkspaceId.Create(payload.GetProperty("workspaceId").GetGuid()),
                ArtifactId.Create(payload.GetProperty("artifactId").GetGuid()),
                ArtifactRevisionId.Create(payload.GetProperty("revisionId").GetGuid()),
                GetEnumeration<ChunkingStrategyType>(payload.GetProperty("strategy").GetString()),
                ChunkingVersion.Create(payload.GetProperty("strategyVersion").GetString()).Value,
                payload.GetProperty("requestedAtUtc").GetDateTimeOffset()).Value;
            DateTimeOffset startedAtUtc = payload.GetProperty("startedAtUtc").GetDateTimeOffset();
            DateTimeOffset completedAtUtc = payload.GetProperty("completedAtUtc").GetDateTimeOffset();
            batch.Start(startedAtUtc);
            batch.Complete(payload.GetProperty("chunkCount").GetInt32(), completedAtUtc);
            await chunkBatches.AddAsync(batch, cancellationToken);
        }

        private async Task ApplyChunkAsync(JsonElement payload, CancellationToken cancellationToken)
        {
            ChunkId id = ChunkId.Create(payload.GetProperty("chunkId").GetGuid());
            Chunk? existing = await chunks.GetByIdAsync(id, cancellationToken);
            string contentHash = payload.GetProperty("contentHash").GetString() ?? string.Empty;
            if (existing is not null)
            {
                if (existing.ContentHash.Value != contentHash)
                {
                    throw new InvalidDataException($"Chunk '{id}' has conflicting content.");
                }

                return;
            }

            JsonElement sourceStart = payload.GetProperty("sourceStart");
            JsonElement sourceLength = payload.GetProperty("sourceLength");
            SourceTextSpan? sourceSpan = sourceStart.ValueKind == JsonValueKind.Null
                || sourceLength.ValueKind == JsonValueKind.Null
                    ? null
                    : SourceTextSpan.Create(sourceStart.GetInt32(), sourceLength.GetInt32()).Value;
            Chunk chunk = Chunk.Create(id,
                ChunkBatchId.Create(payload.GetProperty("batchId").GetGuid()),
                WorkspaceId.Create(payload.GetProperty("workspaceId").GetGuid()),
                ArtifactId.Create(payload.GetProperty("artifactId").GetGuid()),
                ArtifactRevisionId.Create(payload.GetProperty("revisionId").GetGuid()),
                ChunkNumber.Create(payload.GetProperty("number").GetInt32()).Value,
                ChunkContent.Create(payload.GetProperty("content").GetString()).Value,
                sourceSpan,
                GetEnumeration<ChunkingStrategyType>(payload.GetProperty("strategy").GetString()),
                ChunkingVersion.Create(payload.GetProperty("strategyVersion").GetString()).Value,
                payload.GetProperty("createdAtUtc").GetDateTimeOffset()).Value;
            if (chunk.ContentHash.Value != contentHash)
            {
                throw new InvalidDataException($"Chunk '{id}' failed its integrity check.");
            }

            await chunks.AddRangeAsync([chunk], cancellationToken);
        }

        private async Task ApplyChunkEmbeddingAsync(JsonElement payload,
            CancellationToken cancellationToken)
        {
            JsonElement vectorElement = payload.GetProperty("vector");
            if (vectorElement.ValueKind == JsonValueKind.Null)
            {
                return;
            }

            ChunkId chunkId = ChunkId.Create(payload.GetProperty("chunkId").GetGuid());
            Chunk? chunk = await chunks.GetByIdAsync(chunkId, cancellationToken);
            string chunkContentHash = payload.GetProperty("chunkContentHash").GetString() ?? string.Empty;
            if (chunk is null || chunk.ContentHash.Value != chunkContentHash)
            {
                return;
            }

            EmbeddingModel model = EmbeddingModel.Create(
                payload.GetProperty("model").GetString(),
                payload.GetProperty("modelVersion").GetString()).Value;
            float[] vector = vectorElement.EnumerateArray().Select(item => item.GetSingle()).ToArray();
            EmbeddingDimensions dimensions = EmbeddingDimensions.Create(
                payload.GetProperty("dimensions").GetInt32()).Value;
            if (vector.Length != dimensions.Value)
            {
                return;
            }

            ChunkEmbedding? existing = await chunkEmbeddings.GetByChunkIdAsync(
                chunkId, model, cancellationToken);
            if (existing is not null)
            {
                return;
            }

            ChunkEmbeddingId id = ChunkEmbeddingId.Create(
                payload.GetProperty("chunkEmbeddingId").GetGuid());
            ChunkEmbedding embedding = ChunkEmbedding.Create(id,
                WorkspaceId.Create(payload.GetProperty("workspaceId").GetGuid()),
                chunkId,
                ContentHash.Create(chunkContentHash),
                model,
                dimensions,
                payload.GetProperty("createdAtUtc").GetDateTimeOffset()).Value;
            await chunkEmbeddings.AddAsync(embedding, cancellationToken);
            await embeddingVectors.UpsertAsync(id, vector, cancellationToken);
        }

        private async Task ApplyInstructionRulesAsync(Artifact artifact, ArtifactRevision revision,
            JsonElement payload, CancellationToken cancellationToken)
        {
            if (!payload.TryGetProperty("instructionRules", out JsonElement items))
            {
                return;
            }

            List<InstructionRule> created = [];
            foreach (JsonElement item in items.EnumerateArray())
            {
                created.Add(artifact.CreateInstructionRule(revision,
                    RuleKey.Create(item.GetProperty("key").GetString()).Value,
                    item.GetProperty("text").GetString(),
                    ContextPriority.Create(item.GetProperty("priority").GetInt32()).Value).Value);
            }

            if (created.Count > 0)
            {
                await instructionRules.AddRangeAsync(created, cancellationToken);
            }
        }

        private async Task ApplyPolicyRulesAsync(Artifact artifact, ArtifactRevision revision,
            JsonElement payload, CancellationToken cancellationToken)
        {
            if (!payload.TryGetProperty("policyRules", out JsonElement items))
            {
                return;
            }

            List<PolicyRule> created = [];
            foreach (JsonElement item in items.EnumerateArray())
            {
                created.Add(artifact.CreatePolicyRule(revision,
                    RuleKey.Create(item.GetProperty("key").GetString()).Value,
                    item.GetProperty("text").GetString(),
                    ContextPriority.Create(item.GetProperty("priority").GetInt32()).Value,
                    GetEnumeration<PolicyEnforcementType>(item.GetProperty("enforcement").GetString())).Value);
            }

            if (created.Count > 0)
            {
                await policyRules.AddRangeAsync(created, cancellationToken);
            }
        }

        private async Task ApplyMemoriesAsync(Artifact artifact, ArtifactRevision revision, JsonElement payload,
            CancellationToken cancellationToken)
        {
            if (!payload.TryGetProperty("memories", out JsonElement items))
            {
                return;
            }

            foreach (JsonElement item in items.EnumerateArray())
            {
                MemoryId id = MemoryId.Create(item.GetProperty("memoryId").GetGuid());
                if (await memories.GetByIdAsync(id, cancellationToken) is not null)
                {
                    continue;
                }

                MemoryId? superseded = item.TryGetProperty("supersededMemoryId", out JsonElement supersededElement)
                                       && supersededElement.ValueKind != JsonValueKind.Null
                    ? MemoryId.Create(supersededElement.GetGuid())
                    : null;
                MemoryMetadata metadata = artifact.CreateMemoryMetadata(id, revision,
                    GetEnumeration<MemoryCategoryType>(item.GetProperty("category").GetString()),
                    item.GetProperty("confidence").GetDecimal(),
                    item.GetProperty("userConfirmed").GetBoolean(),
                    item.GetProperty("clientIdentity").GetString(),
                    item.GetProperty("sessionIdentity").GetString(),
                    item.GetProperty("capturedAtUtc").GetDateTimeOffset(),
                    superseded).Value;
                await memories.AddAsync(metadata, cancellationToken);
            }
        }

        private async Task ApplyAgentProfileAsync(JsonElement payload, CancellationToken cancellationToken)
        {
            AgentProfileId id = AgentProfileId.Create(payload.GetProperty("profileId").GetGuid());
            if (await agentProfiles.GetByIdAsync(id, cancellationToken) is not null)
            {
                return;
            }

            AgentProfile profile = AgentProfile.Create(id,
                WorkspaceId.Create(payload.GetProperty("workspaceId").GetGuid()),
                GetEnumeration<AgentVendorType>(payload.GetProperty("vendor").GetString()),
                payload.GetProperty("name").GetString(),
                payload.GetProperty("settingsJson").GetString(),
                payload.GetProperty("createdAtUtc").GetDateTimeOffset()).Value;
            await agentProfiles.AddAsync(profile, cancellationToken);
        }

        private async Task EnsureDeviceAsync(SyncEvent syncEvent, CancellationToken cancellationToken)
        {
            if (await devices.GetByIdAsync(syncEvent.DeviceId, cancellationToken) is null)
            {
                await devices.AddAsync(Device.Create(syncEvent.DeviceId, "Synced device",
                    syncEvent.OccurredAtUtc).Value, cancellationToken);
            }
        }

        private static T GetEnumeration<T>(string? name) where T : Enumeration
        {
            return Enumeration.GetAll<T>().SingleOrDefault(item => item.Name == name)
                   ?? throw new InvalidDataException($"Unknown {typeof(T).Name} value '{name}'.");
        }

        private static SourceDefinition? CreateSourceDefinition(JsonElement definition)
        {
            string type = definition.GetProperty("type").GetString() ?? string.Empty;
            return type switch
            {
                "repository" => new RepositorySourceDefinition(
                    definition.GetProperty("repositoryIdentity").GetString()!,
                    definition.TryGetProperty("canonicalRemoteUri", out JsonElement remote)
                    && remote.ValueKind != JsonValueKind.Null ? remote.GetString() : null,
                    new RepositoryScanPolicy(definition.GetProperty("scanPolicy")
                        .GetProperty("maximumFileSizeBytes").GetInt64())),
                "webPage" => new WebPageSourceDefinition(
                    new Uri(definition.GetProperty("uri").GetString()!, UriKind.Absolute)),
                "plainText" => new PlainTextSourceDefinition(
                    definition.GetProperty("title").GetString()!,
                    definition.GetProperty("content").GetString()!),
                "file" when definition.TryGetProperty("blob", out JsonElement blob)
                            && blob.ValueKind != JsonValueKind.Null => new FileSourceDefinition(null,
                    JsonSerializer.Deserialize<BlobSourceReference>(blob.GetRawText())!,
                    definition.GetProperty("fileName").GetString()!,
                    definition.GetProperty("mediaType").GetString()!),
                _ => null
            };
        }
    }
}
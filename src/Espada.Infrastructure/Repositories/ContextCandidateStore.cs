using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class ContextCandidateStore(
        EspadaDbContext dbContext) : IContextCandidateStore
    {
        public async Task<IReadOnlyList<ContextCandidateRecord>>
            LoadByWorkspaceIdAsync(
                WorkspaceId workspaceId,
                CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);

            Binding[] bindings = await dbContext.Bindings
                .AsNoTracking()
                .Where(binding => binding.WorkspaceId == workspaceId)
                .OrderBy(binding => binding.ArtifactRevisionId)
                .ThenBy(binding => binding.Id)
                .ToArrayAsync(cancellationToken);
            if (bindings.Length == 0)
            {
                return [];
            }

            ArtifactRevisionId[] revisionIds = bindings
                .Select(binding => binding.ArtifactRevisionId)
                .Distinct()
                .ToArray();
            ArtifactRevision[] revisions = await dbContext.ArtifactRevisions
                .AsNoTracking()
                .Where(revision => revision.WorkspaceId == workspaceId
                                   && revisionIds.Contains(revision.Id))
                .ToArrayAsync(cancellationToken);
            ArtifactId[] artifactIds = revisions
                .Select(revision => revision.ArtifactId)
                .Distinct()
                .ToArray();
            Artifact[] artifacts = await dbContext.Artifacts
                .AsNoTracking()
                .Where(artifact => artifact.WorkspaceId == workspaceId
                                   && artifactIds.Contains(artifact.Id))
                .ToArrayAsync(cancellationToken);
            InstructionRule[] instructionRules = await dbContext
                .InstructionRules
                .AsNoTracking()
                .Where(rule => revisionIds.Contains(rule.ArtifactRevisionId))
                .OrderBy(rule => rule.RuleKey)
                .ToArrayAsync(cancellationToken);
            PolicyRule[] policyRules = await dbContext.PolicyRules
                .AsNoTracking()
                .Where(rule => revisionIds.Contains(rule.ArtifactRevisionId))
                .OrderBy(rule => rule.RuleKey)
                .ToArrayAsync(cancellationToken);
            MemoryMetadata[] memories = await dbContext.MemoryMetadata
                .AsNoTracking()
                .Where(metadata => revisionIds.Contains(
                    metadata.ArtifactRevisionId))
                .ToArrayAsync(cancellationToken);
            MemoryId[] memoryIds = memories
                .Select(metadata => metadata.Id)
                .ToArray();
            MemoryId?[] supersededIds = memoryIds.Length == 0
                ? []
                : await dbContext.MemoryMetadata
                    .AsNoTracking()
                    .Where(metadata => metadata.SupersededMemoryId != null
                                       && memoryIds.Contains(
                                           metadata.SupersededMemoryId))
                    .Select(metadata => metadata.SupersededMemoryId)
                    .ToArrayAsync(cancellationToken);

            IReadOnlyDictionary<ArtifactRevisionId, ArtifactRevision>
                revisionsById = revisions.ToDictionary(revision => revision.Id);
            IReadOnlyDictionary<ArtifactId, Artifact> artifactsById =
                artifacts.ToDictionary(artifact => artifact.Id);
            ILookup<ArtifactRevisionId, InstructionRule> instructionRulesByRevision =
                instructionRules.ToLookup(rule => rule.ArtifactRevisionId);
            ILookup<ArtifactRevisionId, PolicyRule> policyRulesByRevision =
                policyRules.ToLookup(rule => rule.ArtifactRevisionId);
            IReadOnlyDictionary<ArtifactRevisionId, MemoryMetadata>
                memoriesByRevision = memories.ToDictionary(metadata => metadata.ArtifactRevisionId);
            HashSet<MemoryId> superseded = supersededIds
                .OfType<MemoryId>()
                .ToHashSet();
            List<ContextCandidateRecord> candidates = new(bindings.Length);

            foreach (Binding binding in bindings)
            {
                ArtifactRevision revision = revisionsById[binding.ArtifactRevisionId];
                Artifact artifact = artifactsById[revision.ArtifactId];
                memoriesByRevision.TryGetValue(
                    revision.Id,
                    out MemoryMetadata? memory);
                candidates.Add(new ContextCandidateRecord(
                    binding,
                    artifact,
                    revision,
                    instructionRulesByRevision[revision.Id].ToArray(),
                    policyRulesByRevision[revision.Id].ToArray(),
                    memory,
                    memory is not null && superseded.Contains(memory.Id)));
            }

            return candidates;
        }
    }
}
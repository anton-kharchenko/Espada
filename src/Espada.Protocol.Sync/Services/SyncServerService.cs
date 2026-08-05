using Espada.Application.Contracts.Persistence;
using Espada.Application.Models.Sync;
using Espada.Domain.Aggregates;
using Espada.Domain.Entities;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Espada.Protocol.Sync.Contracts;
using Espada.Protocol.Sync.Mappings;
using Espada.Protocol.Sync.Models;
using Espada.Protocol.Sync.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Espada.Protocol.Sync.Services
{
    public sealed class SyncServerService(
        ISyncDeviceRegistrationRepository deviceRegistrations,
        ISyncEventRepository syncEvents,
        ISyncConflictRepository conflicts,
        IWorkspaceRepository workspaces,
        IWorkspaceMembershipRepository memberships,
        IUnitOfWork unitOfWork,
        IOptions<SyncServerOptions> options)
    {
        private static readonly HashSet<string> MutableEntityTypes =
        [
            nameof(Workspace),
            nameof(Project),
            nameof(ProjectTask),
            nameof(Source),
            nameof(Artifact),
            nameof(AgentProfile),
            nameof(AgentSession)
        ];

        public async Task<RegisterDeviceResponse?> RegisterDeviceAsync(RegisterDeviceRequest request, string issuer,
            string subject, DateTimeOffset registeredAtUtc, CancellationToken cancellationToken)
        {
            DeviceId deviceId = DeviceId.Create(request.DeviceId);
            if (await deviceRegistrations.IsOwnedByAsync(deviceId, issuer, subject, cancellationToken))
            {
                return new RegisterDeviceResponse(request.DeviceId, registeredAtUtc);
            }

            if (await deviceRegistrations.CountByOwnerAsync(issuer, subject, cancellationToken)
                >= options.Value.MaxDevices)
            {
                return null;
            }

            await deviceRegistrations.RegisterAsync(deviceId, request.Name, issuer, subject, registeredAtUtc,
                cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new RegisterDeviceResponse(request.DeviceId, registeredAtUtc);
        }

        public async Task<SyncPushOutcome> PushAsync(SyncPushRequest request, string issuer, string subject,
            CancellationToken cancellationToken)
        {
            SyncServerOptions limits = options.Value;
            if (request.Events.Count > limits.MaxPushEvents
                || request.Events.Sum(item => Encoding.UTF8.GetByteCount(item.Payload.GetRawText()))
                > limits.MaxStorageBytes)
            {
                return new SyncPushOutcome(StatusCodes.Status413PayloadTooLarge, null,
                    "The sync batch exceeds the configured storage or event limit.");
            }

            DeviceId deviceId = DeviceId.Create(request.DeviceId);
            if (!await deviceRegistrations.IsOwnedByAsync(deviceId, issuer, subject, cancellationToken))
            {
                return new SyncPushOutcome(StatusCodes.Status403Forbidden, null,
                    "The sync device is not registered to this identity.");
            }

            HashSet<Guid> newWorkspaceIds = [];
            Dictionary<(Guid WorkspaceId, string Type, Guid EntityId), SyncEvent> latestByEntity = [];
            List<Guid> conflictIds = [];
            long acceptedThrough = 0;
            foreach (SyncEnvelope envelope in request.Events.OrderBy(item => item.Sequence))
            {
                if (envelope.DeviceId != request.DeviceId)
                {
                    return new SyncPushOutcome(StatusCodes.Status400BadRequest, null,
                        "Every sync envelope must belong to the registered device.");
                }

                DomainResult<SyncEvent> mapped = SyncEnvelopeMapper.ToDomain(envelope);
                if (mapped.IsFailure)
                {
                    return new SyncPushOutcome(StatusCodes.Status400BadRequest, null, mapped.Error.Description);
                }

                SyncEvent incoming = mapped.Value;
                if (!await EnsureWorkspaceAccessAsync(incoming, issuer, subject, newWorkspaceIds, cancellationToken))
                {
                    return new SyncPushOutcome(StatusCodes.Status403Forbidden, null,
                        "Cross-tenant workspace synchronization is forbidden.");
                }

                SyncEvent? duplicate = await syncEvents.GetByIdAsync(incoming.Id, cancellationToken);
                if (duplicate is not null)
                {
                    if (duplicate.DeviceId != incoming.DeviceId || duplicate.Sequence != incoming.Sequence
                        || duplicate.PayloadHash != incoming.PayloadHash)
                    {
                        return new SyncPushOutcome(StatusCodes.Status409Conflict, null,
                            "The event identifier is already associated with different content.");
                    }

                    acceptedThrough = Math.Max(acceptedThrough, incoming.Sequence);
                    continue;
                }

                SyncEvent? sequenceDuplicate = await syncEvents.GetByDeviceSequenceAsync(incoming.DeviceId,
                    incoming.Sequence, cancellationToken);
                if (sequenceDuplicate is not null)
                {
                    return new SyncPushOutcome(StatusCodes.Status409Conflict, null,
                        "The device sequence is already associated with another event.");
                }

                (Guid WorkspaceId, string Type, Guid EntityId) key =
                    (incoming.WorkspaceId.Value, incoming.EntityType, incoming.EntityId);
                if (!latestByEntity.TryGetValue(key, out SyncEvent? latest))
                {
                    latest = await syncEvents.GetLatestEntityEventAsync(incoming.WorkspaceId, incoming.EntityType,
                        incoming.EntityId, cancellationToken);
                }

                if (latest is not null && MutableEntityTypes.Contains(incoming.EntityType)
                                       && incoming.BaseVersion != NextVersion(latest))
                {
                    SyncConflict conflict = SyncConflict.Create(SyncConflictId.New(), incoming.WorkspaceId,
                        incoming.EntityType, incoming.EntityId, latest.Id, incoming.Id,
                        JsonSerializer.Serialize(new
                        {
                            expectedBaseVersion = NextVersion(latest),
                            incoming.BaseVersion,
                            latestEventId = latest.Id.Value,
                            incomingEventId = incoming.Id.Value
                        }), DateTimeOffset.UtcNow).Value;
                    await conflicts.AddAsync(conflict, cancellationToken);
                    conflictIds.Add(conflict.Id.Value);
                }

                await syncEvents.AddAsync(incoming, cancellationToken);
                latestByEntity[key] = incoming;
                acceptedThrough = Math.Max(acceptedThrough, incoming.Sequence);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new SyncPushOutcome(StatusCodes.Status200OK,
                new SyncPushResponse(acceptedThrough, conflictIds), null);
        }

        public async Task<SyncPullOutcome> PullAsync(Guid deviceIdValue, string? cursor, string issuer, string subject,
            CancellationToken cancellationToken)
        {
            DeviceId deviceId = DeviceId.Create(deviceIdValue);
            if (!await deviceRegistrations.IsOwnedByAsync(deviceId, issuer, subject, cancellationToken))
            {
                return new SyncPullOutcome(StatusCodes.Status403Forbidden, null,
                    "The sync device is not registered to this identity.");
            }

            if (!long.TryParse(cursor ?? "0", out long serverSequence) || serverSequence < 0)
            {
                return new SyncPullOutcome(StatusCodes.Status400BadRequest, null, "The sync cursor is invalid.");
            }

            IReadOnlyList<Workspace> allowed = await memberships.ListWorkspacesAsync(issuer, subject,
                cancellationToken);
            IReadOnlyList<StoredSyncEvent> stored =
                await syncEvents.ListAfterServerSequenceAsync(allowed.Select(workspace => workspace.Id).ToArray(),
                    serverSequence, 500, cancellationToken);
            List<SyncEnvelope> envelopes = [];
            long bytes = 0;
            long nextCursor = serverSequence;
            foreach (StoredSyncEvent item in stored)
            {
                SyncEnvelope envelope = SyncEnvelopeMapper.ToEnvelope(item.Event);
                long eventBytes = Encoding.UTF8.GetByteCount(envelope.Payload.GetRawText());
                if (bytes + eventBytes > options.Value.MaxEgressBytes)
                {
                    break;
                }

                envelopes.Add(envelope);
                bytes += eventBytes;
                nextCursor = item.ServerSequence;
            }

            return new SyncPullOutcome(StatusCodes.Status200OK,
                new SyncPullResponse(nextCursor.ToString(), envelopes), null);
        }

        private async Task<bool> EnsureWorkspaceAccessAsync(SyncEvent incoming, string issuer, string subject,
            HashSet<Guid> newWorkspaceIds, CancellationToken cancellationToken)
        {
            if (newWorkspaceIds.Contains(incoming.WorkspaceId.Value)
                || await memberships.IsMemberAsync(incoming.WorkspaceId, issuer, subject, cancellationToken))
            {
                return true;
            }

            Workspace? existing = await workspaces.GetByIdAsync(incoming.WorkspaceId, cancellationToken);
            if (existing is not null || incoming.EntityType != nameof(Workspace)
                                     || incoming.EntityId != incoming.WorkspaceId.Value
                                     || incoming.BaseVersion is not null)
            {
                return false;
            }

            using JsonDocument payload = JsonDocument.Parse(incoming.PayloadJson);
            if (!payload.RootElement.TryGetProperty("name", out JsonElement nameElement)
                || !payload.RootElement.TryGetProperty("type", out JsonElement typeElement))
            {
                return false;
            }

            DomainResult<WorkspaceName> name = WorkspaceName.Create(nameElement.GetString());
            WorkspaceType? type = Enumeration.GetAll<WorkspaceType>()
                .SingleOrDefault(item => item.Name == typeElement.GetString());
            if (name.IsFailure || type is null)
            {
                return false;
            }

            Workspace workspace = Workspace.Create(incoming.WorkspaceId, name.Value, type, null,
                incoming.OccurredAtUtc).Value;
            await workspaces.AddAsync(workspace, cancellationToken);
            await memberships.AddAsync(WorkspaceMembership.CreateOwner(WorkspaceMembershipId.New(), workspace.Id,
                issuer, subject, incoming.OccurredAtUtc), cancellationToken);
            newWorkspaceIds.Add(workspace.Id.Value);
            return true;
        }

        private static uint? NextVersion(SyncEvent syncEvent)
        {
            return syncEvent.BaseVersion is null ? 0 : syncEvent.BaseVersion + 1;
        }
    }
}
using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Entities;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Devices;
using Espada.Infrastructure.Sync.Authentication;
using Espada.Infrastructure.Sync.Contracts;
using Espada.Infrastructure.Sync.Options;
using Espada.Protocol.Sync.Contracts;
using Espada.Protocol.Sync.Mappings;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Espada.Infrastructure.Sync.Client
{
    internal sealed class SyncClientService(
        IHttpClientFactory httpClientFactory,
        ISyncAuthorizationService authorization,
        LocalDeviceIdentityStore deviceIdentity,
        LocalSyncStateStore stateStore,
        SyncEventApplier eventApplier,
        ISyncEventRepository syncEvents,
        IUnitOfWork unitOfWork,
        IOptions<SyncClientOptions> options) : ISyncClientService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        public bool IsConfigured => options.Value.IsConfigured();

        public async Task<SyncCycleResponse> RunAsync(CancellationToken cancellationToken)
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException("Espada Cloud sync is not configured.");
            }

            string? accessToken = await authorization.GetAccessTokenAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new UnauthorizedAccessException("Run 'espada login' before synchronizing.");
            }

            Guid deviceIdValue = deviceIdentity.GetOrCreate();
            DeviceId deviceId = DeviceId.Create(deviceIdValue);
            using HttpClient client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.ServerUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            await SendAsync(client, HttpMethod.Post, "sync/v1/devices",
                new RegisterDeviceRequest(deviceIdValue, Environment.MachineName), cancellationToken);

            await syncEvents.MaterializeLocalStateAsync(deviceId, options.Value.IncludeSessionTranscripts,
                cancellationToken);
            IReadOnlyList<SyncEvent> pending = await syncEvents.ListPendingAsync(deviceId, cancellationToken);
            List<Guid> conflictIds = [];
            int pushed = 0;
            foreach (SyncEvent[] batch in pending.Chunk(options.Value.MaxPushEvents))
            {
                SyncPushRequest request = new(deviceIdValue,
                    batch.Select(SyncEnvelopeMapper.ToEnvelope).ToArray());
                SyncPushResponse response = await SendAsync<SyncPushRequest, SyncPushResponse>(client,
                    HttpMethod.Post, "sync/v1/push", request, cancellationToken);
                conflictIds.AddRange(response.ConflictIds);
                foreach (IGrouping<WorkspaceId, SyncEvent> workspaceEvents in batch.GroupBy(item => item.WorkspaceId))
                {
                    SyncCursor cursor = await syncEvents.GetOrCreateCursorAsync(deviceId, workspaceEvents.Key,
                        cancellationToken);
                    cursor.AdvancePush(workspaceEvents.Max(item => item.Sequence), DateTimeOffset.UtcNow);
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
                pushed += batch.Length;
            }

            LocalSyncState state = await stateStore.ReadAsync(cancellationToken);
            string pullPath = QueryHelpers.AddQueryString("sync/v1/pull", new Dictionary<string, string?>
            {
                ["deviceId"] = deviceIdValue.ToString("D"),
                ["cursor"] = state.Cursor
            });
            SyncPullResponse pull = await SendAsync<object, SyncPullResponse>(client, HttpMethod.Get, pullPath, null,
                cancellationToken);
            await eventApplier.ApplyAsync(pull.Events, cancellationToken);
            await stateStore.WriteAsync(new LocalSyncState(pull.Cursor), cancellationToken);
            return new SyncCycleResponse(pushed, pull.Events.Count, conflictIds, pull.Cursor);
        }

        private static async Task SendAsync<TRequest>(HttpClient client, HttpMethod method, string path,
            TRequest request, CancellationToken cancellationToken)
        {
            await SendAsync<TRequest, JsonElement>(client, method, path, request, cancellationToken);
        }

        private static async Task<TResponse> SendAsync<TRequest, TResponse>(HttpClient client, HttpMethod method,
            string path, TRequest? request, CancellationToken cancellationToken)
        {
            using HttpRequestMessage message = new(method, path);
            if (request is not null)
            {
                message.Content = JsonContent.Create(request, options: SerializerOptions);
            }

            using HttpResponseMessage response = await client.SendAsync(message, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"Espada Cloud sync failed with status {(int)response.StatusCode}: {body}");
            }

            if (typeof(TResponse) == typeof(JsonElement)
                && response.Content.Headers.ContentLength is 0)
            {
                return default!;
            }

            return await response.Content.ReadFromJsonAsync<TResponse>(SerializerOptions, cancellationToken)
                   ?? throw new InvalidDataException("Espada Cloud returned an empty sync response.");
        }
    }
}
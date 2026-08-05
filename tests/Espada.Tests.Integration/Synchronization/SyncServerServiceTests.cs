using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Protocol.Sync;
using Espada.Protocol.Sync.Contracts;
using Espada.Protocol.Sync.Models;
using Espada.Protocol.Sync.Options;
using Espada.Protocol.Sync.Services;
using Espada.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Espada.Tests.Integration.Synchronization
{
    [Collection(PostgreSqlIntegrationCollection.Name)]
    public sealed class SyncServerServiceTests(PostgreSqlDatabaseFixture fixture) : PostgreSqlIntegrationTest(fixture)
    {
        [Fact]
        public async Task PushAndPull_ShouldSupportTwoDevicesIdempotencyConflictsAndTenantIsolation()
        {
            await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
            using IServiceScope scope = serviceProvider.CreateScope();
            SyncServerService service = CreateService(scope.ServiceProvider);
            CancellationToken cancellationToken = TestContext.Current.CancellationToken;
            const string issuer = "https://identity.example";
            const string subject = "owner";
            Guid deviceA = Guid.NewGuid();
            Guid deviceB = Guid.NewGuid();
            Guid outsiderDevice = Guid.NewGuid();

            Assert.NotNull(await service.RegisterDeviceAsync(new RegisterDeviceRequest(deviceA, "Device A"), issuer,
                subject, DateTimeOffset.UtcNow, cancellationToken));
            Assert.NotNull(await service.RegisterDeviceAsync(new RegisterDeviceRequest(deviceB, "Device B"), issuer,
                subject, DateTimeOffset.UtcNow, cancellationToken));
            Assert.NotNull(await service.RegisterDeviceAsync(
                new RegisterDeviceRequest(outsiderDevice, "Outsider"), issuer, "outsider", DateTimeOffset.UtcNow,
                cancellationToken));

            Guid workspaceId = Guid.NewGuid();
            SyncEnvelope workspaceEvent = CreateEnvelope(deviceA, 1, workspaceId, nameof(Workspace), workspaceId,
                null, """{"name":"Two-device workspace","type":"Personal"}""");
            SyncPushRequest initialPush = new(deviceA, [workspaceEvent]);

            SyncPushOutcome first = await service.PushAsync(initialPush, issuer, subject, cancellationToken);
            SyncPushOutcome duplicate = await service.PushAsync(initialPush, issuer, subject, cancellationToken);
            SyncPullOutcome pulled = await service.PullAsync(deviceB, "0", issuer, subject, cancellationToken);

            Assert.Equal(StatusCodes.Status200OK, first.StatusCode);
            Assert.Empty(Assert.IsType<SyncPushResponse>(first.Response).ConflictIds);
            Assert.Equal(StatusCodes.Status200OK, duplicate.StatusCode);
            Assert.Equal(1, Assert.IsType<SyncPushResponse>(duplicate.Response).AcceptedThroughSequence);
            Assert.Equal(StatusCodes.Status200OK, pulled.StatusCode);
            Assert.Equal(workspaceEvent.EventId,
                Assert.Single(Assert.IsType<SyncPullResponse>(pulled.Response).Events).EventId);

            Guid projectId = Guid.NewGuid();
            SyncEnvelope deviceAProject = CreateEnvelope(deviceA, 2, workspaceId, nameof(Project), projectId,
                null, """{"name":"Project from A"}""");
            SyncEnvelope deviceBProject = CreateEnvelope(deviceB, 1, workspaceId, nameof(Project), projectId,
                null, """{"name":"Project from B"}""");
            SyncPushOutcome deviceAPush = await service.PushAsync(new SyncPushRequest(deviceA, [deviceAProject]), issuer,
                subject, cancellationToken);
            SyncPushOutcome deviceBPush = await service.PushAsync(new SyncPushRequest(deviceB, [deviceBProject]), issuer,
                subject, cancellationToken);

            Assert.Equal(StatusCodes.Status200OK, deviceAPush.StatusCode);
            Assert.Empty(Assert.IsType<SyncPushResponse>(deviceAPush.Response).ConflictIds);
            Assert.Equal(StatusCodes.Status200OK, deviceBPush.StatusCode);
            Assert.Single(Assert.IsType<SyncPushResponse>(deviceBPush.Response).ConflictIds);

            SyncEnvelope outsiderEvent = CreateEnvelope(outsiderDevice, 1, workspaceId, nameof(Project),
                Guid.NewGuid(), null, """{"name":"Forbidden project"}""");
            SyncPushOutcome forbidden = await service.PushAsync(new SyncPushRequest(outsiderDevice, [outsiderEvent]), issuer,
                "outsider", cancellationToken);

            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
            Assert.Null(forbidden.Response);
        }

        private static SyncServerService CreateService(IServiceProvider services)
        {
            return new SyncServerService(
                services.GetRequiredService<ISyncDeviceRegistrationRepository>(),
                services.GetRequiredService<ISyncEventRepository>(),
                services.GetRequiredService<ISyncConflictRepository>(),
                services.GetRequiredService<IWorkspaceRepository>(),
                services.GetRequiredService<IWorkspaceMembershipRepository>(),
                services.GetRequiredService<IUnitOfWork>(),
                Options.Create(new SyncServerOptions
                {
                    Enabled = true,
                    MaxDevices = 5,
                    MaxPushEvents = 100,
                    MaxStorageBytes = 1_000_000,
                    MaxEgressBytes = 1_000_000
                }));
        }

        private static SyncEnvelope CreateEnvelope(Guid deviceId, long sequence, Guid workspaceId,
            string entityType, Guid entityId, uint? baseVersion, string payloadJson)
        {
            using JsonDocument payload = JsonDocument.Parse(payloadJson);
            JsonElement element = payload.RootElement.Clone();
            string payloadHash = Convert.ToHexStringLower(
                SHA256.HashData(Encoding.UTF8.GetBytes(element.GetRawText())));
            return new SyncEnvelope(SyncProtocolConstants.Version, Guid.NewGuid(), deviceId, sequence, workspaceId,
                entityType, entityId, "upsert", baseVersion, DateTimeOffset.UtcNow, payloadHash, $"{entityType}.v1",
                element);
        }
    }
}
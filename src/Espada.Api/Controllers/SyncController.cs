using Espada.Application.Contracts.Blobs;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Domain.ValueObjects;
using Espada.Protocol.Sync.Contracts;
using Espada.Protocol.Sync.Models;
using Espada.Protocol.Sync.Options;
using Espada.Protocol.Sync.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Espada.Api.Controllers
{
    [ApiController]
    [Route("sync/v1")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public sealed class SyncController(
        SyncServerService syncServer,
        IBlobStoreService blobStore,
        IWorkspaceMembershipRepository memberships,
        IOptions<SyncServerOptions> options) : ControllerBase
    {
        [HttpPost("devices")]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequest request,
            CancellationToken cancellationToken)
        {
            if (!options.Value.Enabled)
            {
                return NotFound();
            }

            if (!TryGetIdentity(out string issuer, out string subject))
            {
                return Unauthorized();
            }

            RegisterDeviceResponse? response = await syncServer.RegisterDeviceAsync(request, issuer, subject,
                DateTimeOffset.UtcNow, cancellationToken);
            return response is null
                ? StatusCode(StatusCodes.Status429TooManyRequests,
                    new { message = "The configured device limit has been reached." })
                : Ok(response);
        }

        [HttpPost("push")]
        public async Task<IActionResult> Push([FromBody] SyncPushRequest request,
            CancellationToken cancellationToken)
        {
            if (!options.Value.Enabled)
            {
                return NotFound();
            }

            if (!TryGetIdentity(out string issuer, out string subject))
            {
                return Unauthorized();
            }

            SyncPushOutcome outcome = await syncServer.PushAsync(request, issuer, subject,
                cancellationToken);
            return StatusCode(outcome.StatusCode, outcome.Response is null
                ? new { message = outcome.Error }
                : outcome.Response);
        }

        [HttpGet("pull")]
        public async Task<IActionResult> Pull([FromQuery] Guid deviceId, [FromQuery] string? cursor,
            CancellationToken cancellationToken)
        {
            if (!options.Value.Enabled)
            {
                return NotFound();
            }

            if (!TryGetIdentity(out string issuer, out string subject))
            {
                return Unauthorized();
            }

            SyncPullOutcome outcome = await syncServer.PullAsync(deviceId, cursor, issuer, subject,
                cancellationToken);
            return StatusCode(outcome.StatusCode, outcome.Response is null
                ? new { message = outcome.Error }
                : outcome.Response);
        }

        [HttpHead("blobs/{hash}")]
        public async Task<IActionResult> BlobExists([FromRoute] string hash, [FromQuery] Guid workspaceId,
            CancellationToken cancellationToken)
        {
            if (!options.Value.Enabled)
            {
                return NotFound();
            }

            if (!await CanAccessWorkspaceAsync(workspaceId, cancellationToken))
            {
                return Forbid();
            }

            try
            {
                await using Stream stream = await blobStore.OpenReadAsync(new BlobHash(hash), cancellationToken);
                return Ok();
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("blobs/{hash}")]
        public async Task<IActionResult> DownloadBlob([FromRoute] string hash, [FromQuery] Guid workspaceId,
            CancellationToken cancellationToken)
        {
            if (!options.Value.Enabled)
            {
                return NotFound();
            }

            if (!await CanAccessWorkspaceAsync(workspaceId, cancellationToken))
            {
                return Forbid();
            }

            try
            {
                Stream stream = await blobStore.OpenReadAsync(new BlobHash(hash), cancellationToken);
                return File(stream, "application/octet-stream", enableRangeProcessing: true);
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("blobs/{hash}")]
        public async Task<IActionResult> UploadBlob([FromRoute] string hash, [FromQuery] Guid workspaceId,
            CancellationToken cancellationToken)
        {
            if (!options.Value.Enabled)
            {
                return NotFound();
            }

            if (!await CanAccessWorkspaceAsync(workspaceId, cancellationToken))
            {
                return Forbid();
            }

            if (Request.ContentLength is null || Request.ContentLength > options.Value.MaxStorageBytes)
            {
                return StatusCode(StatusCodes.Status413PayloadTooLarge);
            }

            BlobDescriptor descriptor = await blobStore.PutAsync(Request.Body,
                new BlobWriteOptions(Request.ContentType ?? "application/octet-stream"), cancellationToken);
            return descriptor.Hash.Value.Equals(hash, StringComparison.OrdinalIgnoreCase)
                ? NoContent()
                : Conflict(new { message = "The uploaded blob does not match the requested content hash." });
        }

        private bool TryGetIdentity(out string issuer, out string subject)
        {
            issuer = User.FindFirst("iss")?.Value ?? string.Empty;
            subject = User.FindFirst("sub")?.Value ?? string.Empty;
            return issuer.Length > 0 && subject.Length > 0;
        }

        private async Task<bool> CanAccessWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken)
        {
            return TryGetIdentity(out string issuer, out string subject)
                   && workspaceId != Guid.Empty
                   && await memberships.IsMemberAsync(WorkspaceId.Create(workspaceId), issuer, subject,
                       cancellationToken);
        }
    }
}
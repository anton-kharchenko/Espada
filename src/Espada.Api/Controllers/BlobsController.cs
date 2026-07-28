using Espada.Api.Contracts.Responses.Blobs;
using Espada.Application.Contracts.Blobs;
using Espada.Application.Models;
using Espada.Infrastructure.Constants;
using Espada.Infrastructure.Options;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Controllers
{
    [Route("api/v{version:apiVersion}/workspaces/{workspaceId:guid}/blobs")]
    public sealed class BlobsController(IBlobStoreService blobStoreService, IOptions<IngestionOptions> ingestionOptions)
        : BaseController
    {
        [HttpPost]
        [RequestSizeLimit(IngestionConstants.DefaultMaximumRawBytes)]
        [ProducesResponseType(typeof(UploadBlobResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        public async Task<IActionResult> Upload([FromRoute] Guid workspaceId,
            [FromHeader(Name = "X-File-Name")] [Required] string fileName, CancellationToken cancellationToken)
        {
            if (workspaceId == Guid.Empty || string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest();
            }

            string normalizedFileName = Path.GetFileName(fileName);
            if (!string.Equals(normalizedFileName, fileName, StringComparison.Ordinal) ||
                normalizedFileName is "." or "..")
            {
                return BadRequest("X-File-Name must contain a file name without a path.");
            }

            long maximumBytes = Math.Min(IngestionConstants.DefaultMaximumRawBytes,
                ingestionOptions.Value.MaximumRawBytes);
            IHttpMaxRequestBodySizeFeature? bodySize = HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
            if (bodySize is { IsReadOnly: false })
            {
                bodySize.MaxRequestBodySize = maximumBytes;
            }

            if (Request.ContentLength > maximumBytes)
            {
                return StatusCode(StatusCodes.Status413PayloadTooLarge);
            }

            string mediaType = Request.ContentType?.Split(';', 2)[0].Trim() ?? "application/octet-stream";
            BlobDescriptor blob =
                await blobStoreService.PutAsync(Request.Body, new BlobWriteOptions(mediaType), cancellationToken);
            UploadBlobResponse response = new(blob.Hash.Value, normalizedFileName, blob.MediaType, blob.Length);

            return CreatedAtAction(nameof(Upload), new { workspaceId, version = "1.0" }, response);
        }
    }
}
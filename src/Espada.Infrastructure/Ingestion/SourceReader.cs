using Espada.Application.Contracts.Blobs;
using Espada.Application.Contracts.Ingestion;
using Espada.Application.Enums;
using Espada.Application.Exceptions;
using Espada.Application.Models;
using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Espada.Application.Constants;

namespace Espada.Infrastructure.Ingestion
{
    internal sealed class SourceReader(
        IBlobStoreService blobStoreService,
        IConnectorSourceClient connectorSourceClient,
        IOptions<IngestionOptions> options) : ISourceReader
    {
        private const int MaximumRedirects = 10;
        private readonly IngestionOptions _options = options.Value;

        public async Task<SourceReadResult> ReadAsync(SourceDefinition definition,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(definition);
            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.OperationTimeoutSeconds));

            SourceReadResult result;
            try
            {
                result = definition switch
                {
                    FileSourceDefinition file => await ReadFileAsync(file, timeout.Token),
                    WebPageSourceDefinition webPage => await ReadWebPageAsync(webPage, timeout.Token),
                    PlainTextSourceDefinition text => FromUtf8(text.Content, text.Title, IngestionMediaTypeConstants.PlainText),
                    ConversationSourceDefinition conversation => FromUtf8(FormatConversation(conversation.Messages),
                        conversation.Title, IngestionMediaTypeConstants.PlainText),
                    ConnectorSourceDefinition connector => FromUtf8(
                        await connectorSourceClient.ReadAsync(connector, timeout.Token),
                        $"{connector.PluginId}-{connector.Resource}.txt", IngestionMediaTypeConstants.PlainText),
                    LegacySourceDefinition => throw new IngestionException(JobFailureCategoryType.Permanent,
                        IngestionFailureCodeConstants.LegacySourceUnsupported,
                        "Legacy source definitions must be registered again."),
                    _ => throw new IngestionException(JobFailureCategoryType.Poison,
                        IngestionFailureCodeConstants.UnknownSourceDefinition, "Unknown source definition.")
                };
            }
            catch (IngestionException)
            {
                throw;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new IngestionException(JobFailureCategoryType.Transient, IngestionFailureCodeConstants.ReadTimeout,
                    "Source reading exceeded the configured timeout.");
            }
            catch (Exception exception) when (exception is HttpRequestException or SocketException or IOException)
            {
                throw new IngestionException(JobFailureCategoryType.Transient, IngestionFailureCodeConstants.SourceUnavailable,
                    "Source could not be read because a dependency was unavailable.", exception);
            }

            if (result.Content.CanSeek && result.Content.Length > _options.MaximumRawBytes)
            {
                await result.Content.DisposeAsync();
                throw new IngestionException(JobFailureCategoryType.Permanent,
                    IngestionFailureCodeConstants.RawSizeLimitExceeded,
                    $"Source exceeds the {_options.MaximumRawBytes}-byte raw limit.");
            }

            return result;
        }

        private async Task<SourceReadResult> ReadFileAsync(FileSourceDefinition definition,
            CancellationToken cancellationToken)
        {
            if (definition.Blob is not null)
            {
                Stream blobStream =
                    await blobStoreService.OpenReadAsync(new BlobHash(definition.Blob.BlobHash), cancellationToken);
                return new SourceReadResult(blobStream, definition.Blob.FileName, definition.Blob.MediaType);
            }

            string path = ResolveAllowedPath(definition.LocalPath!);
            FileInfo file = new(path);
            if (!file.Exists)
            {
                throw new IngestionException(JobFailureCategoryType.Permanent, IngestionFailureCodeConstants.FileNotFound,
                    "Source file does not exist.");
            }

            if (file.Length > _options.MaximumRawBytes)
            {
                throw new IngestionException(JobFailureCategoryType.Permanent,
                    IngestionFailureCodeConstants.RawSizeLimitExceeded,
                    $"Source exceeds the {_options.MaximumRawBytes}-byte raw limit.");
            }

            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            return new SourceReadResult(stream, definition.FileName, definition.MediaType);
        }

        private string ResolveAllowedPath(string requestedPath)
        {
            string candidate = Path.GetFullPath(requestedPath);
            foreach (string configuredRoot in _options.AllowedFileRoots)
            {
                string root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(configuredRoot));
                string relative = Path.GetRelativePath(root, candidate);
                if (Path.IsPathRooted(relative) || relative.Equals("..", StringComparison.Ordinal) ||
                    relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                {
                    continue;
                }

                string current = root;
                foreach (string part in relative.Split(Path.DirectorySeparatorChar,
                             StringSplitOptions.RemoveEmptyEntries))
                {
                    current = Path.Join(current, part);
                    if (File.Exists(current) || Directory.Exists(current))
                    {
                        FileAttributes attributes = File.GetAttributes(current);
                        if ((attributes & FileAttributes.ReparsePoint) != 0)
                        {
                            throw new IngestionException(JobFailureCategoryType.Permanent,
                                IngestionFailureCodeConstants.FileReparsePointRejected,
                                "Source paths may not traverse reparse points.");
                        }
                    }
                }

                return candidate;
            }

            throw new IngestionException(JobFailureCategoryType.Permanent, IngestionFailureCodeConstants.FilePathNotAllowed,
                "Source path is outside the configured roots.");
        }

        private async Task<SourceReadResult> ReadWebPageAsync(WebPageSourceDefinition definition,
            CancellationToken cancellationToken)
        {
            Uri current = definition.Uri;
            for (int redirect = 0; redirect <= MaximumRedirects; redirect++)
            {
                IPAddress[] addresses = await Dns.GetHostAddressesAsync(current.DnsSafeHost, cancellationToken);
                IPAddress[] allowed = addresses.Where(IsPublicAddress).ToArray();
                if (allowed.Length == 0 || allowed.Length != addresses.Length)
                {
                    throw new IngestionException(JobFailureCategoryType.Permanent,
                        IngestionFailureCodeConstants.WebAddressNotPublic,
                        "Web sources must resolve only to public addresses.");
                }

                using SocketsHttpHandler handler = CreatePinnedHandler(current, allowed);
                using HttpClient client = new(handler);
                client.Timeout = Timeout.InfiniteTimeSpan;
                using HttpRequestMessage request = new(HttpMethod.Get, current);
                using HttpResponseMessage response = await client.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (IsRedirect(response.StatusCode))
                {
                    Uri? location = response.Headers.Location;
                    if (location is null)
                    {
                        throw new IngestionException(JobFailureCategoryType.Permanent,
                            IngestionFailureCodeConstants.InvalidRedirect,
                            "Web source returned a redirect without a location.");
                    }

                    current = location.IsAbsoluteUri ? location : new Uri(current, location);
                    if (!string.Equals(current.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new IngestionException(JobFailureCategoryType.Permanent,
                            IngestionFailureCodeConstants.InsecureRedirect, "Web source redirects must remain HTTPS.");
                    }

                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    bool transient =
                        response.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests ||
                        (int)response.StatusCode >= 500;
                    throw new IngestionException(
                        transient ? JobFailureCategoryType.Transient : JobFailureCategoryType.Permanent,
                        transient
                            ? IngestionFailureCodeConstants.WebSourceUnavailable
                            : IngestionFailureCodeConstants.WebSourceRejected,
                        $"Web source returned HTTP {(int)response.StatusCode}.");
                }

                long? contentLength = response.Content.Headers.ContentLength;
                if (contentLength > _options.MaximumRawBytes)
                {
                    throw new IngestionException(JobFailureCategoryType.Permanent,
                        IngestionFailureCodeConstants.RawSizeLimitExceeded,
                        $"Source exceeds the {_options.MaximumRawBytes}-byte raw limit.");
                }

                MemoryStream content = new();
                await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                await CopyLimitedAsync(responseStream, content, _options.MaximumRawBytes, cancellationToken);
                content.Position = 0;
                string mediaType = response.Content.Headers.ContentType?.MediaType ?? IngestionMediaTypeConstants.Html;
                return new SourceReadResult(content, current.Segments.LastOrDefault() ?? "page.html", mediaType);
            }

            throw new IngestionException(
                JobFailureCategoryType.Permanent,
                IngestionFailureCodeConstants.RedirectLimitExceeded,
                $"Web source exceeded {MaximumRedirects} redirects.");
        }

        private static SocketsHttpHandler CreatePinnedHandler(Uri uri, IReadOnlyList<IPAddress> addresses)
        {
            return new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                UseProxy = false,
                ConnectCallback = async (context, cancellationToken) =>
                {
                    if (!string.Equals(
                            context.DnsEndPoint.Host,
                            uri.DnsSafeHost,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        throw new HttpRequestException("Unexpected connection host.");
                    }

                    Exception? lastError = null;
                    foreach (IPAddress address in addresses)
                    {
                        Socket socket = new(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        try
                        {
                            await socket.ConnectAsync(
                                address,
                                context.DnsEndPoint.Port,
                                cancellationToken);
                            return new NetworkStream(socket, true);
                        }
                        catch (Exception exception) when (exception is SocketException or IOException
                                                              or ObjectDisposedException)
                        {
                            lastError = exception;
                            socket.Dispose();
                        }
                    }

                    throw new HttpRequestException("Unable to connect to the resolved public address.", lastError);
                }
            };
        }

        private static bool IsPublicAddress(IPAddress address)
        {
            if (IPAddress.IsLoopback(address))
            {
                return false;
            }

            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                return IsPublicIPv4Address(address.GetAddressBytes());
            }

            if (address.IsIPv6LinkLocal || address.IsIPv6Multicast || address.IsIPv6SiteLocal)
            {
                return false;
            }

            byte[] ipv6 = address.GetAddressBytes();
            return (ipv6[0] & 0xFE) != 0xFC;
        }

        private static bool IsPublicIPv4Address(byte[] bytes)
        {
            byte first = bytes[0];
            byte second = bytes[1];

            if (first is 0 or 10 or 127)
            {
                return false;
            }

            if (first == 169 && second == 254)
            {
                return false;
            }

            if (first == 172 && second is >= 16 and <= 31)
            {
                return false;
            }

            if (first == 192 && second == 168)
            {
                return false;
            }

            if (first == 100 && second is >= 64 and <= 127)
            {
                return false;
            }

            return first < 224;
        }

        private static bool IsRedirect(HttpStatusCode statusCode)
        {
            return statusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Redirect
                or HttpStatusCode.RedirectMethod or HttpStatusCode.TemporaryRedirect
                or HttpStatusCode.PermanentRedirect;
        }

        private static SourceReadResult FromUtf8(string content, string fileName, string mediaType)
        {
            return new SourceReadResult(new MemoryStream(Encoding.UTF8.GetBytes(content), false), fileName, mediaType);
        }

        private static string FormatConversation(IReadOnlyList<ConversationMessage> messages)
        {
            return string.Join(Environment.NewLine,
                messages.Select(message =>
                    $"{message.Timestamp?.ToString("O") ?? "-"} [{message.Role}]" +
                    $"{(string.IsNullOrWhiteSpace(message.Author) ? string.Empty : $" {message.Author}")}: {message.Content}"));
        }

        private static async Task CopyLimitedAsync(Stream source, Stream destination, long limit,
            CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[81920];
            long total = 0;
            int read;
            while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
            {
                total += read;
                if (total > limit)
                {
                    throw new IngestionException(JobFailureCategoryType.Permanent,
                        IngestionFailureCodeConstants.RawSizeLimitExceeded, $"Source exceeds the {limit}-byte raw limit.");
                }

                await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }
        }
    }
}
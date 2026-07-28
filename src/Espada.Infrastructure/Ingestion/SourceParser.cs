using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Espada.Application.Constants;
using Espada.Application.Contracts.Ingestion;
using Espada.Application.Enums;
using Espada.Application.Exceptions;
using Espada.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using UglyToad.PdfPig;
using Page = UglyToad.PdfPig.Content.Page;
using Text = DocumentFormat.OpenXml.Drawing.Text;

namespace Espada.Infrastructure.Ingestion
{
    internal sealed class SourceParser(IOptions<IngestionOptions> options) : ISourceParser
    {
        private static readonly UTF8Encoding StrictUtf8 = new(false, true);
        private readonly IngestionOptions _options = options.Value;

        public async Task<string> ParseAsync(Stream content, string fileName, string mediaType,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentException.ThrowIfNullOrWhiteSpace(mediaType);

            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (extension is IngestionFileExtensionConstants.LegacyDocument or IngestionFileExtensionConstants.LegacySpreadsheet
                or IngestionFileExtensionConstants.LegacyPresentation)
            {
                throw Unsupported("Legacy DOC, XLS and PPT files are not supported.");
            }

            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.OperationTimeoutSeconds));

            try
            {
                string extracted = (mediaType.ToLowerInvariant(), extension) switch
                {
                    (IngestionMediaTypeConstants.Pdf, _) or (_, IngestionFileExtensionConstants.Pdf) =>
                        ParsePdf(content, timeout.Token),
                    (IngestionMediaTypeConstants.OpenXmlDocument, _) or (_, IngestionFileExtensionConstants.OpenXmlDocument) =>
                        ParseDocx(content, timeout.Token),
                    (IngestionMediaTypeConstants.OpenXmlSpreadsheet, _) or (_, IngestionFileExtensionConstants.OpenXmlSpreadsheet) =>
                        ParseXlsx(content, timeout.Token),
                    (IngestionMediaTypeConstants.OpenXmlPresentation, _) or (_, IngestionFileExtensionConstants.OpenXmlPresentation) =>
                        ParsePptx(content, timeout.Token),
                    (IngestionMediaTypeConstants.Html, _) or (IngestionMediaTypeConstants.Xhtml, _)
                        or (_, IngestionFileExtensionConstants.Html or IngestionFileExtensionConstants.ShortHtml) =>
                        await ParseHtmlAsync(content, timeout.Token),
                    (IngestionMediaTypeConstants.Json, _) or (_, IngestionFileExtensionConstants.Json) => await ParseJsonAsync(content,
                        timeout.Token),
                    var value when value.Item1.StartsWith("text/", StringComparison.Ordinal) ||
                                   extension is IngestionFileExtensionConstants.Markdown or IngestionFileExtensionConstants.LongMarkdown
                                       or IngestionFileExtensionConstants.Text => await ReadTextAsync(content, timeout.Token),
                    _ => throw Unsupported($"Media type '{mediaType}' is not supported.")
                };

                int extractedBytes = Encoding.UTF8.GetByteCount(extracted);
                if (extractedBytes > _options.MaximumExtractedBytes)
                {
                    throw new IngestionException(JobFailureCategoryType.Permanent,
                        IngestionFailureCodeConstants.ExtractedSizeLimitExceeded,
                        $"Extracted text exceeds the {_options.MaximumExtractedBytes}-byte limit.");
                }

                return extracted;
            }
            catch (IngestionException)
            {
                throw;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new IngestionException(JobFailureCategoryType.Permanent, IngestionFailureCodeConstants.ParseTimeout,
                    "Source parsing exceeded the configured timeout.");
            }
            catch (JsonException exception)
            {
                throw Malformed("Malformed JSON source.", exception);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                throw Malformed("Source content is malformed or unsupported.", exception);
            }
        }

        private static async Task<string> ReadTextAsync(Stream content, CancellationToken cancellationToken)
        {
            using StreamReader reader = new(content, StrictUtf8, leaveOpen: true);
            return await reader.ReadToEndAsync(cancellationToken);
        }

        private static async Task<string> ParseJsonAsync(Stream content, CancellationToken cancellationToken)
        {
            JsonDocument document = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
            using (document)
            {
                return JsonSerializer.Serialize(document.RootElement,
                    new JsonSerializerOptions { WriteIndented = true });
            }
        }

        private static async Task<string> ParseHtmlAsync(Stream content, CancellationToken cancellationToken)
        {
            HtmlParser parser = new();
            IHtmlDocument document = await parser.ParseDocumentAsync(content, cancellationToken);
            document.QuerySelectorAll("script,style,noscript,template").ToList().ForEach(node => node.Remove());
            INode root = document.Body ?? document.DocumentElement;
            return NormalizeWhitespace(string.Join(' ', root.Descendants<IText>().Select(node => node.Data)));
        }

        private static string ParsePdf(Stream content, CancellationToken cancellationToken)
        {
            using PdfDocument document = PdfDocument.Open(content);
            StringBuilder text = new();
            foreach (Page page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                text.AppendLine(page.Text);
            }

            return text.ToString().Trim();
        }

        private static string ParseDocx(Stream content, CancellationToken cancellationToken)
        {
            using WordprocessingDocument document = WordprocessingDocument.Open(content, false);
            cancellationToken.ThrowIfCancellationRequested();
            return document.MainDocumentPart?.Document?.Body?.InnerText.Trim() ??
                   throw Malformed("DOCX document has no body.");
        }

        private static string ParseXlsx(Stream content, CancellationToken cancellationToken)
        {
            using SpreadsheetDocument document = SpreadsheetDocument.Open(content, false);
            WorkbookPart workbook =
                document.WorkbookPart ?? throw Malformed("XLSX workbook is missing its workbook part.");
            Workbook workbookDocument = workbook.Workbook ?? throw Malformed("XLSX workbook is missing its workbook.");
            SharedStringTable? sharedStrings = workbook.SharedStringTablePart?.SharedStringTable;
            StringBuilder text = new();
            foreach (Sheet sheet in workbookDocument.Sheets?.Elements<Sheet>() ?? [])
            {
                cancellationToken.ThrowIfCancellationRequested();
                text.AppendLine($"# {sheet.Name}");
                WorksheetPart worksheet = (WorksheetPart)workbook.GetPartById(sheet.Id!);
                IEnumerable<string> rows = (worksheet.Worksheet?.Descendants<Row>() ?? [])
                    .Select(row => string.Join(
                        '\t',
                        row.Elements<Cell>().Select(cell => ResolveCell(cell, sharedStrings))));
                foreach (string rowText in rows)
                {
                    text.AppendLine(rowText);
                }
            }

            return text.ToString().Trim();
        }

        private static string ParsePptx(Stream content, CancellationToken cancellationToken)
        {
            using PresentationDocument document = PresentationDocument.Open(content, false);
            PresentationPart presentation = document.PresentationPart ??
                                            throw Malformed("PPTX presentation is missing its presentation part.");
            StringBuilder text = new();
            int slideNumber = 0;
            foreach (SlidePart slide in presentation.SlideParts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                text.AppendLine($"# Slide {++slideNumber}");
                foreach (Text value in slide.Slide?.Descendants<Text>() ?? [])
                {
                    text.AppendLine(value.Text);
                }
            }

            return text.ToString().Trim();
        }

        private static string ResolveCell(Cell cell, SharedStringTable? sharedStrings)
        {
            string value = cell.CellValue?.InnerText ?? cell.InlineString?.InnerText ?? string.Empty;
            return cell.DataType?.Value == CellValues.SharedString && int.TryParse(value, out int index) &&
                   sharedStrings?.ElementAtOrDefault(index) is SharedStringItem item
                ? item.InnerText
                : value;
        }

        private static string NormalizeWhitespace(string value)
        {
            return string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        }

        private static IngestionException Unsupported(string message)
        {
            return new IngestionException(JobFailureCategoryType.Permanent, IngestionFailureCodeConstants.UnsupportedFormat,
                message);
        }

        private static IngestionException Malformed(string message, Exception? innerException = null)
        {
            return new IngestionException(JobFailureCategoryType.Permanent, IngestionFailureCodeConstants.MalformedSource,
                message,
                innerException);
        }
    }
}
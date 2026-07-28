namespace Espada.Application.Constants
{
    public static class IngestionMediaTypeConstants
    {
        public const string Binary = "application/octet-stream";
        public const string Json = "application/json";
        public const string Pdf = "application/pdf";
        public const string Html = "text/html";
        public const string Xhtml = "application/xhtml+xml";
        public const string PlainText = "text/plain";
        public const string Utf8PlainText = "text/plain; charset=utf-8";
        public const string Markdown = "text/markdown";
        public const string OpenXmlDocument = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public const string OpenXmlSpreadsheet = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public const string OpenXmlPresentation =
            "application/vnd.openxmlformats-officedocument.presentationml.presentation";
    }
}
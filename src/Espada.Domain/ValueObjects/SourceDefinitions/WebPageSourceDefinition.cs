using Espada.Domain.Enums;

namespace Espada.Domain.ValueObjects.SourceDefinitions
{
    public sealed record WebPageSourceDefinition : SourceDefinition
    {
        public WebPageSourceDefinition(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);
            if (!uri.IsAbsoluteUri ||
                !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Web page source must use an absolute HTTPS URI.", nameof(uri));
            }

            Uri = uri;
        }

        public Uri Uri { get; init; }

        public override SourceType SourceType => SourceType.WebPage;

        public override string CanonicalLocator => Uri.AbsoluteUri.TrimEnd('/');
    }
}
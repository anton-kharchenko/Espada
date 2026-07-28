using System.Net;

namespace Espada.Mcp.Security
{
    internal sealed class McpAuthorizationOptions
    {
        public Uri Issuer { get; set; } = new("http://127.0.0.1:7433/");

        public Uri Resource { get; set; } = new("http://127.0.0.1:7433/mcp");

        public int DefaultRateCeilingPerMinute { get; set; } = 60;

        public int MaximumRateCeilingPerMinute { get; set; } = 60;

        public int Burst { get; set; } = 10;

        public string LocalIdentityIssuer { get; set; } = "espada:local";

        public string LocalIdentitySubject { get; set; } = Environment.UserName;

        public string? EntraAuthority { get; set; }

        public string? EntraClientId { get; set; }

        public string? EntraClientSecret { get; set; }

        public string? SigningCertificateThumbprint { get; set; }

        public string? EncryptionCertificateThumbprint { get; set; }

        public string? SigningCertificateBase64 { get; set; }

        public string? EncryptionCertificateBase64 { get; set; }

        public List<string> AllowedOrigins { get; set; } = [];

        public bool HasEntraAuthority =>
            !string.IsNullOrWhiteSpace(EntraAuthority)
            && !string.IsNullOrWhiteSpace(EntraClientId)
            && !string.IsNullOrWhiteSpace(EntraClientSecret);

        public bool IsValid()
        {
            if (!Issuer.IsAbsoluteUri
                || !Resource.IsAbsoluteUri
                || !Resource.AbsolutePath.Equals("/mcp", StringComparison.Ordinal)
                || DefaultRateCeilingPerMinute <= 0
                || MaximumRateCeilingPerMinute <= 0
                || DefaultRateCeilingPerMinute > MaximumRateCeilingPerMinute
                || Burst <= 0
                || string.IsNullOrWhiteSpace(LocalIdentityIssuer)
                || string.IsNullOrWhiteSpace(LocalIdentitySubject))
            {
                return false;
            }

            bool resourceMatchesIssuer =
                Resource.Scheme.Equals(
                    Issuer.Scheme,
                    StringComparison.OrdinalIgnoreCase)
                && Resource.Host.Equals(
                    Issuer.Host,
                    StringComparison.OrdinalIgnoreCase)
                && Resource.Port == Issuer.Port;
            bool usesHttps = Issuer.Scheme.Equals(
                Uri.UriSchemeHttps,
                StringComparison.OrdinalIgnoreCase);
            if (usesHttps && !HasTokenProtectionCertificates())
            {
                return false;
            }

            if (!resourceMatchesIssuer
                || !Issuer.Scheme.Equals(
                    Uri.UriSchemeHttp,
                    StringComparison.OrdinalIgnoreCase))
            {
                return resourceMatchesIssuer
                       && (Issuer.IsLoopback || HasValidEntraAuthority());
            }

            return Issuer.IsLoopback
                   || (IPAddress.TryParse(Issuer.Host, out IPAddress? address)
                       && IPAddress.IsLoopback(address));
        }

        private bool HasTokenProtectionCertificates()
        {
            bool hasThumbprints =
                !string.IsNullOrWhiteSpace(SigningCertificateThumbprint)
                && !string.IsNullOrWhiteSpace(
                    EncryptionCertificateThumbprint)
                && !SigningCertificateThumbprint.Equals(
                    EncryptionCertificateThumbprint,
                    StringComparison.OrdinalIgnoreCase);
            bool hasCertificates =
                !string.IsNullOrWhiteSpace(SigningCertificateBase64)
                && !string.IsNullOrWhiteSpace(EncryptionCertificateBase64)
                && !SigningCertificateBase64.Equals(
                    EncryptionCertificateBase64,
                    StringComparison.Ordinal);

            return hasThumbprints || hasCertificates;
        }

        private bool HasValidEntraAuthority()
        {
            return HasEntraAuthority
                   && Uri.TryCreate(
                       EntraAuthority,
                       UriKind.Absolute,
                       out Uri? authority)
                   && authority.Scheme.Equals(
                       Uri.UriSchemeHttps,
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}

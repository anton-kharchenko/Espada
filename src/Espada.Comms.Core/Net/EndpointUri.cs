using System.Diagnostics.CodeAnalysis;

namespace Espada.Comms.Core.Net;

public static class EndpointUri
{
    public static Uri Create(string? value, string settingName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(settingName);

        return TryCreate(value, out Uri? uri) ? uri : throw new InvalidOperationException($"{settingName} must be an absolute HTTP or HTTPS URL.");
    }

    public static bool TryCreate(string? value, [NotNullWhen(true)] out Uri? uri)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out Uri? candidate) && (candidate.Scheme == Uri.UriSchemeHttps || candidate.Scheme == Uri.UriSchemeHttp))
        {
            uri = candidate;

            return true;
        }

        uri = null;

        return false;
    }
}
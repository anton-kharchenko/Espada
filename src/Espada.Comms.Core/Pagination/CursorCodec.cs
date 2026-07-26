using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Espada.Comms.Core.Pagination;

public static class CursorCodec
{
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public static string Encode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return Convert
            .ToBase64String(StrictUtf8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static bool TryDecode(string? cursor, [NotNullWhen(true)] out string? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(cursor))
        {
            return false;
        }

        string base64 = cursor
            .Replace('-', '+')
            .Replace('_', '/');

        int remainder = base64.Length % 4;

        switch (remainder)
        {
            case 1:
                return false;
            case > 0:
                base64 = base64.PadRight(base64.Length + (4 - remainder), '=');
                break;
        }

        try
        {
            value = StrictUtf8.GetString(Convert.FromBase64String(base64));

            return !string.IsNullOrWhiteSpace(value);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }
}
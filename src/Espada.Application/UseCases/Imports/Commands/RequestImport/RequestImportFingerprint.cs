using System.Security.Cryptography;
using System.Text.Json;

namespace Espada.Application.UseCases.Imports.Commands.RequestImport
{
    internal static class RequestImportFingerprint
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        public static string Create(Guid sourceId, ImportOptions options)
        {
            byte[] payload = JsonSerializer.SerializeToUtf8Bytes(new { sourceId, options }, SerializerOptions);
            return Convert.ToHexStringLower(SHA256.HashData(payload));
        }

        public static string SerializeOptions(ImportOptions options)
        {
            return JsonSerializer.Serialize(options, SerializerOptions);
        }
    }
}
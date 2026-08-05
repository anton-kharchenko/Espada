using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    public sealed class SyncDeviceRegistrations
    {
        private SyncDeviceRegistrations()
        {
        }

        public SyncDeviceRegistrations(Guid deviceId, string issuer, string subject, DateTimeOffset registeredAtUtc)
        {
            DeviceId = deviceId;
            Issuer = issuer;
            Subject = subject;
            RegisteredAtUtc = registeredAtUtc;
        }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid DeviceId { get; private set; }
        [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying500)]
        public string Issuer { get; private set; } = string.Empty;
        [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying500)]
        public string Subject { get; private set; } = string.Empty;
        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
        public DateTimeOffset RegisteredAtUtc { get; private set; }
    }
}
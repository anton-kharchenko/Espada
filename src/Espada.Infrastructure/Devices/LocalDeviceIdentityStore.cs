using System.Security.Cryptography;

namespace Espada.Infrastructure.Devices
{
    public sealed class LocalDeviceIdentityStore
    {
        private readonly string _path;

        public LocalDeviceIdentityStore()
        {
            string root = Environment.GetEnvironmentVariable("ESPADA_DATA_ROOT")
                ?? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Espada");
            _path = Path.Join(root, "device-id");
        }

        public Guid GetOrCreate()
        {
            if (File.Exists(_path) && Guid.TryParse(File.ReadAllText(_path).Trim(), out Guid existing))
            {
                return existing;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            Guid deviceId = Guid.NewGuid();
            string temporary = _path + ".tmp-" + Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(8));
            File.WriteAllText(temporary, deviceId.ToString("D"));
            File.Move(temporary, _path, false);
            return deviceId;
        }
    }
}
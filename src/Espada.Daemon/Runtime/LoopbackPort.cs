using System.Net;
using System.Net.Sockets;

namespace Espada.Daemon.Runtime
{
    public static class LoopbackPort
    {
        public static int GetAvailable()
        {
            using TcpListener listener = new(IPAddress.Loopback, 0);
            listener.Start();
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }

        public static void EnsureAvailable(int port)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(port, IPEndPoint.MinPort);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(port, IPEndPoint.MaxPort);
            try
            {
                using TcpListener listener = new(IPAddress.Loopback, port);
                listener.Start();
            }
            catch (SocketException exception)
            {
                throw new InvalidOperationException($"Loopback port {port} is already in use.", exception);
            }
        }
    }
}

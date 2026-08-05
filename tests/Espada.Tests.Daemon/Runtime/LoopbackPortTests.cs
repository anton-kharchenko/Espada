using Espada.Daemon.Runtime;
using System.Net;
using System.Net.Sockets;

namespace Espada.Tests.Daemon.Runtime
{
    public sealed class LoopbackPortTests
    {
        [Fact]
        public void EnsureAvailable_WhenPortIsOccupied_ShouldFailWithoutTakingItOver()
        {
            using TcpListener listener = new(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => LoopbackPort.EnsureAvailable(port));

            Assert.Contains(port.ToString(), exception.Message, StringComparison.Ordinal);
        }
    }
}
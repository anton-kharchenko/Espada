using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Espada.Tests.Api.Fixtures
{
    internal sealed class LoopbackConnectionStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(
            Action<IApplicationBuilder> next)
        {
            return application =>
            {
                application.Use(
                    async (context, continuation) =>
                    {
                        context.Connection.RemoteIpAddress =
                            IPAddress.Loopback;
                        await continuation();
                    });
                next(application);
            };
        }
    }
}

using Espada.Application.Contracts.Security;
using Espada.Application.Models;

namespace Espada.Worker
{
    internal sealed class BackgroundRequestPrincipalAccessor
        : IRequestPrincipalAccessor
    {
        public RequestPrincipal? Principal => null;
    }
}

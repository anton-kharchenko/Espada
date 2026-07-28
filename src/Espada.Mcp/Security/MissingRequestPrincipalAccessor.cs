using Espada.Application.Contracts.Security;
using Espada.Application.Models;

namespace Espada.Mcp.Security
{
    internal sealed class MissingRequestPrincipalAccessor
        : IRequestPrincipalAccessor
    {
        public RequestPrincipal? Principal => null;
    }
}
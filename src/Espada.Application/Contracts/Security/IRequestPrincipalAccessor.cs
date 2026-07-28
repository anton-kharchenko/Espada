using Espada.Application.Models;

namespace Espada.Application.Contracts.Security
{
    public interface IRequestPrincipalAccessor
    {
        RequestPrincipal? Principal { get; }
    }
}
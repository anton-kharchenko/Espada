using Espada.Application.Contracts.Security;
using Espada.Application.Models;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class RequestPrincipalAccessorStub : IRequestPrincipalAccessor
    {
        public RequestPrincipal? Principal { get; set; }
    }
}
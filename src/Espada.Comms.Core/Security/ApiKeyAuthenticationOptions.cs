using Microsoft.AspNetCore.Authentication;
using Espada.Comms.Core.Constants;

namespace Espada.Comms.Core.Security
{
    public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string HeaderName { get; set; } = ApiKeyAuthenticationConstants.DefaultHeaderName;

        public string ApiKey { get; set; } = string.Empty;
    }
}
using Microsoft.AspNetCore.Authentication;

namespace Espada.Comms.Core.Security;

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string HeaderName { get; set; } = ApiKeyAuthenticationDefaults.DefaultHeaderName;

    public string ApiKey { get; set; } = string.Empty;
}
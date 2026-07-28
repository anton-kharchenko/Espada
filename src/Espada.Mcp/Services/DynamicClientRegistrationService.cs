using Espada.Application.Contracts.Time;
using OpenIddict.Abstractions;
using System.Net;
using Espada.Application.Constants;
using Espada.Mcp.Constants;
using Espada.Mcp.Requests;
using Espada.Mcp.Responses;
using Espada.Mcp.Security;

namespace Espada.Mcp.Services
{
    internal sealed class DynamicClientRegistrationService(
        IOpenIddictApplicationManager applicationManager,
        IClockService clockService)
    {
        private const int ClientNameMaxLength = 200;

        public async Task<DynamicClientRegistrationResponse> RegisterAsync(
            DynamicClientRegistrationRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            string clientName = ValidateClientName(request.ClientName);
            Uri[] redirectUris = ValidateRedirectUris(request.RedirectUris);
            string[] scopes = ValidateScopes(request.Scope);
            ValidateProtocolMetadata(request);

            string clientId = $"espada_{Guid.NewGuid():N}";
            OpenIddictApplicationDescriptor descriptor = new()
            {
                ClientId = clientId,
                ClientType = OpenIddictConstants.ClientTypes.Public,
                ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                DisplayName = clientName
            };
            foreach (Uri redirectUri in redirectUris)
            {
                descriptor.RedirectUris.Add(redirectUri);
            }

            descriptor.Permissions.Add(
                OpenIddictConstants.Permissions.Endpoints.Authorization);
            descriptor.Permissions.Add(
                OpenIddictConstants.Permissions.Endpoints.Token);
            descriptor.Permissions.Add(
                OpenIddictConstants.Permissions.Endpoints.Revocation);
            descriptor.Permissions.Add(
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
            descriptor.Permissions.Add(
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
            descriptor.Permissions.Add(
                OpenIddictConstants.Permissions.ResponseTypes.Code);
            descriptor.Requirements.Add(
                OpenIddictConstants.Requirements.Features
                    .ProofKeyForCodeExchange);
            foreach (string scope in scopes)
            {
                descriptor.Permissions.Add(
                    OpenIddictConstants.Permissions.Prefixes.Scope + scope);
            }

            await applicationManager.CreateAsync(
                descriptor,
                cancellationToken);

            return new DynamicClientRegistrationResponse(
                clientId,
                clockService.UtcNow.ToUnixTimeSeconds(),
                clientName,
                redirectUris.Select(uri => uri.AbsoluteUri).ToArray(),
                OpenIddictConstants.ClientAuthenticationMethods.None,
                [
                    OpenIddictConstants.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.GrantTypes.RefreshToken
                ],
                [OpenIddictConstants.ResponseTypes.Code],
                string.Join(' ', scopes));
        }

        private static string ValidateClientName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidClientMetadataException(
                    "client_name is required.");
            }

            string clientName = value.Trim();
            if (clientName.Length > ClientNameMaxLength)
            {
                throw new InvalidClientMetadataException(
                    $"client_name cannot exceed {ClientNameMaxLength} characters.");
            }

            return clientName;
        }

        private static Uri[] ValidateRedirectUris(
            IReadOnlyList<string>? values)
        {
            if (values is null || values.Count == 0)
            {
                throw new InvalidClientMetadataException(
                    "At least one redirect_uri is required.");
            }

            Uri[] redirectUris = values
                .Distinct(StringComparer.Ordinal)
                .Select(ParseRedirectUri)
                .ToArray();
            return redirectUris.Length == values.Count
                ? redirectUris
                : throw new InvalidClientMetadataException(
                    "redirect_uris cannot contain duplicates.");
        }

        private static Uri ParseRedirectUri(string value)
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? redirectUri)
                || !string.IsNullOrEmpty(redirectUri.Fragment)
                || !string.IsNullOrEmpty(redirectUri.UserInfo)
                || redirectUri.Scheme is not ("http" or "https"))
            {
                throw new InvalidClientMetadataException(
                    "Each redirect_uri must be an absolute HTTP or HTTPS URI without user information or a fragment.");
            }

            bool loopback = redirectUri.Host.Equals(
                                "localhost",
                                StringComparison.OrdinalIgnoreCase)
                            || (IPAddress.TryParse(
                                    redirectUri.Host,
                                    out IPAddress? address)
                                && IPAddress.IsLoopback(address));
            if (redirectUri.Scheme == Uri.UriSchemeHttp && !loopback)
            {
                throw new InvalidClientMetadataException(
                    "HTTP redirect_uris must use a loopback host.");
            }

            return redirectUri;
        }

        private static string[] ValidateScopes(string? value)
        {
            string[] scopes = string.IsNullOrWhiteSpace(value)
                ? ApplicationScopeConstants.All
                    .Append(
                        McpAuthorizationConstants.OfflineAccessScope)
                    .Order()
                    .ToArray()
                : value.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries
                        | StringSplitOptions.TrimEntries)
                    .Distinct(StringComparer.Ordinal)
                    .Order()
                    .ToArray();
            string? unsupported = scopes.FirstOrDefault(scope => !ApplicationScopeConstants.All.Contains(scope)
                                                                 && scope
                                                                 != McpAuthorizationConstants.OfflineAccessScope);
            return unsupported is null
                ? scopes
                : throw new InvalidClientMetadataException(
                    $"Scope '{unsupported}' is not supported.");
        }

        private static void ValidateProtocolMetadata(
            DynamicClientRegistrationRequest request)
        {
            if (request.TokenEndpointAuthMethod is not null
                && !request.TokenEndpointAuthMethod.Equals(
                    OpenIddictConstants.ClientAuthenticationMethods.None,
                    StringComparison.Ordinal))
            {
                throw new InvalidClientMetadataException(
                    "Only public clients using token_endpoint_auth_method 'none' are supported.");
            }

            if (request.GrantTypes is not null
                && request.GrantTypes.Any(grantType =>
                    grantType
                        is not OpenIddictConstants.GrantTypes.AuthorizationCode
                        and not OpenIddictConstants.GrantTypes.RefreshToken))
            {
                throw new InvalidClientMetadataException(
                    "Only authorization_code and refresh_token grants are supported.");
            }

            if (request.ResponseTypes is not null
                && request.ResponseTypes.Any(responseType =>
                    responseType
                        is not OpenIddictConstants.ResponseTypes.Code))
            {
                throw new InvalidClientMetadataException(
                    "Only the code response type is supported.");
            }
        }
    }
}
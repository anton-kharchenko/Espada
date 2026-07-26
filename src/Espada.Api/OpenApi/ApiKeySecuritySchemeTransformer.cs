using Espada.Api.Security;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Espada.Api.OpenApi;

internal sealed class ApiKeySecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[ApiKeyAuthenticationDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = ApiKeyAuthenticationDefaults.DefaultHeaderName,
            Description = "Espada local API key."
        };

        return Task.CompletedTask;
    }
}
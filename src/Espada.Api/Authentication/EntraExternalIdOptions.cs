namespace Espada.Api.Authentication;

internal sealed class EntraExternalIdOptions
{
    public string Authority { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;

    public bool IsConfigured() =>
        Uri.TryCreate(Authority, UriKind.Absolute, out Uri? authority)
        && authority.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(Audience)
        && !string.IsNullOrWhiteSpace(Scope);
}
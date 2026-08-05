namespace Espada.Application.Models
{
    public sealed record ContextSelectorMatch(
        string Selector,
        string? Expected,
        string? Actual,
        bool Matched);
}
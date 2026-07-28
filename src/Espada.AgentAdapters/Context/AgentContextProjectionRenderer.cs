using Espada.Application.UseCases.Context.Queries.BuildContext;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Espada.Application.Constants;

namespace Espada.AgentAdapters.Context
{
    public static class AgentContextProjectionRenderer
    {
        private static readonly JsonSerializerOptions JsonOptions = new(
            JsonSerializerDefaults.Web) { WriteIndented = true };

        public static AgentContextProjection Render(BuildContextResponse context)
        {
            ArgumentNullException.ThrowIfNull(context);

            (string format, string mediaType, string content) = context.Agent switch
            {
                ContextAgentConstants.Codex => (
                    "agents",
                    "text/markdown",
                    RenderText("Codex Instructions", context)),
                ContextAgentConstants.Claude => (
                    "claude",
                    "text/markdown",
                    RenderText("Claude Project Context", context)),
                ContextAgentConstants.Gemini => (
                    "gemini",
                    "text/markdown",
                    RenderText("Gemini Context", context)),
                ContextAgentConstants.Generic => (
                    "canonical-json",
                    "application/json",
                    RenderJson(context)),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(context),
                    context.Agent,
                    "Unsupported context projection agent.")
            };

            return new AgentContextProjection(
                context.Agent,
                format,
                mediaType,
                content,
                Encoding.UTF8.GetByteCount(content));
        }

        private static string RenderText(
            string heading,
            BuildContextResponse context)
        {
            StringBuilder builder = new();
            builder.Append("# ").Append(heading).Append('\n')
                .Append("Source: Espada canonical context").Append('\n')
                .Append("Agent: ").Append(context.Agent).Append('\n')
                .Append('\n')
                .Append("## Resolved context").Append('\n');

            for (int index = 0; index < context.IncludedItems.Count; index++)
            {
                ContextItemResponse item = context.IncludedItems[index];
                builder.Append('\n')
                    .Append("### ").Append(index + 1).Append(". ")
                    .Append(item.Title).Append('\n')
                    .Append("Kind: ").Append(item.ArtifactKind).Append('\n');
                if (item.RuleKey is not null)
                {
                    builder.Append("Rule: ").Append(item.RuleKey).Append('\n');
                }

                if (item.Enforcement is not null)
                {
                    builder.Append("Enforcement: [")
                        .Append(item.Enforcement)
                        .Append(']')
                        .Append('\n');
                }

                if (item.UserConfirmed.HasValue)
                {
                    builder.Append("Status: ")
                        .Append(item.UserConfirmed.Value
                            ? "[confirmed]"
                            : "[unconfirmed]")
                        .Append('\n');
                    if (item.Confidence.HasValue)
                    {
                        builder.Append("Confidence: ")
                            .Append(item.Confidence.Value.ToString(
                                "0.###",
                                CultureInfo.InvariantCulture))
                            .Append('\n');
                    }

                    if (item.Provenance is not null)
                    {
                        builder.Append("Provenance: client=")
                            .Append(item.Provenance.ClientIdentity);
                        if (item.Provenance.SessionIdentity is not null)
                        {
                            builder.Append("; session=")
                                .Append(item.Provenance.SessionIdentity);
                        }

                        builder.Append('\n');
                    }
                }

                builder.Append("Content:").Append('\n')
                    .Append(NormalizeLineEndings(item.Content))
                    .Append('\n');
            }

            return builder.ToString();
        }

        private static string RenderJson(BuildContextResponse context)
        {
            object projection = new
            {
                context.WorkspaceId,
                context.OrganizationId,
                context.ProjectId,
                context.TaskId,
                context.RepositoryCanonicalUri,
                context.RepositoryRelativePath,
                context.Branch,
                context.Agent,
                Items = context.IncludedItems.Select(item => new
                {
                    item.BindingId,
                    item.ArtifactId,
                    item.RevisionId,
                    item.ArtifactKind,
                    item.Title,
                    item.RuleKey,
                    item.Enforcement,
                    item.Content,
                    item.RulePriority,
                    item.ArtifactPriority,
                    Status = item.UserConfirmed.HasValue
                        ? item.UserConfirmed.Value
                            ? "[confirmed]"
                            : "[unconfirmed]"
                        : null,
                    item.UserConfirmed,
                    item.Confidence,
                    Provenance = item.Provenance is null
                        ? null
                        : new { item.Provenance.ClientIdentity, item.Provenance.SessionIdentity },
                    item.SizeInBytes
                }),
                context.Conflicts,
                context.Budget
            };

            return $"{NormalizeLineEndings(
                JsonSerializer.Serialize(projection, JsonOptions))}\n";
        }

        private static string NormalizeLineEndings(string value)
        {
            return value.Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n');
        }
    }
}
using Espada.AgentAdapters.Context;
using Espada.Application.Constants;
using Espada.Application.UseCases.Context.Queries.BuildContext;
using Espada.Application.UseCases.Memories.Queries.SearchMemory;
using Espada.Tests.AgentAdapters.TestData;
using System.Text;

namespace Espada.Tests.AgentAdapters.Context
{
    public sealed class AgentContextProjectionRendererTests
    {
        [Theory]
        [MemberData(nameof(AgentContextProjectionTestData.GoldenFiles), MemberType = typeof(AgentContextProjectionTestData))]
        public void Render_ShouldMatchGoldenFile(
            string agent,
            string goldenFile)
        {
            BuildContextResponse context = CreateContext(agent);

            AgentContextProjection first =
                AgentContextProjectionRenderer.Render(context);
            AgentContextProjection second =
                AgentContextProjectionRenderer.Render(context);

            string expected = File.ReadAllText(
                    Path.Join(
                        AppContext.BaseDirectory,
                        "Golden",
                        "Context",
                        goldenFile))
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n');
            Assert.Equal(expected, first.Content);
            Assert.Equal(first, second);
            Assert.Equal(
                Encoding.UTF8.GetByteCount(first.Content),
                first.SizeInBytes);
            Assert.DoesNotContain("2026-07-28", first.Content, StringComparison.Ordinal);
            Assert.Contains("[unconfirmed]", first.Content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Render_ShouldNotCreateCompatibilityFiles()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string[] before = Directory.GetFiles(
                currentDirectory,
                "*.*",
                SearchOption.TopDirectoryOnly);

            _ = AgentContextProjectionRenderer.Render(
                CreateContext(ContextAgentConstants.Codex));

            string[] after = Directory.GetFiles(
                currentDirectory,
                "*.*",
                SearchOption.TopDirectoryOnly);
            Assert.Equal(before, after);
            Assert.DoesNotContain(
                after,
                path => Path.GetFileName(path) is
                    "AGENTS.md" or "CLAUDE.md" or "GEMINI.md");
        }

        private static BuildContextResponse CreateContext(string agent)
        {
            ContextSpecificityResponse specificity = new(
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0);
            ContextSelectorMatchResponse[] selectors =
            [
                new("workspace", "11111111-1111-1111-1111-111111111111", "11111111-1111-1111-1111-111111111111", true)
            ];
            ContextItemResponse[] items =
            [
                new(
                    Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                    Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"),
                    Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1"),
                    "policy",
                    "Secrets policy",
                    "security.secrets",
                    "hard",
                    "Never expose secrets.",
                    100,
                    100,
                    null,
                    null,
                    null,
                    specificity,
                    selectors,
                    21,
                    ContextDecisionCodeConstants.Included),
                new(
                    Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                    Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"),
                    Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc2"),
                    "instruction",
                    "Repository style",
                    "repo.style",
                    null,
                    "Follow repository style.",
                    10,
                    20,
                    null,
                    null,
                    null,
                    specificity,
                    selectors,
                    24,
                    ContextDecisionCodeConstants.Included),
                new(
                    Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                    Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3"),
                    Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc3"),
                    "memory",
                    "Mapping preference",
                    null,
                    null,
                    "Use AutoMapper for declarative DTO mapping.",
                    0,
                    0,
                    false,
                    0.82m,
                    new MemoryProvenanceResponse(
                        "claude",
                        "session-1",
                        new DateTimeOffset(
                            2026,
                            7,
                            28,
                            12,
                            0,
                            0,
                            TimeSpan.Zero),
                        false,
                        null),
                    specificity,
                    selectors,
                    43,
                    ContextDecisionCodeConstants.Included)
            ];

            return new BuildContextResponse(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                null,
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                null,
                "https://example.test/espada.git",
                "src",
                "feature/context",
                agent,
                items,
                [],
                [],
                [],
                new ContextBudgetSummaryResponse(
                    4_096,
                    21,
                    88,
                    4_008,
                    3,
                    0));
        }
    }
}
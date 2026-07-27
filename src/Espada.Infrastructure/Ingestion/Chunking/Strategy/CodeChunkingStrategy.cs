using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Domain.Enums;

namespace Espada.Infrastructure.Ingestion.Chunking.Strategy;

internal sealed class CodeChunkingStrategy() : BoundaryChunkingStrategy(ChunkingStrategyType.Code.Name, DefaultSeparators)
{
    private static readonly string[] DefaultSeparators =
    [
        "\nclass ", "\ninterface ", "\nrecord ", "\ndef ", "\nfunc ",
        "\nfn ", "\n\n", "\n", " "
    ];

    private static readonly string[] PythonSeparators = ["\nclass ", "\ndef ", "\n\n", "\n", " "];

    private static readonly string[] GoSeparators = ["\ntype ", "\nfunc ", "\n\n", "\n", " "];

    private static readonly string[] RustSeparators = ["\nstruct ", "\nenum ", "\nimpl ", "\nfn ", "\n\n", "\n", " "];

    private static readonly string[] ObjectOrientedSeparators = ["\nclass ", "\ninterface ", "\nrecord ", "\npublic ", "\nprivate ", "\n\n", "\n", " "];

    private static readonly string[] ScriptSeparators = ["\nclass ", "\ninterface ", "\nfunction ", "\nconst ", "\nexport ", "\n\n", "\n", " "];

    protected override IReadOnlyList<string> ResolveSeparators(ImportOptions options) =>
        options.CodeLanguage?.ToLowerInvariant() switch
        {
            CodeLanguageNames.Python => PythonSeparators,
            CodeLanguageNames.Go => GoSeparators,
            CodeLanguageNames.Rust => RustSeparators,
            CodeLanguageNames.Java or
            CodeLanguageNames.CSharp or
            CodeLanguageNames.CSharpShort => ObjectOrientedSeparators,
            CodeLanguageNames.TypeScript or
            CodeLanguageNames.JavaScript or
            CodeLanguageNames.TypeScriptShort or
            CodeLanguageNames.JavaScriptShort => ScriptSeparators,
            _ => base.ResolveSeparators(options)
        };

    private static class CodeLanguageNames
    {
        public const string Python = "python";
        public const string Go = "go";
        public const string Rust = "rust";
        public const string Java = "java";
        public const string CSharp = "csharp";
        public const string CSharpShort = "cs";
        public const string TypeScript = "typescript";
        public const string JavaScript = "javascript";
        public const string TypeScriptShort = "ts";
        public const string JavaScriptShort = "js";
    }
}
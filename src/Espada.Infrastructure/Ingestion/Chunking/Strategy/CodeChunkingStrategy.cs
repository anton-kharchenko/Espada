using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Domain.Enums;
using Espada.Infrastructure.Constants;

namespace Espada.Infrastructure.Ingestion.Chunking.Strategy
{
    internal sealed class CodeChunkingStrategy()
        : BoundaryChunkingStrategy(ChunkingStrategyType.Code.Name, DefaultSeparators)
    {
        private static readonly string[] DefaultSeparators =
        [
            "\nclass ", "\ninterface ", "\nrecord ", "\ndef ", "\nfunc ",
            "\nfn ", "\n\n", "\n", " "
        ];

        private static readonly string[] PythonSeparators = ["\nclass ", "\ndef ", "\n\n", "\n", " "];

        private static readonly string[] GoSeparators = ["\ntype ", "\nfunc ", "\n\n", "\n", " "];

        private static readonly string[] RustSeparators =
            ["\nstruct ", "\nenum ", "\nimpl ", "\nfn ", "\n\n", "\n", " "];

        private static readonly string[] ObjectOrientedSeparators =
            ["\nclass ", "\ninterface ", "\nrecord ", "\npublic ", "\nprivate ", "\n\n", "\n", " "];

        private static readonly string[] ScriptSeparators =
            ["\nclass ", "\ninterface ", "\nfunction ", "\nconst ", "\nexport ", "\n\n", "\n", " "];

        protected override IReadOnlyList<string> ResolveSeparators(ImportOptions options)
        {
            return options.CodeLanguage?.ToLowerInvariant() switch
            {
                CodeLanguageNameConstants.Python => PythonSeparators,
                CodeLanguageNameConstants.Go => GoSeparators,
                CodeLanguageNameConstants.Rust => RustSeparators,
                CodeLanguageNameConstants.Java or
                    CodeLanguageNameConstants.CSharp or
                    CodeLanguageNameConstants.CSharpShort => ObjectOrientedSeparators,
                CodeLanguageNameConstants.TypeScript or
                    CodeLanguageNameConstants.JavaScript or
                    CodeLanguageNameConstants.TypeScriptShort or
                    CodeLanguageNameConstants.JavaScriptShort => ScriptSeparators,
                _ => base.ResolveSeparators(options)
            };
        }
    }
}
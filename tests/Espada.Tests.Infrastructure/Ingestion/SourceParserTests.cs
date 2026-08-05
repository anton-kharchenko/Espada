using Espada.Application.Constants;
using Espada.Application.Enums;
using Espada.Application.Exceptions;
using Espada.Infrastructure.Ingestion;
using Espada.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Text;

namespace Espada.Tests.Infrastructure.Ingestion
{
    public sealed class SourceParserTests
    {
        private readonly SourceParser _parser = new(Options.Create(new IngestionOptions()));

        public static TheoryData<string> LegacyOfficeFileNames =>
            new()
            {
                $"legacy{IngestionFileExtensionConstants.LegacyDocument}",
                $"legacy{IngestionFileExtensionConstants.LegacySpreadsheet}",
                $"legacy{IngestionFileExtensionConstants.LegacyPresentation}"
            };

        [Fact]
        public async Task Html_ShouldRemoveExecutableAndStyleContent()
        {
            await using MemoryStream content =
                new(
                    "<html><style>.hidden{}</style><body><h1>Title</h1><script>secret()</script><p>Body</p></body></html>"u8
                        .ToArray());

            string extracted = await _parser.ParseAsync(content, "page.html", "text/html",
                TestContext.Current.CancellationToken);

            Assert.Equal("Title Body", extracted);
        }

        [Fact]
        public async Task Json_ShouldProduceDeterministicIndentedText()
        {
            await using MemoryStream content = new(Encoding.UTF8.GetBytes("""{"answer":42}"""));

            string extracted = await _parser.ParseAsync(
                content,
                "data.json",
                "application/json",
                TestContext.Current.CancellationToken);

            Assert.Equal("{\n  \"answer\": 42\n}", extracted.Replace("\r\n", "\n"));
        }

        [Theory]
        [MemberData(nameof(LegacyOfficeFileNames))]
        public async Task LegacyOfficeFormats_ShouldFailPermanently(string fileName)
        {
            await using MemoryStream content = new([1, 2, 3]);

            IngestionException exception = await Assert.ThrowsAsync<IngestionException>(() => _parser.ParseAsync(
                content,
                fileName,
                IngestionMediaTypeConstants.Binary,
                TestContext.Current.CancellationToken));

            Assert.Equal(JobFailureCategoryType.Permanent, exception.Category);
            Assert.Equal(IngestionFailureCodeConstants.UnsupportedFormat, exception.Code);
        }
    }
}
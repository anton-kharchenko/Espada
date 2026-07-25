namespace Espada.Tests.Application.TestData
{
    public static class StringTheoryData
    {
        public static TheoryData<string?> NullOrWhiteSpaceValues =>
        [
            null!,
            string.Empty,
            " ",
            "    ",
            "\t",
            "\r\n"
        ];
    }
}
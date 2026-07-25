namespace Espada.Tests.Api.Contracts.TestData;

internal static class StringTheoryData
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
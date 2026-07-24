namespace Espada.Tests.Domain.TestData;

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
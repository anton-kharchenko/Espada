using Espada.Comms.Core.Pagination;

namespace Espada.Tests.Comms.Core.Pagination;

public sealed class CursorCodecTests
{
    public static TheoryData<string> RoundTripValues =>
        new()
        {
            "workspace:42",
            "revision/Привет"
        };

    public static TheoryData<string?> InvalidCursors =>
        new()
        {
            null!,
            string.Empty,
            " ",
            "a",
            "%%%",
            "____"
        };

    [Theory]
    [MemberData(nameof(RoundTripValues))]
    public void Encode_ThenDecode_ReturnsOriginalValue(
        string value)
    {
        string cursor = CursorCodec.Encode(value);

        bool decoded = CursorCodec.TryDecode(
            cursor,
            out string? decodedValue);

        Assert.True(decoded);
        Assert.Equal(value, decodedValue);
        Assert.DoesNotContain("=", cursor, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(InvalidCursors))]
    public void TryDecode_WithInvalidCursor_ReturnsFalse(
        string? cursor)
    {
        bool decoded = CursorCodec.TryDecode(
            cursor,
            out string? value);

        Assert.False(decoded);
        Assert.Null(value);
    }
}
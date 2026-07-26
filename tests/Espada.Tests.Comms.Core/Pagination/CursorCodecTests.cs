using Espada.Comms.Core.Pagination;

namespace Espada.Tests.Comms.Core.Pagination;

public sealed class CursorCodecTests
{
    [Theory]
    [InlineData("workspace:42")]
    [InlineData("revision/Привет")]
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
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("a")]
    [InlineData("%%%")]
    [InlineData("____")]
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
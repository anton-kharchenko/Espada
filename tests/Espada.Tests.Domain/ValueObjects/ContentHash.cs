namespace Espada.Tests.Domain.ValueObjects;

public sealed class ContentHashTests
{
    [Fact]
    public void FromUtf8_WithSameContent_ShouldReturnSameHash()
    {
        // Act
        ContentHash first = ContentHash.FromUtf8("Espada");

        ContentHash second = ContentHash.FromUtf8("Espada");

        // Assert
        first.Should().Be(second);
    }

    [Fact]
    public void FromUtf8_WithDifferentContent_ShouldReturnDifferentHash()
    {
        // Act
        ContentHash first = ContentHash.FromUtf8("Espada");

        ContentHash second = ContentHash.FromUtf8("Another value");

        // Assert
        first.Should().NotBe(second);
    }

    [Fact]
    public void FromUtf8_ShouldReturnLowercaseSha256Hex()
    {
        // Act
        ContentHash hash = ContentHash.FromUtf8("Espada");

        // Assert
        hash.Value.Should().HaveLength(64);
        hash.Value.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void Create_WithUppercaseHash_ShouldNormalizeToLowercase()
    {
        // Arrange
        string value = new('A', 64);

        // Act
        ContentHash hash = ContentHash.Create(value);

        // Assert
        hash.Value.Should().Be(new string('a', 64));
    }
}
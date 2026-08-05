using Espada.Mcp.Security;

namespace Espada.Tests.Mcp.Security;

public sealed class McpAuthorizationOptionsTests
{
    [Fact]
    public void IsValid_WithLoopbackHttpAuthority_ShouldSucceed()
    {
        McpAuthorizationOptions options = new();

        Assert.True(options.IsValid());
    }

    [Fact]
    public void IsValid_WithNonLoopbackHttpAuthority_ShouldFail()
    {
        McpAuthorizationOptions options = new()
        {
            Issuer = new Uri("http://mcp.example.test/"),
            Resource = new Uri("http://mcp.example.test/mcp")
        };

        Assert.False(options.IsValid());
    }

    [Fact]
    public void IsValid_WithHttpsAuthorityWithoutCertificates_ShouldFail()
    {
        McpAuthorizationOptions options = new()
        {
            Issuer = new Uri("https://localhost/"),
            Resource = new Uri("https://localhost/mcp")
        };

        Assert.False(options.IsValid());
    }

    [Fact]
    public void IsValid_WithLoopbackHttpsAuthorityAndCertificates_ShouldSucceed()
    {
        McpAuthorizationOptions options = new()
        {
            Issuer = new Uri("https://localhost/"),
            Resource = new Uri("https://localhost/mcp"),
            SigningCertificateThumbprint = "signing",
            EncryptionCertificateThumbprint = "encryption"
        };

        Assert.True(options.IsValid());
    }

    [Fact]
    public void IsValid_WithCloudAuthorityWithoutEntra_ShouldFail()
    {
        McpAuthorizationOptions options = new()
        {
            Issuer = new Uri("https://mcp.example.test/"),
            Resource = new Uri("https://mcp.example.test/mcp"),
            SigningCertificateThumbprint = "signing",
            EncryptionCertificateThumbprint = "encryption"
        };

        Assert.False(options.IsValid());
    }

    [Fact]
    public void IsValid_WithCloudAuthorityEntraAndCertificates_ShouldSucceed()
    {
        McpAuthorizationOptions options = new()
        {
            Issuer = new Uri("https://mcp.example.test/"),
            Resource = new Uri("https://mcp.example.test/mcp"),
            EntraAuthority = "https://login.microsoftonline.com/tenant/v2.0",
            EntraClientId = "client",
            EntraClientSecret = "secret",
            SigningCertificateThumbprint = "signing",
            EncryptionCertificateThumbprint = "encryption"
        };

        Assert.True(options.IsValid());
    }

    [Fact]
    public void IsValid_WithCloudAuthorityEntraAndBase64Certificates_ShouldSucceed()
    {
        McpAuthorizationOptions options = new()
        {
            Issuer = new Uri("https://mcp.example.test/"),
            Resource = new Uri("https://mcp.example.test/mcp"),
            EntraAuthority = "https://login.microsoftonline.com/tenant/v2.0",
            EntraClientId = "client",
            EntraClientSecret = "secret",
            SigningCertificateBase64 = "signing",
            EncryptionCertificateBase64 = "encryption"
        };

        Assert.True(options.IsValid());
    }

    [Fact]
    public void IsValid_WithSameBase64CertificateForBothPurposes_ShouldFail()
    {
        McpAuthorizationOptions options = new()
        {
            Issuer = new Uri("https://localhost/"),
            Resource = new Uri("https://localhost/mcp"),
            SigningCertificateBase64 = "certificate",
            EncryptionCertificateBase64 = "certificate"
        };

        Assert.False(options.IsValid());
    }
}
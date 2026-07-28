namespace Espada.Infrastructure.Security
{
    public sealed record BootstrapIdentity(
        string IdentityIssuer,
        string IdentitySubject);
}
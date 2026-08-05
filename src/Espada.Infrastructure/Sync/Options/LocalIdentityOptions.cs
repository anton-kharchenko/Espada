namespace Espada.Infrastructure.Sync.Options
{
    internal sealed class LocalIdentityOptions
    {
        public const string SectionName = "WebConsole";

        public string LocalIdentityIssuer { get; set; } = "espada:local";

        public string LocalIdentitySubject { get; set; } =
            Environment.UserName;
    }
}
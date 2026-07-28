namespace Espada.Api.Authentication
{
    internal sealed class WebConsoleOptions
    {
        public const string SectionName = "WebConsole";

        public WebConsoleMode Mode { get; set; } = WebConsoleMode.Local;

        public string LocalIdentityIssuer { get; set; } = "espada:local";

        public string LocalIdentitySubject { get; set; } =
            Environment.UserName;

        public bool IsValid()
        {
            return Enum.IsDefined(Mode)
                   && !string.IsNullOrWhiteSpace(LocalIdentityIssuer)
                   && !string.IsNullOrWhiteSpace(LocalIdentitySubject);
        }
    }
}

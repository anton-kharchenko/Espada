namespace Espada.Api.Authentication.Constants
{
    internal static class WebConsoleAuthenticationConstants
    {
        public const string CookieScheme = "Espada.Console";

        public const string CloudOpenIdConnectScheme =
            "Espada.Console.Entra";

        public const string AccessPolicy = "Espada.Console.Access";

        public const string CookieName = "Espada.Console.Session";

        public const string AntiforgeryCookieName =
            "Espada.Console.Antiforgery";

        public const string AntiforgeryRequestCookieName =
            "Espada.Console.Csrf";

        public const string AntiforgeryHeaderName = "X-CSRF-TOKEN";

        public const string IdentityIssuerClaim = "iss";

        public const string IdentitySubjectClaim = "sub";

        public const string SessionIdentityClaim =
            "espada:console_session_id";
    }
}
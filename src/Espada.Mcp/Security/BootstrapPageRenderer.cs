using System.Net;

namespace Espada.Mcp.Security
{
    internal static class BootstrapPageRenderer
    {
        public static string Render(string nonce)
        {
            string encodedNonce = WebUtility.HtmlEncode(nonce);
            return $$"""
                     <!doctype html>
                     <html lang="en">
                     <head>
                         <meta charset="utf-8">
                         <meta name="viewport" content="width=device-width,initial-scale=1">
                         <title>Espada local authorization</title>
                         <style>
                             :root { color-scheme: light dark; font: 16px/1.5 system-ui, sans-serif; }
                             body { display: grid; min-height: 100vh; margin: 0; place-items: center; }
                             main { max-width: 34rem; padding: 2rem; }
                             button { font: inherit; padding: .65rem 1rem; }
                         </style>
                     </head>
                     <body>
                         <main>
                             <h1>Authorize this local Espada session</h1>
                             <p>The link is single-use and expires in five minutes.</p>
                             <form method="post">
                                 <input id="code" name="code" type="hidden">
                                 <input id="returnUrl" name="returnUrl" type="hidden">
                                 <button type="submit">Continue</button>
                             </form>
                             <p id="error" hidden>The bootstrap link is incomplete.</p>
                         </main>
                         <script nonce="{{encodedNonce}}">
                             const values = new URLSearchParams(location.hash.slice(1));
                             const code = values.get("code");
                             if (code) {
                                 document.getElementById("code").value = code;
                                 document.getElementById("returnUrl").value =
                                     values.get("returnUrl") || "";
                             } else {
                                 document.querySelector("button").disabled = true;
                                 document.getElementById("error").hidden = false;
                             }
                             history.replaceState(null, "", location.pathname);
                         </script>
                     </body>
                     </html>
                     """;
        }
    }
}
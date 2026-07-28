namespace Espada.Api.Authentication
{
    internal static class WebConsoleBootstrapPageRenderer
    {
        public static string Render(string nonce)
        {
            return $$"""
                <!doctype html>
                <html lang="en">
                <head>
                  <meta charset="utf-8">
                  <meta name="viewport" content="width=device-width,initial-scale=1">
                  <title>Opening Espada</title>
                  <style>
                    :root { color-scheme: light dark; font-family: system-ui, sans-serif; }
                    body { display: grid; min-height: 100vh; margin: 0; place-items: center; }
                    main { max-width: 32rem; padding: 2rem; text-align: center; }
                  </style>
                </head>
                <body>
                  <main>
                    <h1>Opening Espada Console</h1>
                    <p id="status">Creating your local browser session…</p>
                  </main>
                  <script nonce="{{nonce}}">
                    const parameters = new URLSearchParams(location.hash.slice(1));
                    const body = new URLSearchParams({
                      code: parameters.get('code') ?? '',
                      returnUrl: parameters.get('returnUrl') ?? '/app'
                    });
                    history.replaceState(null, '', location.pathname);
                    fetch(location.pathname, {
                      method: 'POST',
                      credentials: 'same-origin',
                      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                      body
                    }).then(response => {
                      if (!response.ok) throw new Error('bootstrap_failed');
                      location.replace(response.redirected ? response.url : '/app');
                    }).catch(() => {
                      document.getElementById('status').textContent =
                        'The link is invalid, expired, or already used. Create a new link from the Espada CLI.';
                    });
                  </script>
                </body>
                </html>
                """;
        }
    }
}
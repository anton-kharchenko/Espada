using Espada.Mcp.Constants;
using Espada.Mcp.Models;
using OpenIddict.Abstractions;
using System.Net;

namespace Espada.Mcp.Security
{
    internal static class AuthorizationConsentPageRenderer
    {
        public static string RenderSessionRequired()
        {
            return """
                   <!doctype html>
                   <html lang="en">
                   <head>
                       <meta charset="utf-8">
                       <meta name="viewport" content="width=device-width,initial-scale=1">
                       <title>Local authorization required · Espada</title>
                       <style>
                           :root { color-scheme: light dark; font: 16px/1.5 system-ui, sans-serif; }
                           body { display: grid; min-height: 100vh; margin: 0; place-items: center; }
                           main { max-width: 38rem; padding: 2rem; }
                           code { font-family: ui-monospace, monospace; }
                       </style>
                   </head>
                   <body>
                       <main>
                           <h1>Local authorization required</h1>
                           <p>Create and open a one-time link with <code>espada auth bootstrap</code>, then retry this authorization request.</p>
                       </main>
                   </body>
                   </html>
                   """;
        }

        public static string Render(
            McpAuthorizationGrant grant,
            OpenIddictRequest request,
            string antiforgeryToken)
        {
            string workspace = grant.WorkspaceId?.ToString("D")
                               ?? "Create a workspace";
            string scopes = string.Join(
                Environment.NewLine,
                grant.Scopes.Select(scope => $"<li>{WebUtility.HtmlEncode(scope)}</li>"));
            return $$"""
                     <!doctype html>
                     <html lang="en">
                     <head>
                         <meta charset="utf-8">
                         <meta name="viewport" content="width=device-width,initial-scale=1">
                         <title>Authorize {{WebUtility.HtmlEncode(grant.ClientId)}} · Espada</title>
                         <style>
                             :root { color-scheme: light dark; font: 16px/1.5 system-ui, sans-serif; }
                             body { display: grid; min-height: 100vh; margin: 0; place-items: center; }
                             main { max-width: 38rem; padding: 2rem; }
                             .actions { display: flex; gap: .75rem; margin-top: 1.5rem; }
                             button { font: inherit; padding: .65rem 1rem; }
                         </style>
                     </head>
                     <body>
                         <main>
                             <h1>Authorize MCP client</h1>
                             <p><strong>Client:</strong> {{WebUtility.HtmlEncode(grant.ClientId)}}</p>
                             <p><strong>Workspace:</strong> {{WebUtility.HtmlEncode(workspace)}}</p>
                             <p>The client requests:</p>
                             <ul>{{scopes}}</ul>
                             <form method="post">
                                 {{RenderHiddenInput("client_id", request.ClientId)}}
                                 {{RenderHiddenInput("redirect_uri", request.RedirectUri)}}
                                 {{RenderHiddenInput("response_type", request.ResponseType)}}
                                 {{RenderHiddenInput("scope", request.Scope)}}
                                 {{RenderHiddenInput("state", request.State)}}
                                 {{RenderHiddenInput("code_challenge", request.CodeChallenge)}}
                                 {{RenderHiddenInput("code_challenge_method", request.CodeChallengeMethod)}}
                                 {{RenderHiddenInput("resource", request.GetParameter("resource").ToString())}}
                                 {{RenderHiddenInput("workspace_id", request.GetParameter("workspace_id").ToString())}}
                                 <input type="hidden"
                                        name="{{McpAuthorizationConstants.AntiforgeryFieldName}}"
                                        value="{{WebUtility.HtmlEncode(antiforgeryToken)}}">
                                 <div class="actions">
                                     <button name="decision" value="allow" type="submit">Allow</button>
                                     <button name="decision" value="deny" type="submit">Deny</button>
                                 </div>
                             </form>
                         </main>
                     </body>
                     </html>
                     """;
        }

        private static string RenderHiddenInput(
            string name,
            string? value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : $"<input type=\"hidden\" name=\"{WebUtility.HtmlEncode(name)}\" value=\"{WebUtility.HtmlEncode(value)}\">";
        }
    }
}
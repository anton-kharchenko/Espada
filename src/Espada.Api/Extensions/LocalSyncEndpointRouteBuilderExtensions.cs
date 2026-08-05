using Espada.Infrastructure.Sync.Authentication;
using Espada.Infrastructure.Sync.Client;
using Espada.Infrastructure.Sync.Contracts;
using Espada.Infrastructure.Sync.Options;
using Microsoft.Extensions.Options;

namespace Espada.Api.Extensions
{
    internal static class LocalSyncEndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder MapLocalSyncEndpoints(this IEndpointRouteBuilder endpoints)
        {
            RouteGroupBuilder group = endpoints.MapGroup("/api/v1.0")
                .WithTags("Local Sync");
            group.MapPost("/auth/login", BeginLogin)
                .RequireAuthorization();
            group.MapGet("/auth/callback", CompleteLogin)
                .AllowAnonymous();
            group.MapPost("/sync", RunSync)
                .RequireAuthorization();
            return endpoints;
        }

        private static IResult BeginLogin(HttpRequest request, ISyncAuthorizationService authorization,
            IOptions<SyncClientOptions> options)
        {
            if (!options.Value.IsConfigured())
            {
                return Results.Conflict(new { message = "Espada Cloud sync is not configured." });
            }

            Uri redirectUri = new($"{request.Scheme}://{request.Host}/api/v1.0/auth/callback");
            return Results.Ok(new { authorizationUrl = authorization.Begin(redirectUri).ToString() });
        }

        private static async Task<IResult> CompleteLogin(string? state, string? code, string? error,
            ISyncAuthorizationService authorization, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                return Results.Text("Espada Cloud sign-in was cancelled. You can close this window.",
                    "text/plain; charset=utf-8");
            }

            if (string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(code))
            {
                return Results.BadRequest("The authorization callback is incomplete.");
            }

            try
            {
                await authorization.CompleteAsync(state, code, cancellationToken);
                return Results.Text("Espada Cloud sign-in completed. You can close this window.",
                    "text/plain; charset=utf-8");
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(exception.Message);
            }
        }

        private static async Task<IResult> RunSync(ISyncClientService syncClient,
            CancellationToken cancellationToken)
        {
            if (!syncClient.IsConfigured)
            {
                return Results.Conflict(new { message = "Espada Cloud sync is not configured." });
            }

            try
            {
                return Results.Ok(await syncClient.RunAsync(cancellationToken));
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (HttpRequestException exception)
            {
                return Results.Problem(exception.Message, statusCode: StatusCodes.Status502BadGateway);
            }
        }
    }
}
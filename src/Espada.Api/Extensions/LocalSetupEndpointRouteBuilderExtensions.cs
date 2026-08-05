using Espada.Api.Authentication.Constants;
using Espada.Domain.Rules;
using Espada.LocalSetup.Contracts;
using Espada.LocalSetup.Contracts.Requests;
using Espada.LocalSetup.Contracts.Responses;
using System.Security.Claims;

namespace Espada.Api.Extensions
{
    internal static class LocalSetupEndpointRouteBuilderExtensions
    {
        public static RouteGroupBuilder MapLocalSetupEndpoints(this RouteGroupBuilder protectedBff)
        {
            RouteGroupBuilder setup = protectedBff.MapGroup("/setup");
            setup.MapGet("/preview", PreviewAsync).Produces<LocalSetupPreviewResponse>();
            setup.MapPost("/commit", CommitAsync).Produces<LocalSetupCommitResponse>();
            return protectedBff;
        }

        private static async Task<IResult> PreviewAsync(string path, ILocalSetupService service,
            CancellationToken cancellationToken)
        {
            try
            {
                return Results.Ok(await service.PreviewAsync(path, cancellationToken));
            }
            catch (Exception exception) when (exception is ArgumentException or DirectoryNotFoundException
                                              or InvalidOperationException)
            {
                return Results.BadRequest(new { code = "local_setup_invalid", message = exception.Message });
            }
        }

        private static async Task<IResult> CommitAsync(CommitLocalSetupRequest request, ClaimsPrincipal user,
            ILocalSetupService service, CancellationToken cancellationToken)
        {
            string? issuer = user.FindFirstValue(WebConsoleAuthenticationConstants.IdentityIssuerClaim);
            string? subject = user.FindFirstValue(WebConsoleAuthenticationConstants.IdentitySubjectClaim);
            if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(subject))
            {
                return Results.Unauthorized();
            }

            try
            {
                DomainResult<LocalSetupCommitResponse> result = await service.CommitAsync(request,
                    request.RepositoryPath, issuer, subject, cancellationToken);
                return WebConsoleResults.From(result);
            }
            catch (Exception exception) when (exception is ArgumentException or DirectoryNotFoundException
                                              or InvalidOperationException)
            {
                return Results.BadRequest(new { code = "local_setup_invalid", message = exception.Message });
            }
        }
    }
}
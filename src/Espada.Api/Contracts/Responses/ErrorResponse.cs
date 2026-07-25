using Espada.Domain.Rules;

namespace Espada.Api.Contracts.Responses;

public sealed record ErrorResponse(string Code, string Description)
{
    public static ErrorResponse FromError(DomainError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new ErrorResponse(error.Code, error.Description);
    }
}
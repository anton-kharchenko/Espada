using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Espada.Application.Behaviors
{
    internal sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            IValidator<TRequest>[] validatorArray = validators.ToArray();

            if (validatorArray.Length == 0)
            {
                return await next(cancellationToken);
            }

            ValidationContext<TRequest> context = new(request);

            ValidationResult[] validationResults =
                await Task.WhenAll(validatorArray.Select(validator =>
                    validator.ValidateAsync(context, cancellationToken)));

            ValidationFailure[] failures = validationResults
                .SelectMany(result => result.Errors)
                .Where(failure => failure is not null)
                .ToArray();

            if (failures.Length != 0)
            {
                throw new ValidationException(failures);
            }

            return await next(cancellationToken);
        }
    }
}
using Espada.Domain.Rules;
using MediatR;

namespace Espada.Application.Contracts.Messaging
{
    public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, DomainResult<TResponse>>
        where TQuery : IQuery<TResponse>;
}
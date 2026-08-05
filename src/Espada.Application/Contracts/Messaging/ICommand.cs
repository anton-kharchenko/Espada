using Espada.Domain.Rules;
using MediatR;

namespace Espada.Application.Contracts.Messaging
{
    public interface ICommand : IRequest<DomainResult>;

    public interface ICommand<TResponse> : IRequest<DomainResult<TResponse>>;
}
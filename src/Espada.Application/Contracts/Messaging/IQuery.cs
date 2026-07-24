using Espada.Domain.Rules;
using MediatR;

namespace Espada.Application.Contracts.Messaging;

public interface IQuery<TResponse> : IRequest<DomainResult<TResponse>>;
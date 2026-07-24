using Espada.Domain.Rules;
using MediatR;

namespace Espada.Application.Contracts.Messaging;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, DomainResult> where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, DomainResult<TResponse>> where TCommand : ICommand<TResponse>;
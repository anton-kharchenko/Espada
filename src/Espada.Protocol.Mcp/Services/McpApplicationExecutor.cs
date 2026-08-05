using AutoMapper;
using Espada.Application.Contracts.Security;
using Espada.Application.Models;
using Espada.Application.Policies;
using Espada.Domain.Rules;
using MediatR;
using System.Text.Json;

namespace Espada.Protocol.Mcp.Services
{
    public sealed class McpApplicationExecutor(
        ISender sender,
        IMapper mapper,
        IRequestPrincipalAccessor principalAccessor,
        WorkspaceAccessPolicy accessPolicy)
    {
        public RequestPrincipal Principal =>
            principalAccessor.Principal
            ?? throw McpErrorMapper.Unauthorized(
                "The MCP request principal was not initialized.");

        public void AuthorizeWorkspaceCreation()
        {
            McpErrorMapper.ThrowIfFailure(
                accessPolicy.AuthorizeWorkspaceCreation());
        }

        public async Task AuthorizeWorkspaceAsync(
            Guid workspaceId,
            string requiredScope,
            CancellationToken cancellationToken)
        {
            DomainResult result = await accessPolicy.AuthorizeWorkspaceAsync(
                workspaceId,
                requiredScope,
                cancellationToken);
            McpErrorMapper.ThrowIfFailure(result);
        }

        public TDestination Map<TDestination>(object source)
        {
            try
            {
                return mapper.Map<TDestination>(source);
            }
            catch (AutoMapperMappingException exception)
                when (exception.InnerException is
                          ArgumentException or JsonException)
            {
                throw McpErrorMapper.InvalidArgument(
                    exception.InnerException.Message);
            }
            catch (ArgumentException exception)
            {
                throw McpErrorMapper.InvalidArgument(exception.Message);
            }
            catch (JsonException exception)
            {
                throw McpErrorMapper.InvalidArgument(exception.Message);
            }
        }

        public async Task<TResponse> SendAsync<TResponse>(
            IRequest<DomainResult<TResponse>> request,
            CancellationToken cancellationToken)
        {
            DomainResult<TResponse> result = await sender.Send(
                request,
                cancellationToken);
            McpErrorMapper.ThrowIfFailure(result);
            return result.Value;
        }

        public async Task SendAsync(
            IRequest<DomainResult> request,
            CancellationToken cancellationToken)
        {
            DomainResult result = await sender.Send(request, cancellationToken);
            McpErrorMapper.ThrowIfFailure(result);
        }
    }
}
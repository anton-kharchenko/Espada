using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace
{
    internal sealed class CreateWorkspaceCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IOrganizationRepository organizationRepository,
        IWorkspaceMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork,
        IClockService clockService,
        IMapper mapper)
        : ICommandHandler<CreateWorkspaceCommand, CreateWorkspaceResponse>
    {
        public async Task<DomainResult<CreateWorkspaceResponse>> Handle(
            CreateWorkspaceCommand request,
            CancellationToken cancellationToken)
        {
            DomainResult<WorkspaceName> nameResult = WorkspaceName.Create(request.Name);
            if (nameResult.IsFailure)
            {
                return DomainResult.Failure<CreateWorkspaceResponse>(nameResult.Error);
            }

            OrganizationId? organizationId = null;
            if (request.OrganizationId.HasValue)
            {
                if (request.OrganizationId.Value == Guid.Empty)
                {
                    return DomainResult.Failure<CreateWorkspaceResponse>(
                        OrganizationApplicationErrors.InvalidId);
                }

                Organization? organization = await organizationRepository.GetByIdAsync(
                    OrganizationId.Create(request.OrganizationId.Value),
                    cancellationToken);
                if (organization is null)
                {
                    return DomainResult.Failure<CreateWorkspaceResponse>(
                        OrganizationApplicationErrors.NotFound(request.OrganizationId.Value));
                }

                organizationId = organization.Id;
            }

            WorkspaceId workspaceId = WorkspaceId.New();
            DomainResult<Workspace> workspaceResult = Workspace.Create(
                workspaceId,
                nameResult.Value,
                request.Type,
                organizationId,
                clockService.UtcNow);
            if (workspaceResult.IsFailure)
            {
                return DomainResult.Failure<CreateWorkspaceResponse>(workspaceResult.Error);
            }

            Workspace workspace = workspaceResult.Value;

            await workspaceRepository.AddAsync(workspace, cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.IdentityIssuer)
                && !string.IsNullOrWhiteSpace(request.IdentitySubject))
            {
                WorkspaceMembership owner = WorkspaceMembership.CreateOwner(
                    WorkspaceMembershipId.New(),
                    workspace.Id,
                    request.IdentityIssuer,
                    request.IdentitySubject,
                    clockService.UtcNow);

                await membershipRepository.AddAsync(owner, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DomainResult.Success(mapper.Map<CreateWorkspaceResponse>(workspace));
        }
    }
}
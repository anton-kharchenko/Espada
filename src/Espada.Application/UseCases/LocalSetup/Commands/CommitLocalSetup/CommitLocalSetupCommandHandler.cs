using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Espada.Domain.ValueObjects.SourceDefinitions;
using System.Security.Cryptography;
using System.Text;

namespace Espada.Application.UseCases.LocalSetup.Commands.CommitLocalSetup
{
    internal sealed class CommitLocalSetupCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IWorkspaceMembershipRepository membershipRepository,
        IProjectRepository projectRepository,
        ISourceRepository sourceRepository,
        IArtifactRepository artifactRepository,
        IArtifactRevisionRepository revisionRepository,
        IInstructionRuleRepository instructionRuleRepository,
        IBindingRepository bindingRepository,
        IDeviceRepository deviceRepository,
        IAgentProfileRepository profileRepository,
        IAgentInstallationRepository installationRepository,
        IUnitOfWork unitOfWork,
        IClockService clockService)
        : ICommandHandler<CommitLocalSetupCommand, CommitLocalSetupResponse>
    {
        public async Task<DomainResult<CommitLocalSetupResponse>> Handle(CommitLocalSetupCommand request,
            CancellationToken cancellationToken)
        {
            WorkspaceId workspaceId = WorkspaceId.Create(request.SetupId);
            ProjectId projectId = ProjectId.Create(CreateId(request.SetupId, "project"));
            SourceId sourceId = SourceId.Create(CreateId(request.SetupId, "repository-source"));
            Workspace? existing = await workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
            if (existing is not null)
            {
                return DomainResult.Success(new CommitLocalSetupResponse(workspaceId.Value, projectId.Value,
                    sourceId.Value, true));
            }

            DomainResult<WorkspaceName> workspaceName = WorkspaceName.Create(request.WorkspaceName);
            if (workspaceName.IsFailure)
            {
                return DomainResult.Failure<CommitLocalSetupResponse>(workspaceName.Error);
            }

            DateTimeOffset createdAtUtc = clockService.UtcNow;
            Workspace workspace = Workspace.Create(workspaceId, workspaceName.Value, WorkspaceType.Personal, null,
                createdAtUtc).Value;
            DomainResult<Project> projectResult = Project.Create(projectId, workspaceId, request.ProjectName,
                request.CanonicalRemoteUri, [Path.GetFullPath(request.RepositoryRoot)], createdAtUtc);
            if (projectResult.IsFailure)
            {
                return DomainResult.Failure<CommitLocalSetupResponse>(projectResult.Error);
            }

            DomainResult<SourceName> sourceName = SourceName.Create($"{request.ProjectName} repository");
            if (sourceName.IsFailure)
            {
                return DomainResult.Failure<CommitLocalSetupResponse>(sourceName.Error);
            }

            RepositorySourceDefinition definition = new(projectId.Value.ToString("D"), request.CanonicalRemoteUri,
                new RepositoryScanPolicy());
            DomainResult<Source> sourceResult = Source.Create(sourceId, workspaceId, sourceName.Value, definition,
                createdAtUtc);
            if (sourceResult.IsFailure)
            {
                return DomainResult.Failure<CommitLocalSetupResponse>(sourceResult.Error);
            }

            await workspaceRepository.AddAsync(workspace, cancellationToken);
            await membershipRepository.AddAsync(WorkspaceMembership.CreateOwner(
                WorkspaceMembershipId.Create(CreateId(request.SetupId, "membership")), workspaceId,
                request.IdentityIssuer, request.IdentitySubject, createdAtUtc), cancellationToken);
            await projectRepository.AddAsync(projectResult.Value, cancellationToken);
            await sourceRepository.AddAsync(sourceResult.Value, cancellationToken);

            DomainResult initialInstruction = await AddInstructionAsync(request.SetupId, "initial",
                "Initial instructions", request.InitialInstruction, null, null, workspace, projectResult.Value,
                createdAtUtc, cancellationToken);
            if (initialInstruction.IsFailure)
            {
                return DomainResult.Failure<CommitLocalSetupResponse>(initialInstruction.Error);
            }

            for (int index = 0; index < request.Instructions.Count; index++)
            {
                LocalSetupInstructionInput instruction = request.Instructions[index];
                DomainResult imported = await AddInstructionAsync(request.SetupId, $"imported-{index}",
                    Path.GetFileName(instruction.RelativePath), instruction.Content,
                    NormalizeDirectory(instruction.RelativePath), instruction.Agent, workspace, projectResult.Value,
                    createdAtUtc, cancellationToken);
                if (imported.IsFailure)
                {
                    return DomainResult.Failure<CommitLocalSetupResponse>(imported.Error);
                }
            }

            DeviceId deviceId = DeviceId.Create(request.DeviceId);
            Device? device = await deviceRepository.GetByIdAsync(deviceId, cancellationToken);
            if (device is null)
            {
                DomainResult<Device> deviceResult = Device.Create(deviceId, request.DeviceName, createdAtUtc);
                if (deviceResult.IsFailure)
                {
                    return DomainResult.Failure<CommitLocalSetupResponse>(deviceResult.Error);
                }

                await deviceRepository.AddAsync(deviceResult.Value, cancellationToken);
            }

            foreach (LocalSetupAgentInput agent in request.Agents)
            {
                LocalSetupAgentInput agent1 = agent;
                AgentVendorType? vendor = Enumeration.GetAll<AgentVendorType>()
                    .SingleOrDefault(candidate => candidate.Id == agent1.VendorId);
                if (vendor is null)
                {
                    return DomainResult.Failure<CommitLocalSetupResponse>(new DomainError(
                        "LocalSetup.AgentVendor.Unsupported", $"Agent vendor ID {agent.VendorId} is not supported."));
                }

                DomainResult<AgentProfile> profile = AgentProfile.Create(
                    AgentProfileId.Create(CreateId(request.SetupId, $"profile-{vendor.Id}")), workspaceId, vendor,
                    $"{vendor.Name} default", "{}", createdAtUtc);
                DomainResult<AgentInstallation> installation = AgentInstallation.Create(
                    AgentInstallationId.Create(CreateId(request.DeviceId, $"installation-{vendor.Id}")), deviceId,
                    vendor, agent.ExecutablePath, agent.Version, agent.IsAuthenticated, createdAtUtc);
                if (profile.IsFailure || installation.IsFailure)
                {
                    return DomainResult.Failure<CommitLocalSetupResponse>(
                        profile.IsFailure ? profile.Error : installation.Error);
                }

                await profileRepository.AddAsync(profile.Value, cancellationToken);
                await installationRepository.AddAsync(installation.Value, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return DomainResult.Success(new CommitLocalSetupResponse(workspaceId.Value, projectId.Value,
                sourceId.Value, false));
        }

        private async Task<DomainResult> AddInstructionAsync(Guid setupId, string key, string title, string content,
            string? relativePath, string? agent, Workspace workspace, Project project, DateTimeOffset createdAtUtc,
            CancellationToken cancellationToken)
        {
            DomainResult<ArtifactTitle> artifactTitle = ArtifactTitle.Create(title);
            DomainResult<ArtifactContent> artifactContent = ArtifactContent.Create(content);
            DomainResult<RuleKey> ruleKey = RuleKey.Create($"setup.instruction.{key}");
            if (artifactTitle.IsFailure || artifactContent.IsFailure || ruleKey.IsFailure)
            {
                return DomainResult.Failure(artifactTitle.IsFailure
                    ? artifactTitle.Error
                    : artifactContent.IsFailure ? artifactContent.Error : ruleKey.Error);
            }

            ArtifactId artifactId = ArtifactId.Create(CreateId(setupId, $"artifact-{key}"));
            Artifact artifact = Artifact.Create(artifactId, workspace.Id, artifactTitle.Value,
                ArtifactKindType.Instruction, ArtifactType.Markdown, createdAtUtc).Value;
            ArtifactRevision revision = artifact.CreateRevision(
                ArtifactRevisionId.Create(CreateId(setupId, $"revision-{key}")), artifactContent.Value, createdAtUtc)
                .Value;
            DomainResult<InstructionRule> rule = artifact.CreateInstructionRule(revision, ruleKey.Value, content,
                ContextPriority.Neutral);
            DomainResult<Binding> binding = artifact.CreateBinding(
                BindingId.Create(CreateId(setupId, $"binding-{key}")), revision, workspace, null, project,
                project.CanonicalRemoteUri, relativePath, null, null, agent, createdAtUtc);
            if (rule.IsFailure || binding.IsFailure)
            {
                return DomainResult.Failure(rule.IsFailure ? rule.Error : binding.Error);
            }

            await artifactRepository.AddAsync(artifact, cancellationToken);
            await revisionRepository.AddAsync(revision, cancellationToken);
            await instructionRuleRepository.AddRangeAsync([rule.Value], cancellationToken);
            await bindingRepository.UpsertAsync(binding.Value, cancellationToken);
            return DomainResult.Success();
        }

        private static string? NormalizeDirectory(string relativePath)
        {
            string? directory = Path.GetDirectoryName(relativePath)?.Replace('\\', '/');
            return string.IsNullOrWhiteSpace(directory) ? null : directory;
        }

        private static Guid CreateId(Guid namespaceId, string name)
        {
            byte[] namespaceBytes = namespaceId.ToByteArray();
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);
            byte[] value = new byte[namespaceBytes.Length + nameBytes.Length];
            namespaceBytes.CopyTo(value, 0);
            nameBytes.CopyTo(value, namespaceBytes.Length);
            byte[] hash = SHA256.HashData(value);
            hash[7] = (byte)((hash[7] & 0x0F) | 0x50);
            hash[8] = (byte)((hash[8] & 0x3F) | 0x80);
            return new Guid(hash.AsSpan(0, 16));
        }
    }
}
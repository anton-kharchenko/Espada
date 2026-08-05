using Espada.Api.LocalSetup.Models;
using Espada.Application.UseCases.LocalSetup.Commands.CommitLocalSetup;
using Espada.Domain.Rules;
using MediatR;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Espada.Api.LocalSetup.Services
{
    internal sealed class LocalSetupService(
        GitRepositoryInspector repositoryInspector,
        AgentDiscoveryService agentDiscovery,
        McpConfigurationPreviewService configurationPreview,
        ManagedMcpConfigurationWriter configurationWriter,
        LocalRuntimeConfigurationWriter runtimeConfigurationWriter,
        LocalDeviceIdentityStore deviceIdentity,
        IMediator mediator)
    {
        public async Task<LocalSetupPreviewResponse> PreviewAsync(string path, CancellationToken cancellationToken)
        {
            GitRepositorySnapshot repository = await repositoryInspector.InspectAsync(path, cancellationToken);
            IReadOnlyList<LocalSetupAgentPreview> agents = await agentDiscovery.DiscoverAsync(cancellationToken);
            string projectName = new DirectoryInfo(repository.Root).Name;
            LocalRuntimeStateSnapshot ports = ReadPorts();
            return new LocalSetupPreviewResponse(CreateSetupId(repository.Root), repository.Root, projectName,
                projectName, repository.CanonicalRemoteUri, repository.Instructions, agents,
                configurationPreview.Create(agents),
                new LocalSetupPortPreview(ports.ApiPort, ports.McpPort, ports.PostgresPort), true);
        }

        public async Task<DomainResult<LocalSetupCommitResponse>> CommitAsync(CommitLocalSetupRequest request,
            string repositoryPath, string issuer, string subject, CancellationToken cancellationToken)
        {
            LocalSetupPreviewResponse preview = await PreviewAsync(repositoryPath, cancellationToken);
            if (preview.SetupId != request.SetupId)
            {
                return DomainResult.Failure<LocalSetupCommitResponse>(new DomainError(
                    "LocalSetup.PreviewChanged", "The setup preview no longer matches the selected repository."));
            }

            runtimeConfigurationWriter.Validate(request, preview.Ports);
            HashSet<int> selectedVendorIds = request.AgentVendorIds.ToHashSet();
            LocalSetupAgentPreview[] selectedAgents = preview.Agents
                .Where(agent => selectedVendorIds.Contains(agent.VendorId) && agent.IsInstalled && agent.IsAuthenticated)
                .ToArray();
            CommitLocalSetupCommand command = new(preview.SetupId, deviceIdentity.GetOrCreate(),
                request.WorkspaceName, request.ProjectName, preview.RepositoryRoot, preview.CanonicalRemoteUri,
                request.InitialInstruction, issuer, subject, Environment.MachineName,
                preview.Instructions.Select(instruction => new LocalSetupInstructionInput(instruction.RelativePath,
                    instruction.Content, instruction.Agent)).ToArray(),
                selectedAgents.Select(agent => new LocalSetupAgentInput(agent.VendorId, agent.ExecutablePath!,
                    agent.Version, agent.IsAuthenticated)).ToArray());
            DomainResult<CommitLocalSetupResponse> result = await mediator.Send(command, cancellationToken);
            if (result.IsFailure)
            {
                return DomainResult.Failure<LocalSetupCommitResponse>(result.Error);
            }

            await runtimeConfigurationWriter.WriteAsync(request, preview.Ports, cancellationToken);
            IReadOnlyList<string> configuredAgents = request.ConfigureMcp
                ? await configurationWriter.WriteAsync(preview.McpConfigurations,
                    selectedAgents.Select(agent => agent.Vendor).ToHashSet(StringComparer.OrdinalIgnoreCase),
                    cancellationToken)
                : [];
            return DomainResult.Success(new LocalSetupCommitResponse(result.Value.WorkspaceId,
                result.Value.ProjectId, result.Value.RepositorySourceId, result.Value.AlreadyCompleted,
                configuredAgents));
        }

        private static Guid CreateSetupId(string repositoryRoot)
        {
            string normalized = Path.TrimEndingDirectorySeparator(Path.GetFullPath(repositoryRoot));
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(OperatingSystem.IsWindows()
                ? normalized.ToUpperInvariant()
                : normalized));
            hash[7] = (byte)((hash[7] & 0x0F) | 0x50);
            hash[8] = (byte)((hash[8] & 0x3F) | 0x80);
            return new Guid(hash.AsSpan(0, 16));
        }

        private static LocalRuntimeStateSnapshot ReadPorts()
        {
            string root = Environment.GetEnvironmentVariable("ESPADA_DATA_ROOT")
                ?? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Espada");
            string statePath = Path.Join(root, "runtime-state.json");
            return File.Exists(statePath)
                ? JsonSerializer.Deserialize<LocalRuntimeStateSnapshot>(File.ReadAllText(statePath),
                      new JsonSerializerOptions(JsonSerializerDefaults.Web))
                  ?? new LocalRuntimeStateSnapshot(7432, 7433, 5433)
                : new LocalRuntimeStateSnapshot(7432, 7433, 5433);
        }
    }
}

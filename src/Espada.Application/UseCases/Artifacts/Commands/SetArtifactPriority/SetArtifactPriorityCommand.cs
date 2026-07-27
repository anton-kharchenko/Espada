using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Artifacts.Commands.SetArtifactPriority;

public sealed record SetArtifactPriorityCommand(Guid WorkspaceId, Guid ArtifactId, int Priority) : ICommand;
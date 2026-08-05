using Espada.Application.Contracts.Repositories;
using Espada.Application.Models;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Domain.Rules;
using MediatR;

namespace Espada.Worker.Repositories
{
    internal sealed class RepositoryWatcherService(
        IServiceScopeFactory scopeFactory,
        ILogger<RepositoryWatcherService> logger) : BackgroundService
    {
        private static readonly TimeSpan Debounce = TimeSpan.FromMilliseconds(750);
        private static readonly TimeSpan RegistrationRefresh = TimeSpan.FromSeconds(30);
        private readonly Dictionary<Guid, RepositoryWatchState> _states = [];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DateTimeOffset nextRefreshUtc = DateTimeOffset.MinValue;
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
                    if (nowUtc >= nextRefreshUtc)
                    {
                        await RefreshAsync(stoppingToken);
                        nextRefreshUtc = nowUtc + RegistrationRefresh;
                    }

                    foreach (RepositoryWatchState state in _states.Values.ToArray())
                    {
                        if (state.TryTake(nowUtc, Debounce))
                        {
                            await ReconcileAsync(state.Registration, stoppingToken);
                        }
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(250), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            finally
            {
                foreach (RepositoryWatchState state in _states.Values)
                {
                    state.Dispose();
                }
            }
        }

        private async Task RefreshAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            IReadOnlyList<RepositoryWatchRegistration> registrations = await scope.ServiceProvider
                .GetRequiredService<IRepositoryWatchRegistrationStore>()
                .ListAsync(cancellationToken);
            HashSet<Guid> currentSourceIds = registrations.Select(registration => registration.SourceId).ToHashSet();
            foreach (Guid removed in _states.Keys.Where(sourceId => !currentSourceIds.Contains(sourceId)).ToArray())
            {
                _states.Remove(removed, out RepositoryWatchState? state);
                state?.Dispose();
            }

            foreach (RepositoryWatchRegistration registration in registrations)
            {
                if (_states.TryGetValue(registration.SourceId, out RepositoryWatchState? current) &&
                    current.Matches(registration))
                {
                    continue;
                }

                current?.Dispose();
                _states[registration.SourceId] = new RepositoryWatchState(registration);
            }
        }

        private async Task ReconcileAsync(RepositoryWatchRegistration registration,
            CancellationToken cancellationToken)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                DomainResult<RequestImportResponse> result = await scope.ServiceProvider.GetRequiredService<IMediator>()
                    .Send(new RequestImportCommand(registration.WorkspaceId, registration.SourceId,
                        $"watch:{registration.SourceId:N}", new ImportOptions()), cancellationToken);
                if (result.IsFailure)
                {
                    logger.LogWarning("Repository reconciliation failed for source {SourceId}: {ErrorCode}.",
                        registration.SourceId, result.Error.Code);
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Repository reconciliation crashed for source {SourceId}.",
                    registration.SourceId);
            }
        }
    }
}
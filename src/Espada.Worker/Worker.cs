using Espada.Application.Contracts.Ingestion;
using Espada.Application.Contracts.Jobs;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.Enums;
using Espada.Application.Exceptions;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Npgsql;
using Espada.Billing.Contracts;

namespace Espada.Worker;

public sealed class Worker(IServiceScopeFactory scopeFactory, ILogger<Worker> logger) : BackgroundService
{
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(2);

    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromMinutes(30)
    ];

    private readonly string _workerId = $"{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                bool didWork = await PublishOutboxAsync(stoppingToken);
                didWork |= await ProcessJobAsync(stoppingToken);
                didWork |= await ProcessPaymentEventAsync(stoppingToken);
                didWork |= await ReconcileUsageAsync(stoppingToken);
                if (!didWork)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(250), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ingestion worker loop failed.");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }

    private async Task<bool> ReconcileUsageAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        IUsageReconciliationProcessor? processor = scope.ServiceProvider.GetService<IUsageReconciliationProcessor>();
        return processor is not null && await processor.ProcessNextAsync(_workerId, cancellationToken);
    }

    private async Task<bool> ProcessPaymentEventAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        IStripeWebhookProcessor? processor = scope.ServiceProvider.GetService<IStripeWebhookProcessor>();
        return processor is not null && await processor.ProcessNextAsync(_workerId, cancellationToken);
    }

    private async Task<bool> PublishOutboxAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IOutboxPublisher>().PublishNextAsync(_workerId, cancellationToken);
    }

    private async Task<bool> ProcessJobAsync(CancellationToken stoppingToken)
    {
        IngestionJob? job;
        using (IServiceScope claimScope = scopeFactory.CreateScope())
        {
            job = await claimScope.ServiceProvider.GetRequiredService<IJobQueue>()
                .ClaimAsync(_workerId, LeaseDuration, stoppingToken);
        }

        if (job is null)
        {
            return false;
        }

        try
        {
            using IServiceScope executionScope = scopeFactory.CreateScope();
            await executionScope.ServiceProvider.GetRequiredService<IImportPipelineStageExecutor>().ExecuteAsync(job, stoppingToken);
            await executionScope.ServiceProvider.GetRequiredService<IJobQueue>().CompleteAsync(job.JobId, _workerId, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            await FinishFailedJobAsync(job, JobFailureCategoryType.Cancelled, "Import was cancelled.", stoppingToken);
        }
        catch (IngestionException exception)
        {
            await FinishFailedJobAsync(job, exception.Category, exception.Message, stoppingToken);
        }
        catch (Exception exception) when (IsTransient(exception))
        {
            await FinishFailedJobAsync(
                job,
                JobFailureCategoryType.Transient,
                "A transient dependency error interrupted the stage.",
                stoppingToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Poisoned ingestion job {JobId} at stage {Stage}.", job.JobId, job.Stage);
            await FinishFailedJobAsync(job, JobFailureCategoryType.Poison, "The job payload or stage handler was invalid.", stoppingToken);
        }

        return true;
    }

    private async Task FinishFailedJobAsync(IngestionJob job, JobFailureCategoryType category, string sanitizedError, CancellationToken cancellationToken)
    {
        JobFailureCategoryType effectiveCategory = category;
        DateTimeOffset availableAtUtc = DateTimeOffset.UtcNow;
        if (category == JobFailureCategoryType.Transient && job.Attempt <= RetryDelays.Length)
        {
            TimeSpan delay = RetryDelays[job.Attempt - 1];
            double jitter = Random.Shared.NextDouble() * 0.4 - 0.2;
            availableAtUtc += delay + TimeSpan.FromMilliseconds(delay.TotalMilliseconds * jitter);
        }
        else if (category == JobFailureCategoryType.Transient)
        {
            effectiveCategory = JobFailureCategoryType.Permanent;
            sanitizedError = "Transient retry limit was exhausted.";
        }

        using IServiceScope scope = scopeFactory.CreateScope();
        IJobQueue queue = scope.ServiceProvider.GetRequiredService<IJobQueue>();
        await queue.RetryAsync(job.JobId, _workerId, effectiveCategory, Sanitize(sanitizedError), availableAtUtc, cancellationToken);

        if (effectiveCategory != JobFailureCategoryType.Transient)
        {
            await MarkImportTerminalAsync(scope.ServiceProvider, job.ImportJobId, effectiveCategory, sanitizedError, cancellationToken);
        }
    }

    private static async Task MarkImportTerminalAsync(
        IServiceProvider serviceProvider,
        ImportJobId importJobId,
        JobFailureCategoryType category,
        string reason,
        CancellationToken cancellationToken)
    {
        IImportJobRepository repository = serviceProvider.GetRequiredService<IImportJobRepository>();
        ImportJob? importJob = await repository.GetByIdAsync(importJobId, cancellationToken);
        if (importJob is null
            || importJob.Status.Equals(ImportStatusType.Succeeded)
            || importJob.Status.Equals(ImportStatusType.Failed)
            || importJob.Status.Equals(ImportStatusType.Cancelled))
        {
            return;
        }

        IClockService clock = serviceProvider.GetRequiredService<IClockService>();
        if (category == JobFailureCategoryType.Cancelled)
        {
            importJob.Cancel(clock.UtcNow);
        }
        else
        {
            DomainResult<ImportFailure> failure = ImportFailure.Create(
                category == JobFailureCategoryType.Poison
                    ? "poison_message"
                    : "ingestion_failed",
                Sanitize(reason));
            if (failure.IsSuccess)
            {
                importJob.Fail(failure.Value, clock.UtcNow);
            }
        }

        await serviceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(cancellationToken);
        await serviceProvider.GetRequiredService<IJobQueue>().CancelPendingAsync(importJobId, cancellationToken);
    }

    private static bool IsTransient(Exception exception) =>
        exception is HttpRequestException or IOException or TimeoutException or NpgsqlException;

    private static string Sanitize(string value)
    {
        string normalized = value.Replace('\r', ' ').Replace('\n', ' ').Trim();
        return normalized.Length <= ImportFailure.ReasonMaxLength ? normalized : normalized[..ImportFailure.ReasonMaxLength];
    }
}
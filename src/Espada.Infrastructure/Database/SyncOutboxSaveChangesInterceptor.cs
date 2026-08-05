using Espada.Infrastructure.Models;
using Espada.Infrastructure.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Runtime.CompilerServices;

namespace Espada.Infrastructure.Database
{
    internal sealed class SyncOutboxSaveChangesInterceptor(
        SyncChangeSignal signal) : SaveChangesInterceptor
    {
        private readonly ConditionalWeakTable<DbContext, object> _pending = new();

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            DbContext? context = eventData.Context;
            if (context is not null
                && context.ChangeTracker.Entries<OutboxMessageRecord>()
                    .Any(entry => entry.State == EntityState.Added))
            {
                _pending.GetValue(context, static _ => new object());
            }

            return ValueTask.FromResult(result);
        }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null
                && _pending.Remove(eventData.Context))
            {
                signal.Set();
            }

            return ValueTask.FromResult(result);
        }

        public override Task SaveChangesFailedAsync(
            DbContextErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                _pending.Remove(eventData.Context);
            }

            return Task.CompletedTask;
        }
    }
}
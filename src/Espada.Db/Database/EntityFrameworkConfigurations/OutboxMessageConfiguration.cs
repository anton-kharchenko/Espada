using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessages>
    {
        public void Configure(EntityTypeBuilder<OutboxMessages> builder)
        {
            builder.Property(message => message.EventId).ValueGeneratedNever();
            builder.Property(message => message.PayloadJson).HasColumnType(DbJsonColumnTypeConstants.Jsonb);
            builder.HasIndex(message => new { message.ProcessedAtUtc, message.AvailableAtUtc });
        }
    }
}
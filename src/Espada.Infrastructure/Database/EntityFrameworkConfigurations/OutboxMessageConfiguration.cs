using Espada.Db.Constants;
using Espada.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessageRecord>, IEntityTypeConfiguration<Espada.Db.Models.OutboxMessages>
{
    public void Configure(EntityTypeBuilder<OutboxMessageRecord> builder)
    {
        builder.ToTable(DbTableConstants.OutboxMessages, DbConstants.SchemaName);
        builder.HasKey(message => message.EventId);
        builder.Property(message => message.EventName).HasMaxLength(200).IsRequired();
        builder.Property(message => message.PayloadJson).HasColumnType(DbJsonColumnTypeConstants.Jsonb).IsRequired();
        builder.Property(message => message.LeaseOwner).HasMaxLength(200);
        builder.Property(message => message.SanitizedError).HasMaxLength(4000);
        builder.HasIndex(message => new { message.ProcessedAtUtc, message.AvailableAtUtc });
    }

    public void Configure(EntityTypeBuilder<Espada.Db.Models.OutboxMessages> builder)
    {
        builder.Property(message => message.EventId).ValueGeneratedNever();
        builder.Property(message => message.PayloadJson).HasColumnType(DbJsonColumnTypeConstants.Jsonb);
        builder.HasIndex(message => new { message.ProcessedAtUtc, message.AvailableAtUtc });
    }
}
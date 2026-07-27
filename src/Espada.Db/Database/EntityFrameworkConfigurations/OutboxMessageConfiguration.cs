using Espada.Db.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<Models.OutboxMessages>
{
    public void Configure(EntityTypeBuilder<Models.OutboxMessages> builder)
    {
        builder.Property(message => message.EventId).ValueGeneratedNever();
        builder.Property(message => message.PayloadJson).HasColumnType(DbJsonColumnTypeConstants.Jsonb);
        builder.HasIndex(message => new { message.ProcessedAtUtc, message.AvailableAtUtc });
    }
}

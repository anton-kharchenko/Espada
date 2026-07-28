using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class PolicyRuleConfiguration : IEntityTypeConfiguration<PolicyRule>
    {
        public void Configure(EntityTypeBuilder<PolicyRule> builder)
        {
            builder.ToTable(DbTableConstants.PolicyRules, DbConstants.SchemaName,
                table => table.HasCheckConstraint(DbIndexConstants.PolicyRuleKind, "\"Kind\" = 'policy'"));
            builder.HasKey(entity => new { entity.ArtifactRevisionId, entity.RuleKey })
                .HasName(DbIndexConstants.PolicyRuleRevisionKey);
            builder.Property(entity => entity.ArtifactRevisionId).HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => ArtifactRevisionId.Create(value));
            builder.Property(entity => entity.KindType).HasColumnName("Kind")
                .HasColumnType(DbTextColumnTypeConstants.Varchar32).HasMaxLength(DbMaxLengthConstants.L32)
                .HasConversion(kind => kind.Name,
                    value => Enumeration.GetAll<ArtifactKindType>().Single(kind => kind.Name == value)).IsRequired();
            builder.Property(entity => entity.RuleKey).HasColumnType(DbTextColumnTypeConstants.Varchar100)
                .HasMaxLength(DbMaxLengthConstants.L100)
                .HasConversion(key => key.Value, value => RuleKey.Create(value).Value);
            builder.Property(entity => entity.Text).HasColumnType(DbTextColumnTypeConstants.TextType).IsRequired();
            builder.Property(entity => entity.Priority).HasColumnType(DbNumericColumnTypeConstants.Integer)
                .HasConversion(priority => priority.Value, value => ContextPriority.Create(value).Value).IsRequired();
            builder.Property(entity => entity.EnforcementType).HasColumnName("Enforcement")
                .HasColumnType(DbTextColumnTypeConstants.Varchar32).HasMaxLength(DbMaxLengthConstants.L32)
                .HasConversion(enforcement => enforcement.Name,
                    value => Enumeration.GetAll<PolicyEnforcementType>()
                        .Single(enforcement => enforcement.Name == value)).IsRequired();
            builder.HasOne<ArtifactRevision>().WithMany()
                .HasForeignKey(entity => new { entity.ArtifactRevisionId, Kind = entity.KindType })
                .HasPrincipalKey(revision => new { revision.Id, Kind = revision.KindType })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
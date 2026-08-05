using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class PolicyRuleConfiguration : IEntityTypeConfiguration<PolicyRules>
    {
        public void Configure(EntityTypeBuilder<PolicyRules> builder)
        {
            builder.ToTable(DbTableConstants.PolicyRules, DbConstants.SchemaName,
                table => table.HasCheckConstraint(DbIndexConstants.PolicyRuleKind, "\"Kind\" = 'policy'"));
            builder.HasKey(model => new { model.ArtifactRevisionId, model.RuleKey })
                .HasName(DbIndexConstants.PolicyRuleRevisionKey);
            builder.HasOne<ArtifactRevisions>().WithMany()
                .HasForeignKey(model => new { model.ArtifactRevisionId, model.Kind })
                .HasPrincipalKey(model => new { model.ArtifactRevisionId, model.Kind })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class InstructionRuleConfiguration : IEntityTypeConfiguration<InstructionRules>
    {
        public void Configure(EntityTypeBuilder<InstructionRules> builder)
        {
            builder.ToTable(DbTableConstants.InstructionRules, DbConstants.SchemaName,
                table => table.HasCheckConstraint(DbIndexConstants.InstructionRuleKind, "\"Kind\" = 'instruction'"));
            builder.HasKey(model => new { model.ArtifactRevisionId, model.RuleKey })
                .HasName(DbIndexConstants.InstructionRuleRevisionKey);
            builder.HasOne<ArtifactRevisions>().WithMany()
                .HasForeignKey(model => new { model.ArtifactRevisionId, model.Kind })
                .HasPrincipalKey(model => new { model.ArtifactRevisionId, model.Kind })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
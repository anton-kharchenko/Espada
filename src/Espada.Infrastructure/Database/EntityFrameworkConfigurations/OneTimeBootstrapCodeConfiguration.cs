using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class OneTimeBootstrapCodeConfiguration
        : IEntityTypeConfiguration<OneTimeBootstrapCodes>
    {
        public void Configure(EntityTypeBuilder<OneTimeBootstrapCodes> builder)
        {
            builder.ToTable(
                DbTableConstants.OneTimeBootstrapCodes,
                DbConstants.SchemaName);
            builder.HasKey(code => code.OneTimeBootstrapCodeId);
            builder.Property(code => code.OneTimeBootstrapCodeId)
                .ValueGeneratedNever();
            builder.Property(code => code.CodeHash)
                .HasMaxLength(64)
                .IsRequired();
            builder.Property(code => code.Purpose)
                .HasMaxLength(32)
                .IsRequired();
            builder.Property(code => code.IdentityIssuer)
                .HasMaxLength(500)
                .IsRequired();
            builder.Property(code => code.IdentitySubject)
                .HasMaxLength(200)
                .IsRequired();
            builder.HasIndex(code => code.CodeHash)
                .HasDatabaseName(DbIndexConstants.OneTimeBootstrapCodeHash)
                .IsUnique();
        }
    }
}
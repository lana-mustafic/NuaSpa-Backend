using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class RevokedJwtConfiguration : IEntityTypeConfiguration<RevokedJwt>
{
    public void Configure(EntityTypeBuilder<RevokedJwt> builder)
    {
        builder.ToTable("RevokedJwts", tb => tb.HasComment("Opozvani JWT tokeni (logout)"));

        builder.HasIndex(x => x.Jti).IsUnique();
        builder.HasIndex(x => x.ExpiresAtUtc);

        builder.Property(x => x.Jti).HasMaxLength(64).IsRequired();
    }
}

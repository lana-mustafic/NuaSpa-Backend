using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", tb => tb.HasComment("Rotating refresh tokens for session renewal"));

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.FamilyId);
        builder.HasIndex(x => x.ExpiresAtUtc);

        builder.Property(x => x.TokenHash).HasMaxLength(88).IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReplacedBy)
            .WithMany()
            .HasForeignKey(x => x.ReplacedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

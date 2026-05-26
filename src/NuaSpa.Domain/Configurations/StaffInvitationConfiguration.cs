using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class StaffInvitationConfiguration : IEntityTypeConfiguration<StaffInvitation>
{
    public void Configure(EntityTypeBuilder<StaffInvitation> builder)
    {
        builder.ToTable("StaffInvitations");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Email).HasMaxLength(256).IsRequired();
        builder.Property(s => s.TokenHash).HasMaxLength(64).IsRequired();

        builder.HasIndex(s => s.TokenHash);
        builder.HasIndex(s => new { s.ZaposlenikId, s.AcceptedAt });

        builder.HasOne(s => s.Zaposlenik)
            .WithMany()
            .HasForeignKey(s => s.ZaposlenikId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Korisnik)
            .WithMany()
            .HasForeignKey(s => s.KorisnikId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.CreatedByKorisnik)
            .WithMany()
            .HasForeignKey(s => s.CreatedByKorisnikId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class KorisnikConfiguration : IEntityTypeConfiguration<Korisnik>
{
    public void Configure(EntityTypeBuilder<Korisnik> builder)
    {
        builder.ToTable("AspNetUsers");

        builder.Property(k => k.Ime).HasMaxLength(50).IsRequired();
        builder.Property(k => k.Prezime).HasMaxLength(50).IsRequired();
        builder.Property(k => k.NapomenaZaTerapeuta).HasMaxLength(1200);

        builder.HasOne(k => k.Grad)
            .WithMany()
            .HasForeignKey(k => k.GradId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(k => k.Zaposlenik)
            .WithMany()
            .HasForeignKey(k => k.ZaposlenikId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

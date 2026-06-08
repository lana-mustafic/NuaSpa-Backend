using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class RecenzijaConfiguration : IEntityTypeConfiguration<Recenzija>
{
    public void Configure(EntityTypeBuilder<Recenzija> builder)
    {
        builder.ToTable("Recenzije");

        builder.Property(r => r.Komentar).HasMaxLength(1000).IsRequired();
        builder.Property(r => r.AdminOdgovor).HasMaxLength(2000);

        builder.HasOne(r => r.Usluga)
            .WithMany()
            .HasForeignKey(r => r.UslugaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Korisnik)
            .WithMany()
            .HasForeignKey(r => r.KorisnikId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Zaposlenik)
            .WithMany()
            .HasForeignKey(r => r.ZaposlenikId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Rezervacija)
            .WithMany()
            .HasForeignKey(r => r.RezervacijaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.RezervacijaId)
            .IsUnique()
            .HasFilter("[RezervacijaId] IS NOT NULL");
    }
}

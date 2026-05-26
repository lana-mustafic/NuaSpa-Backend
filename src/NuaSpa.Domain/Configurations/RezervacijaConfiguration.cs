using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Domain.Configurations;

public class RezervacijaConfiguration : IEntityTypeConfiguration<Rezervacija>
{
    public void Configure(EntityTypeBuilder<Rezervacija> builder)
    {
        builder.ToTable("Rezervacije");

        builder.Property(r => r.RazlogOtkaza).HasMaxLength(400);
        builder.Property(r => r.Status).HasConversion<int>().HasDefaultValue(RezervacijaStatus.Pending);
        builder.Property(r => r.SnimakCijena).HasColumnType("decimal(18,2)");
        builder.Property(r => r.SnimakTrajanjeMinuta).HasDefaultValue(60);

        builder.HasIndex(r => new { r.KorisnikId, r.ZaposlenikId, r.DatumRezervacije })
            .IsUnique()
            .HasFilter($"[{nameof(Rezervacija.Status)}] <> {(int)RezervacijaStatus.Cancelled}");

        builder.HasOne(r => r.Korisnik)
            .WithMany()
            .HasForeignKey(r => r.KorisnikId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Usluga)
            .WithMany()
            .HasForeignKey(r => r.UslugaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Zaposlenik)
            .WithMany()
            .HasForeignKey(r => r.ZaposlenikId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Prostorija)
            .WithMany()
            .HasForeignKey(r => r.ProstorijaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

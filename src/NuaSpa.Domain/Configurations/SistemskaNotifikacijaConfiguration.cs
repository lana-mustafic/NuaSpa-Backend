using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class SistemskaNotifikacijaConfiguration : IEntityTypeConfiguration<SistemskaNotifikacija>
{
    public void Configure(EntityTypeBuilder<SistemskaNotifikacija> builder)
    {
        builder.ToTable("SistemskaNotifikacije");

        builder.Property(x => x.Naslov).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Tekst).HasMaxLength(2000).IsRequired();

        builder.HasIndex(x => new { x.KorisnikId, x.Procitana, x.CreatedAt });
        builder.HasIndex(x => new { x.KorisnikId, x.CreatedAt });

        builder.HasOne(x => x.Korisnik)
            .WithMany()
            .HasForeignKey(x => x.KorisnikId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Rezervacija)
            .WithMany()
            .HasForeignKey(x => x.RezervacijaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

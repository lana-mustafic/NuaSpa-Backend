using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

/// <summary>Međutabela rezervacija ↔ oprema (količina je operativni detalj rezervacije).</summary>
public class RezervacijaOpremaConfiguration : IEntityTypeConfiguration<RezervacijaOprema>
{
    public void Configure(EntityTypeBuilder<RezervacijaOprema> builder)
    {
        builder.ToTable("RezervacijeOprema", tb => tb.HasComment("Međutabela: rezervacija ↔ oprema"));

        builder.HasIndex(x => new { x.RezervacijaId, x.OpremaId }).IsUnique();

        builder.HasOne(ro => ro.Rezervacija)
            .WithMany(r => r.RezervacijaOprema)
            .HasForeignKey(ro => ro.RezervacijaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ro => ro.Oprema)
            .WithMany()
            .HasForeignKey(ro => ro.OpremaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

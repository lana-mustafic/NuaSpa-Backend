using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class PlacanjeConfiguration : IEntityTypeConfiguration<Placanje>
{
    public void Configure(EntityTypeBuilder<Placanje> builder)
    {
        builder.ToTable("Placanja");

        builder.Property(p => p.Iznos).HasPrecision(18, 2);
        builder.Property(p => p.MetodaPlacanja).HasMaxLength(50).IsRequired();
        builder.Property(p => p.TransakcijskiBroj).HasMaxLength(100).IsRequired();

        builder.HasOne(p => p.Rezervacija)
            .WithMany()
            .HasForeignKey(p => p.RezervacijaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

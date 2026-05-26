using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Domain.Configurations;

public class RezervacijaStatusPromjenaConfiguration : IEntityTypeConfiguration<RezervacijaStatusPromjena>
{
    public void Configure(EntityTypeBuilder<RezervacijaStatusPromjena> builder)
    {
        builder.ToTable("RezervacijaStatusPromjene");

        builder.Property(x => x.Opis).HasMaxLength(400);

        builder.HasOne(x => x.Rezervacija)
            .WithMany(r => r.StatusPromjene)
            .HasForeignKey(x => x.RezervacijaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.RezervacijaId);
        builder.HasIndex(x => x.CreatedAt);
    }
}

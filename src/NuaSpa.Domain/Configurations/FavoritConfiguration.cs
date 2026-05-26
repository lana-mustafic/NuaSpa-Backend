using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

/// <summary>Međutabela (M:N) korisnik ↔ usluga — bez dodatnih poslovnih atributa.</summary>
public class FavoritConfiguration : IEntityTypeConfiguration<Favorit>
{
    public void Configure(EntityTypeBuilder<Favorit> builder)
    {
        builder.ToTable("Favoriti", tb => tb.HasComment("Međutabela: korisnik ↔ omiljena usluga"));

        builder.HasIndex(x => new { x.KorisnikId, x.UslugaId }).IsUnique();

        builder.HasOne(f => f.Korisnik)
            .WithMany()
            .HasForeignKey(f => f.KorisnikId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Usluga)
            .WithMany()
            .HasForeignKey(f => f.UslugaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class UslugaConfiguration : IEntityTypeConfiguration<Usluga>
{
    public void Configure(EntityTypeBuilder<Usluga> builder)
    {
        builder.ToTable("Usluge");

        builder.Property(u => u.Naziv).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Opis).HasMaxLength(1000).IsRequired();
        builder.Property(u => u.Cijena).HasPrecision(18, 2);
        builder.Property(u => u.SlikaUrl).HasMaxLength(500);

        builder.HasOne(u => u.KategorijaUsluga)
            .WithMany()
            .HasForeignKey(u => u.KategorijaUslugaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

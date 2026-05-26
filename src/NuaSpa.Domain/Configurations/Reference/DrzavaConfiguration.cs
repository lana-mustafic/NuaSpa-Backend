using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations.Reference;

/// <summary>Referentna tablica — države (državljanstvo, adrese).</summary>
public class DrzavaConfiguration : IEntityTypeConfiguration<Drzava>
{
    public void Configure(EntityTypeBuilder<Drzava> builder)
    {
        builder.ToTable("Drzave", tb => tb.HasComment("Referentna tablica: države"));

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Naziv).HasMaxLength(100).IsRequired();
        builder.Property(d => d.PozivniBroj).HasMaxLength(10).IsRequired();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations.Reference;

/// <summary>Referentna tablica — kategorije usluga.</summary>
public class KategorijaUslugaConfiguration : IEntityTypeConfiguration<KategorijaUsluga>
{
    public void Configure(EntityTypeBuilder<KategorijaUsluga> builder)
    {
        builder.ToTable("KategorijeUsluga", tb => tb.HasComment("Referentna tablica: kategorije usluga"));

        builder.HasKey(k => k.Id);

        builder.Property(k => k.Naziv).HasMaxLength(100).IsRequired();
        builder.Property(k => k.Opis).HasMaxLength(500);
    }
}

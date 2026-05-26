using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class ObavijestConfiguration : IEntityTypeConfiguration<Obavijest>
{
    public void Configure(EntityTypeBuilder<Obavijest> builder)
    {
        builder.ToTable("Obavijesti");

        builder.Property(x => x.Naslov).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Tekst).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.SlikaUrl).HasMaxLength(500);

        builder.HasIndex(x => new { x.Aktivna, x.DatumObjave });
    }
}

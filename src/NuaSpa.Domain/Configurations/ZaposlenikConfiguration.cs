using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class ZaposlenikConfiguration : IEntityTypeConfiguration<Zaposlenik>
{
    public void Configure(EntityTypeBuilder<Zaposlenik> builder)
    {
        builder.ToTable("Zaposlenici");

        builder.Property(z => z.Ime).HasMaxLength(50).IsRequired();
        builder.Property(z => z.Prezime).HasMaxLength(50).IsRequired();
        builder.Property(z => z.Specijalizacija).HasMaxLength(500).IsRequired();
        builder.Property(z => z.Telefon).HasMaxLength(30);
        builder.Property(z => z.Email).HasMaxLength(120);
        builder.Property(z => z.Jezici).HasMaxLength(200);
        builder.Property(z => z.Obrazovanje).HasMaxLength(1000);
        builder.Property(z => z.Lokacija).HasMaxLength(120);
        builder.Property(z => z.Bio).HasMaxLength(2000);

        builder.HasOne(z => z.KategorijaUsluga)
            .WithMany()
            .HasForeignKey(z => z.KategorijaUslugaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}


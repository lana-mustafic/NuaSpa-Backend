using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class KorisnikAktivnostConfiguration : IEntityTypeConfiguration<KorisnikAktivnost>
{
    public void Configure(EntityTypeBuilder<KorisnikAktivnost> builder)
    {
        builder.ToTable("KorisnikAktivnosti");

        builder.Property(a => a.SearchTerm).HasMaxLength(200);

        builder.HasIndex(a => new { a.KorisnikId, a.CreatedAt });
        builder.HasIndex(a => a.UslugaId);

        builder.HasOne(a => a.Korisnik)
            .WithMany()
            .HasForeignKey(a => a.KorisnikId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Usluga)
            .WithMany()
            .HasForeignKey(a => a.UslugaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.KategorijaUsluga)
            .WithMany()
            .HasForeignKey(a => a.KategorijaUslugaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class NarudzbaProizvodaConfiguration : IEntityTypeConfiguration<NarudzbaProizvoda>
{
    public void Configure(EntityTypeBuilder<NarudzbaProizvoda> builder)
    {
        builder.ToTable("NarudzbeProizvoda");

        builder.Property(n => n.UkupnaCijena).HasPrecision(18, 2);

        builder.HasOne(n => n.Korisnik)
            .WithMany()
            .HasForeignKey(n => n.KorisnikId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Proizvod)
            .WithMany()
            .HasForeignKey(n => n.ProizvodId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

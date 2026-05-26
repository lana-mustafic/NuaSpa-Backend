using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class SpaCentarConfiguration : IEntityTypeConfiguration<SpaCentar>
{
    public void Configure(EntityTypeBuilder<SpaCentar> builder)
    {
        builder.ToTable("SpaCentri");

        builder.Property(s => s.Naziv).HasMaxLength(120).IsRequired();
        builder.Property(s => s.Adresa).HasMaxLength(200);
        builder.Property(s => s.Email).HasMaxLength(120);
        builder.Property(s => s.Telefon).HasMaxLength(60);
        builder.Property(s => s.Opis).HasMaxLength(1200);
    }
}

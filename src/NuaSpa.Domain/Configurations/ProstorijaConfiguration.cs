using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class ProstorijaConfiguration : IEntityTypeConfiguration<Prostorija>
{
    public void Configure(EntityTypeBuilder<Prostorija> builder)
    {
        builder.ToTable("Prostorije");

        builder.Property(p => p.Naziv).HasMaxLength(80).IsRequired();
        builder.Property(p => p.Opis).HasMaxLength(400);

        builder.HasOne(p => p.SpaCentar)
            .WithMany()
            .HasForeignKey(p => p.SpaCentarId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

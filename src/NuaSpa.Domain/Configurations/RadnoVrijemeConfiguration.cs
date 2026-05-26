using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class RadnoVrijemeConfiguration : IEntityTypeConfiguration<RadnoVrijeme>
{
    public void Configure(EntityTypeBuilder<RadnoVrijeme> builder)
    {
        builder.ToTable("RadnaVremena");

        builder.HasOne(r => r.SpaCentar)
            .WithMany()
            .HasForeignKey(r => r.SpaCentarId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

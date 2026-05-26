using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class SkladisteConfiguration : IEntityTypeConfiguration<Skladiste>
{
    public void Configure(EntityTypeBuilder<Skladiste> builder)
    {
        builder.ToTable("Skladista");

        builder.Property(s => s.Lokacija).HasMaxLength(200).IsRequired();

        builder.HasOne(s => s.Proizvod)
            .WithMany()
            .HasForeignKey(s => s.ProizvodId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

public class OpremaConfiguration : IEntityTypeConfiguration<Oprema>
{
    public void Configure(EntityTypeBuilder<Oprema> builder)
    {
        builder.ToTable("Oprema");

        builder.Property(o => o.Naziv).HasMaxLength(120).IsRequired();
        builder.Property(o => o.Napomena).HasMaxLength(400);

        builder.HasOne(o => o.SpaCentar)
            .WithMany()
            .HasForeignKey(o => o.SpaCentarId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

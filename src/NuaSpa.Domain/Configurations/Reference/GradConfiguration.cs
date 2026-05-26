using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations.Reference;

/// <summary>Referentna tablica — gradovi (adrese korisnika).</summary>
public class GradConfiguration : IEntityTypeConfiguration<Grad>
{
    public void Configure(EntityTypeBuilder<Grad> builder)
    {
        builder.ToTable("Gradovi", tb => tb.HasComment("Referentna tablica: gradovi"));

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Naziv).HasMaxLength(100).IsRequired();
        builder.Property(g => g.PostanskiBroj).HasMaxLength(20).IsRequired();

        builder.HasOne(g => g.Drzava)
            .WithMany()
            .HasForeignKey(g => g.DrzavaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

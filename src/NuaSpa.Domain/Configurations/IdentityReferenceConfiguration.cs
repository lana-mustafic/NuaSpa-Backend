using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Domain.Configurations;

/// <summary>Referentna ASP.NET Identity tablica uloga (AspNetRoles).</summary>
public class IdentityReferenceConfiguration : IEntityTypeConfiguration<Uloga>
{
    public void Configure(EntityTypeBuilder<Uloga> builder)
    {
        builder.ToTable("AspNetRoles", tb => tb.HasComment("Referentna tablica: uloge (ASP.NET Identity)"));
    }
}

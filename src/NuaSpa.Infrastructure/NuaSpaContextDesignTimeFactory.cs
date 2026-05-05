using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NuaSpa.Domain;

namespace NuaSpa.Infrastructure;

/// <summary>
/// Omogućava <c>dotnet ef</c> bez pokretanja web hosta. Connection string samo iz okruženja / user secrets (ne iz koda).
/// </summary>
public class NuaSpaContextDesignTimeFactory : IDesignTimeDbContextFactory<NuaSpaContext>
{
    public NuaSpaContext CreateDbContext(string[] args)
    {
        var cs =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("NuaSpa__ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(cs))
        {
            throw new InvalidOperationException(
                "Za migracije postavite ConnectionStrings__DefaultConnection (npr. iz istog user secrets kao API projekt).");
        }

        var optionsBuilder = new DbContextOptionsBuilder<NuaSpaContext>();
        optionsBuilder.UseSqlServer(cs, x => x.MigrationsAssembly("NuaSpa.Infrastructure"));
        return new NuaSpaContext(optionsBuilder.Options);
    }
}

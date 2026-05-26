using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NuaSpa.Application.Configuration;
using NuaSpa.Domain;

namespace NuaSpa.Infrastructure;

/// <summary>
/// Omogućava <c>dotnet ef</c> bez pokretanja web hosta. Connection string iz .env / okruženja.
/// </summary>
public class NuaSpaContextDesignTimeFactory : IDesignTimeDbContextFactory<NuaSpaContext>
{
    public NuaSpaContext CreateDbContext(string[] args)
    {
        EnvFileLoader.Load();

        var cs =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("NuaSpa__ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(cs))
        {
            throw new InvalidOperationException(
                "Za migracije kopirajte .env.example u .env i postavite ConnectionStrings__DefaultConnection.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<NuaSpaContext>();
        optionsBuilder.UseSqlServer(cs, x => x.MigrationsAssembly("NuaSpa.Infrastructure"));
        return new NuaSpaContext(optionsBuilder.Options);
    }
}

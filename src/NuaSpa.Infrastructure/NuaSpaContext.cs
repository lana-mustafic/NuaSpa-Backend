using System;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Infrastructure
{
    public class NuaSpaContext : DbContext
    {
        public NuaSpaContext(DbContextOptions<NuaSpaContext> options) : base(options)
        {
        }

        public DbSet<Drzava> Drzave { get; set; }
        public DbSet<Grad> Gradovi { get; set; }
        public DbSet<Uloga> Uloge { get; set; }
        public DbSet<KategorijaUsluga> KategorijeUsluga { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Senior trik: Fiksni datum za sve početne podatke
            var seedDate = new DateTime(2026, 1, 1);

            // Seed podataka za Uloge
            modelBuilder.Entity<Uloga>().HasData(
                new Uloga { Id = 1, Naziv = "Admin", CreatedAt = seedDate },
                new Uloga { Id = 2, Naziv = "Desktop", CreatedAt = seedDate },
                new Uloga { Id = 3, Naziv = "Mobile", CreatedAt = seedDate }
            );

            // Seed podataka za Drzave
            modelBuilder.Entity<Drzava>().HasData(
                new Drzava { Id = 1, Naziv = "Bosna i Hercegovina", PozivniBroj = "387", CreatedAt = seedDate }
            );

            // Seed podataka za Gradove
            modelBuilder.Entity<Grad>().HasData(
                new Grad { Id = 1, Naziv = "Sarajevo", PostanskiBroj = "71000", DrzavaId = 1, CreatedAt = seedDate },
                new Grad { Id = 2, Naziv = "Mostar", PostanskiBroj = "88000", DrzavaId = 1, CreatedAt = seedDate }
            );
        }
    }
}
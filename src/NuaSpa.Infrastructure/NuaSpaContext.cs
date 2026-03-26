using Microsoft.EntityFrameworkCore;
using NuaSpa.Domain.Entities;
using System;
using System.Runtime.Intrinsics.X86;

namespace NuaSpa.Infrastructure
{
    public class NuaSpaContext : DbContext
    {
        public NuaSpaContext(DbContextOptions<NuaSpaContext> options) : base(options)
        {
        }

        // --- Referentne tabele ---
        public DbSet<Drzava> Drzave { get; set; }
        public DbSet<Grad> Gradovi { get; set; }
        public DbSet<Uloga> Uloge { get; set; }
        public DbSet<KategorijaUsluga> KategorijeUsluga { get; set; }

        // --- Core tabele (Task 1.2) ---
        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<Zaposlenik> Zaposlenici { get; set; }
        public DbSet<Usluga> Usluge { get; set; }
        public DbSet<Rezervacija> Rezervacije { get; set; }
        public DbSet<Recenzija> Recenzije { get; set; }

        // --- Proizvodi i Skladište ---
        public DbSet<Proizvod> Proizvodi { get; set; }
        public DbSet<Skladiste> Skladista { get; set; }
        public DbSet<NarudzbaProizvoda> NarudzbeProizvoda { get; set; }

        // --- Finansije ---
        public DbSet<Placanje> Placanja { get; set; }
        public DbSet<Popust> Popusti { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Ostavljamo ovo kao "sigurnosni ventil" zbog tvog LocalDB-a
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=180081;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var seedDate = new DateTime(2026, 1, 1);

            // --- SEED PODATAKA ---

            modelBuilder.Entity<Uloga>().HasData(
                new Uloga { Id = 1, Naziv = "Admin", CreatedAt = seedDate },
                new Uloga { Id = 2, Naziv = "Desktop", CreatedAt = seedDate },
                new Uloga { Id = 3, Naziv = "Mobile", CreatedAt = seedDate }
            );

            modelBuilder.Entity<Drzava>().HasData(
                new Drzava { Id = 1, Naziv = "Bosna i Hercegovina", PozivniBroj = "387", CreatedAt = seedDate }
            );

            modelBuilder.Entity<Grad>().HasData(
                new Grad { Id = 1, Naziv = "Sarajevo", PostanskiBroj = "71000", DrzavaId = 1, CreatedAt = seedDate },
                new Grad { Id = 2, Naziv = "Mostar", PostanskiBroj = "88000", DrzavaId = 1, CreatedAt = seedDate }
            );

            // Dodajemo jednu osnovnu kategoriju da Usluge ne bi pucale
            modelBuilder.Entity<KategorijaUsluga>().HasData(
                new KategorijaUsluga { Id = 1, Naziv = "Masaže", Opis = "Relaksacione i terapeutske masaže", CreatedAt = seedDate }
            );
        }
    }
}
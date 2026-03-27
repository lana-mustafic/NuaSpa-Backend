using System;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Infrastructure
{
    public class NuaSpaContext : DbContext
    {
        public NuaSpaContext(DbContextOptions<NuaSpaContext> options) : base(options) { }

        public DbSet<Drzava> Drzave { get; set; }
        public DbSet<Grad> Gradovi { get; set; }
        public DbSet<Uloga> Uloge { get; set; }
        public DbSet<KategorijaUsluga> KategorijeUsluga { get; set; }
        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<Zaposlenik> Zaposlenici { get; set; }
        public DbSet<Usluga> Usluge { get; set; }
        public DbSet<Rezervacija> Rezervacije { get; set; }
        public DbSet<Recenzija> Recenzije { get; set; }
        public DbSet<Proizvod> Proizvodi { get; set; }
        public DbSet<Skladiste> Skladista { get; set; }
        public DbSet<NarudzbaProizvoda> NarudzbeProizvoda { get; set; }
        public DbSet<Placanje> Placanja { get; set; }
        public DbSet<Popust> Popusti { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var seedDate = new DateTime(2026, 1, 1);

            // 1. SEED PODATAKA
            modelBuilder.Entity<Uloga>().HasData(
                new Uloga { Id = 1, Naziv = "Admin", CreatedAt = seedDate },
                new Uloga { Id = 2, Naziv = "Klijent", CreatedAt = seedDate },
                new Uloga { Id = 3, Naziv = "Zaposlenik", CreatedAt = seedDate }
            );

            modelBuilder.Entity<Drzava>().HasData(
                new Drzava { Id = 1, Naziv = "Bosna i Hercegovina", PozivniBroj = "387", CreatedAt = seedDate }
            );

            modelBuilder.Entity<Grad>().HasData(
                new Grad { Id = 1, Naziv = "Sarajevo", PostanskiBroj = "71000", DrzavaId = 1, CreatedAt = seedDate },
                new Grad { Id = 3, Naziv = "Konjic", PostanskiBroj = "88400", DrzavaId = 1, CreatedAt = seedDate }
                // Dodaj ostale gradove po potrebi...
            );

            // --- Task 4.3: ISPRAVLJEN SEED ZA KORISNIKE ---
            modelBuilder.Entity<Korisnik>().HasData(
                new Korisnik
                {
                    Id = 1,
                    Ime = "Admin",
                    Prezime = "NuaSpa",
                    Email = "admin@nuaspa.ba",
                    KorisnickoIme = "admin",
                    PasswordHash = "dummy_hash_123", // Usklađeno sa klasom
                    PasswordSalt = "dummy_salt_123", // Usklađeno sa klasom
                    Telefon = "033123456",
                    UlogaId = 1,
                    GradId = 1,
                    Status = true,
                    CreatedAt = seedDate
                },
                new Korisnik
                {
                    Id = 2,
                    Ime = "Lana",
                    Prezime = "Korisnik",
                    Email = "lana@test.ba",
                    KorisnickoIme = "lana",
                    PasswordHash = "dummy_hash_456",
                    PasswordSalt = "dummy_salt_456",
                    Telefon = "061222333",
                    UlogaId = 2,
                    GradId = 3, // Konjic
                    Status = true,
                    CreatedAt = seedDate
                }
            );

            // 2. FLUENT API (Decimali i Indeksi)
            modelBuilder.Entity<Usluga>().Property(u => u.Cijena).HasPrecision(18, 2);
            modelBuilder.Entity<Korisnik>().HasIndex(k => k.Email).IsUnique();
            modelBuilder.Entity<Korisnik>().HasIndex(k => k.KorisnickoIme).IsUnique();

            // Relacije
            modelBuilder.Entity<Korisnik>()
                .HasOne(k => k.Grad)
                .WithMany()
                .HasForeignKey(k => k.GradId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
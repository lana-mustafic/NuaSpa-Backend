using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Domain.Entities;
using System;

namespace NuaSpa.Domain
{
    public class NuaSpaContext : IdentityDbContext<Korisnik, Uloga, int>
    {
        public NuaSpaContext(DbContextOptions<NuaSpaContext> options) : base(options) { }

        public DbSet<Drzava> Drzave { get; set; } = null!;
        public DbSet<Grad> Gradovi { get; set; } = null!;
        public DbSet<KategorijaUsluga> KategorijeUsluga { get; set; } = null!;
        public DbSet<Zaposlenik> Zaposlenici { get; set; } = null!;
        public DbSet<Usluga> Usluge { get; set; } = null!;
        public DbSet<Rezervacija> Rezervacije { get; set; } = null!;
        public DbSet<Recenzija> Recenzije { get; set; } = null!;
        public DbSet<Proizvod> Proizvodi { get; set; } = null!;
        public DbSet<Skladiste> Skladista { get; set; } = null!;
        public DbSet<NarudzbaProizvoda> NarudzbeProizvoda { get; set; } = null!;
        public DbSet<Placanje> Placanja { get; set; } = null!;
        public DbSet<Popust> Popusti { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // OVO MORA BITI PRVO!
            base.OnModelCreating(modelBuilder);

            var seedDate = new DateTime(2026, 1, 1);

            // 1. SEED ZA ULOGE
            modelBuilder.Entity<Uloga>().HasData(
                new Uloga { Id = 1, Name = "Admin", NormalizedName = "ADMIN", CreatedAt = seedDate, ConcurrencyStamp = Guid.NewGuid().ToString() },
                new Uloga { Id = 2, Name = "Klijent", NormalizedName = "KLIJENT", CreatedAt = seedDate, ConcurrencyStamp = Guid.NewGuid().ToString() },
                new Uloga { Id = 3, Name = "Zaposlenik", NormalizedName = "ZAPOSLENIK", CreatedAt = seedDate, ConcurrencyStamp = Guid.NewGuid().ToString() }
            );

            modelBuilder.Entity<Drzava>().HasData(
                new Drzava { Id = 1, Naziv = "Bosna i Hercegovina", PozivniBroj = "387", CreatedAt = seedDate }
            );

            modelBuilder.Entity<Grad>().HasData(
                new Grad { Id = 1, Naziv = "Sarajevo", PostanskiBroj = "71000", DrzavaId = 1, CreatedAt = seedDate },
                new Grad { Id = 3, Naziv = "Konjic", PostanskiBroj = "88400", DrzavaId = 1, CreatedAt = seedDate }
            );

            // 2. SEED ZA KORISNIKE (SecurityStamp je OBAVEZAN za Identity)
            var adminId = 1;
            var lanaId = 2;

            modelBuilder.Entity<Korisnik>().HasData(
                new Korisnik
                {
                    Id = adminId,
                    Ime = "Admin",
                    Prezime = "NuaSpa",
                    Email = "admin@nuaspa.ba",
                    NormalizedEmail = "ADMIN@NUASPA.BA",
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    PasswordHash = "AQAAAAEAACcQAAAAE...dummy_hash",
                    SecurityStamp = Guid.NewGuid().ToString(), // Dodano!
                    PhoneNumber = "033123456",
                    GradId = 1,
                    Status = true,
                    DatumRegistracije = seedDate
                },
                new Korisnik
                {
                    Id = lanaId,
                    Ime = "Lana",
                    Prezime = "Korisnik",
                    Email = "lana@test.ba",
                    NormalizedEmail = "LANA@TEST.BA",
                    UserName = "lana",
                    NormalizedUserName = "LANA",
                    PasswordHash = "AQAAAAEAACcQAAAAE...dummy_hash",
                    SecurityStamp = Guid.NewGuid().ToString(), // Dodano!
                    PhoneNumber = "061222333",
                    GradId = 3,
                    Status = true,
                    DatumRegistracije = seedDate
                }
            );

            // 3. POVEZIVANJE KORISNIKA I ULOGA (Ovo ti je falilo!)
            modelBuilder.Entity<IdentityUserRole<int>>().HasData(
                new IdentityUserRole<int> { UserId = adminId, RoleId = 1 }, // Admin je Admin
                new IdentityUserRole<int> { UserId = lanaId, RoleId = 2 }   // Lana je Klijent
            );

            // 4. FLUENT API
            modelBuilder.Entity<Usluga>().Property(u => u.Cijena).HasPrecision(18, 2);

            modelBuilder.Entity<Korisnik>()
                .HasOne(k => k.Grad)
                .WithMany()
                .HasForeignKey(k => k.GradId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
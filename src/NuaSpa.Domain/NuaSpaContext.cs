using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Common;
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
        public DbSet<StaffInvitation> StaffInvitations { get; set; } = null!;
        public DbSet<Usluga> Usluge { get; set; } = null!;
        public DbSet<Rezervacija> Rezervacije { get; set; } = null!;
        public DbSet<RezervacijaOprema> RezervacijeOprema { get; set; } = null!;
        public DbSet<Recenzija> Recenzije { get; set; } = null!;
        public DbSet<Favorit> Favoriti { get; set; } = null!;
        public DbSet<KorisnikAktivnost> KorisnikAktivnosti { get; set; } = null!;
        public DbSet<Proizvod> Proizvodi { get; set; } = null!;
        public DbSet<Skladiste> Skladista { get; set; } = null!;
        public DbSet<NarudzbaProizvoda> NarudzbeProizvoda { get; set; } = null!;
        public DbSet<Placanje> Placanja { get; set; } = null!;
        public DbSet<Popust> Popusti { get; set; } = null!;

        public DbSet<SpaCentar> SpaCentri { get; set; } = null!;
        public DbSet<RadnoVrijeme> RadnaVremena { get; set; } = null!;
        public DbSet<Prostorija> Prostorije { get; set; } = null!;
        public DbSet<Oprema> Oprema { get; set; } = null!;
        public DbSet<RevokedJwt> RevokedJwts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NuaSpaContext).Assembly);

            var seedDate = new DateTime(2026, 1, 1);

            // 1. SEED ZA ULOGE
            modelBuilder.Entity<Uloga>().HasData(
                new Uloga
                {
                    Id = RoleNames.AdminRoleId,
                    Name = RoleNames.Admin,
                    NormalizedName = RoleNames.AdminNormalized,
                    CreatedAt = seedDate,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new Uloga
                {
                    Id = RoleNames.KlijentRoleId,
                    Name = RoleNames.Klijent,
                    NormalizedName = RoleNames.KlijentNormalized,
                    CreatedAt = seedDate,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new Uloga
                {
                    Id = RoleNames.ZaposlenikRoleId,
                    Name = RoleNames.Zaposlenik,
                    NormalizedName = RoleNames.ZaposlenikNormalized,
                    CreatedAt = seedDate,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                }
            );

            modelBuilder.Entity<Drzava>().HasData(
                new Drzava { Id = 1, Naziv = "Bosna i Hercegovina", PozivniBroj = "387", CreatedAt = seedDate }
            );

            modelBuilder.Entity<Grad>().HasData(
                new Grad { Id = 1, Naziv = "Sarajevo", PostanskiBroj = "71000", DrzavaId = 1, CreatedAt = seedDate },
                new Grad { Id = 3, Naziv = "Konjic", PostanskiBroj = "88400", DrzavaId = 1, CreatedAt = seedDate }
            );

            modelBuilder.Entity<KategorijaUsluga>().HasData(
                new KategorijaUsluga { Id = 1, Naziv = "Massage", Opis = "Masaže i relaks tretmani", CreatedAt = seedDate },
                new KategorijaUsluga { Id = 2, Naziv = "Facial", Opis = "Tretmani lica", CreatedAt = seedDate },
                new KategorijaUsluga { Id = 3, Naziv = "Body", Opis = "Tretmani tijela", CreatedAt = seedDate }
            );

            // 2. SEED ZA KORISNIKE — PasswordHash u migraciji je placeholder;
            //    radne lozinke postavlja Development seeder preko UserManager (Identity hash).
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
                new IdentityUserRole<int> { UserId = adminId, RoleId = RoleNames.AdminRoleId }, // Admin je Admin
                new IdentityUserRole<int> { UserId = lanaId, RoleId = RoleNames.KlijentRoleId }   // Lana je Klijent
            );

            // Seed a single default spa center (id=1) + default working hours.
            modelBuilder.Entity<SpaCentar>().HasData(
                new SpaCentar
                {
                    Id = 1,
                    Naziv = "NuaSpa",
                    Adresa = "Sarajevo",
                    Email = "info@nuaspa.ba",
                    Telefon = "033 000 000",
                    Opis = "Luksuzni spa centar. Relax. Renew. Rejuvenate.",
                    CreatedAt = seedDate
                }
            );

            // 09:00–17:00 Mon–Fri, closed Sat/Sun by default.
            modelBuilder.Entity<RadnoVrijeme>().HasData(
                new RadnoVrijeme { Id = 1, SpaCentarId = 1, DanUSedmici = 1, IsClosed = false, OtvaraMin = 9 * 60, ZatvaraMin = 17 * 60, CreatedAt = seedDate },
                new RadnoVrijeme { Id = 2, SpaCentarId = 1, DanUSedmici = 2, IsClosed = false, OtvaraMin = 9 * 60, ZatvaraMin = 17 * 60, CreatedAt = seedDate },
                new RadnoVrijeme { Id = 3, SpaCentarId = 1, DanUSedmici = 3, IsClosed = false, OtvaraMin = 9 * 60, ZatvaraMin = 17 * 60, CreatedAt = seedDate },
                new RadnoVrijeme { Id = 4, SpaCentarId = 1, DanUSedmici = 4, IsClosed = false, OtvaraMin = 9 * 60, ZatvaraMin = 17 * 60, CreatedAt = seedDate },
                new RadnoVrijeme { Id = 5, SpaCentarId = 1, DanUSedmici = 5, IsClosed = false, OtvaraMin = 9 * 60, ZatvaraMin = 17 * 60, CreatedAt = seedDate },
                new RadnoVrijeme { Id = 6, SpaCentarId = 1, DanUSedmici = 6, IsClosed = true, OtvaraMin = null, ZatvaraMin = null, CreatedAt = seedDate },
                new RadnoVrijeme { Id = 7, SpaCentarId = 1, DanUSedmici = 7, IsClosed = true, OtvaraMin = null, ZatvaraMin = null, CreatedAt = seedDate }
            );
        }
    }
}
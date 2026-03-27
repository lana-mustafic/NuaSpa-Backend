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

        // --- Referentne tabele ---
        public DbSet<Drzava> Drzave { get; set; }
        public DbSet<Grad> Gradovi { get; set; }
        public DbSet<Uloga> Uloge { get; set; }
        public DbSet<KategorijaUsluga> KategorijeUsluga { get; set; }

        // --- Core tabele ---
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
            if (!optionsBuilder.IsConfigured)
            {
                // Napomena: Ovo će se kasnije premjestiti u appsettings.json u Tasku 3.3
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=180081;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var seedDate = new DateTime(2026, 1, 1);

            // --- 1. SEED PODATAKA ---
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

            modelBuilder.Entity<KategorijaUsluga>().HasData(
                new KategorijaUsluga { Id = 1, Naziv = "Masaže", Opis = "Relaksacione i terapeutske masaže", CreatedAt = seedDate }
            );

            // --- 2. FLUENT API KONFIGURACIJA (Task 3.2) ---

            // Preciznost za Decimal tipove (Finansije)
            modelBuilder.Entity<Usluga>().Property(u => u.Cijena).HasPrecision(18, 2);
            modelBuilder.Entity<Proizvod>().Property(p => p.Cijena).HasPrecision(18, 2);
            modelBuilder.Entity<Placanje>().Property(p => p.Iznos).HasPrecision(18, 2);
            modelBuilder.Entity<Popust>().Property(p => p.Procenat).HasPrecision(5, 2);
            modelBuilder.Entity<NarudzbaProizvoda>().Property(n => n.UkupnaCijena).HasPrecision(18, 2);

            // Unikatni indeksi
            modelBuilder.Entity<Korisnik>().HasIndex(k => k.Email).IsUnique();
            modelBuilder.Entity<Korisnik>().HasIndex(k => k.KorisnickoIme).IsUnique();
            modelBuilder.Entity<Proizvod>().HasIndex(p => p.Sifra).IsUnique();

            // Relacije i DeleteBehavior
            modelBuilder.Entity<Grad>()
                .HasOne(g => g.Drzava)
                .WithMany()
                .HasForeignKey(g => g.DrzavaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Korisnik>()
                .HasOne(k => k.Uloga)
                .WithMany()
                .HasForeignKey(k => k.UlogaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rezervacija>()
                .HasOne(r => r.Korisnik)
                .WithMany()
                .HasForeignKey(r => r.KorisnikId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Rezervacija>()
                .HasOne(r => r.Zaposlenik)
                .WithMany()
                .HasForeignKey(r => r.ZaposlenikId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Recenzija>()
                .HasOne(r => r.Korisnik)
                .WithMany()
                .HasForeignKey(r => r.KorisnikId)
                .OnDelete(DeleteBehavior.NoAction);

            // Spojna tabela M:N (NarudzbaProizvoda)
            modelBuilder.Entity<NarudzbaProizvoda>()
                .HasOne(np => np.Korisnik)
                .WithMany()
                .HasForeignKey(np => np.KorisnikId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NarudzbaProizvoda>()
                .HasOne(np => np.Proizvod)
                .WithMany()
                .HasForeignKey(np => np.ProizvodId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
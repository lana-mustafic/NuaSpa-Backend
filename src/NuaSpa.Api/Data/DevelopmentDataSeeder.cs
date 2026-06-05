using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;
using NuaSpa.Application.Common;
using NuaSpa.Application.Services.Booking;

namespace NuaSpa.Api.Data;

/// <summary>
/// Obogaćuje bazu test podacima u Development (nakon migracija i Identity seed-a).
/// Lozinke se uvijek postavljaju preko UserManager (nikad dummy hash iz HasData).
/// </summary>
public static class DevelopmentDataSeeder
{
    private const string SeedImageFileName = "seed-usluga.jpg";

    public static async Task SeedAsync(
        IServiceProvider services,
        IWebHostEnvironment env,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var context = services.GetRequiredService<NuaSpaContext>();
        var userManager = services.GetRequiredService<UserManager<Korisnik>>();

        var seedImageUrl = await EnsureSeedImageAsync(env, logger, cancellationToken);
        await EnsureCatalogReferenceDataAsync(context, seedImageUrl, logger, cancellationToken);

        if (await context.Usluge.CountAsync(cancellationToken) >= 8)
        {
            logger.LogInformation("Dev seed: dovoljno usluga već postoji, preskačem obogaćivanje.");
            return;
        }

        await EnsureProstorijeAsync(context, cancellationToken);

        var catMassage = await context.KategorijeUsluga
            .FirstAsync(k => k.Id == 1, cancellationToken);
        var catFacial = await context.KategorijeUsluga
            .FirstAsync(k => k.Id == 2, cancellationToken);
        var catBody = await context.KategorijeUsluga
            .FirstAsync(k => k.Id == 3, cancellationToken);

        var therapists = await EnsureTherapistsAsync(context, catMassage, catFacial, catBody, cancellationToken);
        var servicesList = await EnsureUslugeAsync(context, seedImageUrl, catMassage, catFacial, catBody, cancellationToken);
        var clients = await EnsureExtraClientsAsync(userManager, context, cancellationToken);
        await EnsureProizvodiAsync(context, cancellationToken);
        await EnsureRezervacijeAndRecenzijeAsync(
            context, clients, therapists, servicesList, cancellationToken);

        logger.LogInformation("Dev seed: test podaci uspješno dodani.");
    }

    private static async Task<string> EnsureSeedImageAsync(
        IWebHostEnvironment env,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var dir = Path.Combine(webRoot, "uploads", "usluge");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, SeedImageFileName);

        if (!File.Exists(path))
        {
            // Minimalni validni JPEG (1x1 px) — isti fajl za sve seed usluge.
            var jpeg = Convert.FromBase64String(
                "/9j/4AAQSkZJRgABAQEASABIAAD/2wBDAP//////////////////////////////////////////////////////////////////////////////////////2wBDAf//////////////////////////////////////////////////////////////////////////////////////wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAX/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAAAAX/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwCwAA//2Q==");
            await File.WriteAllBytesAsync(path, jpeg, cancellationToken);
            logger.LogInformation("Dev seed: kreirana seed slika {Path}", path);
        }

        return $"/api/files/usluge/{SeedImageFileName}";
    }

    /// <summary>
    /// Idempotent catalog reference data (runs even when full dev seed is skipped).
    /// </summary>
    private static async Task EnsureCatalogReferenceDataAsync(
        NuaSpaContext context,
        string slikaUrl,
        ILogger logger,
        CancellationToken ct)
    {
        var catBeauty = await EnsureCategoryAsync(
            context,
            "Beauty",
            "Brows, lashes, and makeup treatments.",
            ct);

        var seedDate = DateTime.UtcNow;
        var beautyServices = new (string Naziv, int KatId, decimal Cijena, int Trajanje, string Opis)[]
        {
            ("Brow Lamination", catBeauty.Id, 55m, 45, "Defined, fuller-looking brows."),
            ("Classic Lash Extensions", catBeauty.Id, 85m, 90, "Natural volume lash set."),
            ("Event Makeup", catBeauty.Id, 95m, 60, "Professional makeup for events."),
        };

        var added = 0;
        foreach (var (naziv, katId, cijena, trajanje, opis) in beautyServices)
        {
            if (await context.Usluge.AnyAsync(u => u.Naziv == naziv && !u.IsDeleted, ct))
            {
                continue;
            }

            context.Usluge.Add(new Usluga
            {
                Naziv = naziv,
                KategorijaUslugaId = katId,
                Cijena = cijena,
                TrajanjeMinuta = trajanje,
                Opis = opis,
                SlikaUrl = slikaUrl,
                CreatedAt = seedDate,
                IsDeleted = false,
            });
            added++;
        }

        if (added > 0)
        {
            await context.SaveChangesAsync(ct);
        }

        logger.LogInformation(
            "Dev seed: catalog reference data ensured (Beauty category + {Added} services).",
            added);
    }

    private static async Task<KategorijaUsluga> EnsureCategoryAsync(
        NuaSpaContext context,
        string naziv,
        string? opis,
        CancellationToken ct)
    {
        var existing = await context.KategorijeUsluga
            .FirstOrDefaultAsync(k => !k.IsDeleted && k.Naziv == naziv, ct);
        if (existing != null)
        {
            return existing;
        }

        var entity = new KategorijaUsluga
        {
            Naziv = naziv,
            Opis = opis,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
        };
        context.KategorijeUsluga.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    private static async Task EnsureProstorijeAsync(NuaSpaContext context, CancellationToken ct)
    {
        if (await context.Prostorije.AnyAsync(ct))
        {
            return;
        }

        var seedDate = DateTime.UtcNow;
        context.Prostorije.AddRange(
            new Prostorija
            {
                SpaCentarId = 1,
                Naziv = "Zen Suite 1",
                Kapacitet = 1,
                CreatedAt = seedDate,
            },
            new Prostorija
            {
                SpaCentarId = 1,
                Naziv = "Zen Suite 2",
                Kapacitet = 2,
                CreatedAt = seedDate,
            });
        await context.SaveChangesAsync(ct);
    }

    private static async Task<List<Zaposlenik>> EnsureTherapistsAsync(
        NuaSpaContext context,
        KategorijaUsluga catMassage,
        KategorijaUsluga catFacial,
        KategorijaUsluga catBody,
        CancellationToken ct)
    {
        var existing = await context.Zaposlenici.Where(z => !z.IsDeleted).ToListAsync(ct);
        if (existing.Count >= 4)
        {
            return existing;
        }

        var seedDate = DateTime.UtcNow;
        var toAdd = new List<Zaposlenik>
        {
            new()
            {
                Ime = "Amira", Prezime = "Hadžić", Specijalizacija = "Swedish & deep tissue",
                KategorijaUslugaId = catMassage.Id, DatumZaposlenja = seedDate, Status = ZaposlenikStatus.Active,
                Email = "amira.hadzic@nuaspa.ba", Telefon = "061111001", CreatedAt = seedDate,
            },
            new()
            {
                Ime = "Kenan", Prezime = "Softić", Specijalizacija = "Facial & skin care",
                KategorijaUslugaId = catFacial.Id, DatumZaposlenja = seedDate, Status = ZaposlenikStatus.Active,
                Email = "kenan.softic@nuaspa.ba", Telefon = "061111002", CreatedAt = seedDate,
            },
            new()
            {
                Ime = "Lejla", Prezime = "Mujić", Specijalizacija = "Body wrap & scrub",
                KategorijaUslugaId = catBody.Id, DatumZaposlenja = seedDate, Status = ZaposlenikStatus.Active,
                Email = "lejla.mujic@nuaspa.ba", Telefon = "061111003", CreatedAt = seedDate,
            },
        };

        foreach (var z in toAdd)
        {
            if (!existing.Any(e => e.Email == z.Email))
            {
                context.Zaposlenici.Add(z);
            }
        }

        await context.SaveChangesAsync(ct);
        return await context.Zaposlenici.Where(z => !z.IsDeleted).ToListAsync(ct);
    }

    private static async Task<List<Usluga>> EnsureUslugeAsync(
        NuaSpaContext context,
        string slikaUrl,
        KategorijaUsluga catMassage,
        KategorijaUsluga catFacial,
        KategorijaUsluga catBody,
        CancellationToken ct)
    {
        var seedDate = DateTime.UtcNow;
        var templates = new[]
        {
            ("Swedish Massage 60", catMassage.Id, 80m, 60, "Klasična švedska masaža cijelog tijela."),
            ("Deep Tissue 45", catMassage.Id, 95m, 45, "Dubinska masaža za napete mišiće."),
            ("Hot Stone 75", catMassage.Id, 120m, 75, "Masaža vrućim kamenjem."),
            ("Aromatherapy 60", catMassage.Id, 90m, 60, "Aromaterapijska relaks masaža."),
            ("Hydrating Facial", catFacial.Id, 70m, 50, "Hidratantni tretman lica."),
            ("Anti-Age Facial", catFacial.Id, 110m, 60, "Anti-age tretman s vitaminom C."),
            ("Body Scrub", catBody.Id, 65m, 45, "Piling cijelog tijela."),
            ("Detox Body Wrap", catBody.Id, 85m, 55, "Detoks omot tijela."),
            ("Couples Massage", catMassage.Id, 150m, 60, "Masaža za parove u istoj prostoriji."),
            ("Express Back Massage", catMassage.Id, 45m, 30, "Brza masaža leđa."),
        };

        foreach (var (naziv, katId, cijena, trajanje, opis) in templates)
        {
            if (await context.Usluge.AnyAsync(u => u.Naziv == naziv && !u.IsDeleted, ct))
            {
                continue;
            }

            context.Usluge.Add(new Usluga
            {
                Naziv = naziv,
                KategorijaUslugaId = katId,
                Cijena = cijena,
                TrajanjeMinuta = trajanje,
                Opis = opis,
                SlikaUrl = slikaUrl,
                CreatedAt = seedDate,
                IsDeleted = false,
            });
        }

        await context.SaveChangesAsync(ct);
        return await context.Usluge.Where(u => !u.IsDeleted).ToListAsync(ct);
    }

    private static async Task<List<Korisnik>> EnsureExtraClientsAsync(
        UserManager<Korisnik> userManager,
        NuaSpaContext context,
        CancellationToken ct)
    {
        var clients = new List<Korisnik>();
        var specs = new[]
        {
            ("marko", "marko@test.ba", "Marko", "Kovač", 1),
            ("ajla", "ajla@test.ba", "Ajla", "Begić", 1),
            ("dino", "dino@test.ba", "Dino", "Salihović", 3),
            ("mina", "mina@test.ba", "Mina", "Čović", 3),
            ("selma", "selma@test.ba", "Selma", "Džafić", 1),
        };

        foreach (var (user, email, ime, prezime, gradId) in specs)
        {
            var k = await userManager.FindByNameAsync(user);
            if (k == null)
            {
                k = new Korisnik
                {
                    UserName = user,
                    Email = email,
                    Ime = ime,
                    Prezime = prezime,
                    GradId = gradId,
                    Status = true,
                    DatumRegistracije = DateTime.UtcNow,
                };
                var create = await userManager.CreateAsync(k, "Klijent123!");
                if (!create.Succeeded)
                {
                    continue;
                }

                await userManager.AddToRoleAsync(k, RoleConstants.Klijent);
            }

            clients.Add(k);
        }

        var lana = await userManager.FindByNameAsync("lana");
        if (lana != null)
        {
            clients.Add(lana);
        }

        return clients.DistinctBy(c => c.Id).ToList();
    }

    private static async Task EnsureProizvodiAsync(NuaSpaContext context, CancellationToken ct)
    {
        if (await context.Proizvodi.AnyAsync(ct))
        {
            return;
        }

        var seedDate = DateTime.UtcNow;
        var products = new[]
        {
            ("NS-OL-001", "Lavanda ulje 50ml", 24.90m),
            ("NS-CR-002", "Hidratantna krema", 32.00m),
            ("NS-SC-003", "Piling za tijelo", 28.50m),
            ("NS-TE-004", "Čaj za relaks", 12.00m),
            ("NS-GF-005", "Poklon set", 89.00m),
        };

        foreach (var (sifra, naziv, cijena) in products)
        {
            var p = new Proizvod
            {
                Sifra = sifra,
                Naziv = naziv,
                Cijena = cijena,
                Opis = naziv,
                CreatedAt = seedDate,
                IsDeleted = false,
            };
            context.Proizvodi.Add(p);
            context.Skladista.Add(new Skladiste
            {
                Proizvod = p,
                KolicinaNaStanju = 20,
                Lokacija = "Glavno skladište",
                CreatedAt = seedDate,
                IsDeleted = false,
            });
        }

        await context.SaveChangesAsync(ct);
    }

    private static async Task EnsureRezervacijeAndRecenzijeAsync(
        NuaSpaContext context,
        List<Korisnik> clients,
        List<Zaposlenik> therapists,
        List<Usluga> usluge,
        CancellationToken ct)
    {
        var rng = new Random(210240);
        var now = DateTime.UtcNow;
        var prostorija = await context.Prostorije.FirstOrDefaultAsync(ct);

        if (await context.Rezervacije.CountAsync(r => !r.IsDeleted, ct) < 15)
        {
            for (var i = 0; i < 18; i++)
            {
                var client = clients[rng.Next(clients.Count)];
                var usluga = usluge[rng.Next(usluge.Count)];
                var eligible = therapists
                    .Where(t => TherapistServiceEligibility.Matches(usluga, t))
                    .ToList();
                var terapeut = eligible.Count > 0
                    ? eligible[rng.Next(eligible.Count)]
                    : therapists[rng.Next(therapists.Count)];

                var daysOffset = rng.Next(-14, 21);
                var hour = rng.Next(9, 17);
                var datum = now.Date.AddDays(daysOffset).AddHours(hour);
                var isPast = daysOffset < 0;

                var rez = new Rezervacija
                {
                    KorisnikId = client.Id,
                    UslugaId = usluga.Id,
                    ZaposlenikId = terapeut.Id,
                    ProstorijaId = prostorija?.Id,
                    DatumRezervacije = datum,
                    Status = isPast ? RezervacijaStatus.Completed : RezervacijaStatus.Pending,
                    IsPotvrdjena = true,
                    IsPlacena = isPast && rng.NextDouble() > 0.3,
                    IsOtkazana = false,
                    CreatedAt = now,
                    IsDeleted = false,
                };
                context.Rezervacije.Add(rez);
            }

            await context.SaveChangesAsync(ct);
        }

        if (await context.Recenzije.CountAsync(r => !r.IsDeleted, ct) >= 8)
        {
            return;
        }

        var pastRez = await context.Rezervacije
            .Where(r =>
                !r.IsDeleted
                && r.DatumRezervacije < now
                && r.Status == RezervacijaStatus.Completed)
            .Take(12)
            .ToListAsync(ct);

        foreach (var rez in pastRez)
        {
            if (await context.Recenzije.AnyAsync(
                    r => r.KorisnikId == rez.KorisnikId
                        && r.UslugaId == rez.UslugaId
                        && r.ZaposlenikId == rez.ZaposlenikId,
                    ct))
            {
                continue;
            }

            context.Recenzije.Add(new Recenzija
            {
                KorisnikId = rez.KorisnikId,
                UslugaId = rez.UslugaId,
                ZaposlenikId = rez.ZaposlenikId,
                Ocjena = rng.Next(3, 6),
                Komentar = "Excellent treatment — highly recommend the NuaSpa team.",
                CreatedAt = rez.DatumRezervacije.AddHours(2),
                IsDeleted = false,
            });
        }

        foreach (var client in clients.Take(3))
        {
            var favUsluga = usluge[rng.Next(usluge.Count)];
            if (!await context.Favoriti.AnyAsync(f => f.KorisnikId == client.Id && f.UslugaId == favUsluga.Id, ct))
            {
                context.Favoriti.Add(new Favorit
                {
                    KorisnikId = client.Id,
                    UslugaId = favUsluga.Id,
                    CreatedAt = now,
                    IsDeleted = false,
                });
            }
        }

        await context.SaveChangesAsync(ct);
    }
}

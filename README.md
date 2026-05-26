# NuaSpa Backend

ASP.NET Core 9 Web API za NuaSpa spa management sistem.

## Preduvjeti

- .NET 9 SDK
- SQL Server (LocalDB ili Docker)
- RabbitMQ (opcionalno, za e-mail notifikacije preko Worker servisa)
- Stripe ključevi (opcionalno, za online plaćanje)

## Konfiguracija

Kopiraj `.env.example` u `.env`:

```bash
cp .env.example .env
```

Ključne varijable:

| Varijabla | Opis |
|-----------|------|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string |
| `ASPNETCORE_URLS` | npr. `http://localhost:5088` |
| `JwtSettings__Key` | JWT signing key (min. 32 znaka) |
| `Stripe__SecretKey` / `Stripe__WebhookSecret` | Stripe integracija |
| `RabbitMQ__*` | Queue za e-mail worker |

Migracije se primjenjuju automatski pri pokretanju API-ja u Development okruženju.

Ručno:

```bash
dotnet ef database update --project src/NuaSpa.Infrastructure --startup-project src/NuaSpa.Api
```

## Pokretanje

```bash
dotnet run --project src/NuaSpa.Api
```

Swagger UI (Development): `http://localhost:5088/`

## Dev korisnici (Development seeder)

| Korisnik | Lozinka | Uloga |
|----------|---------|-------|
| `admin` | `Admin123!` | Admin |
| `lana` | `Lana123!` | Klijent |
| `therapist` | `Therapist123!` | Terapeut |

## Arhitektura

| Projekt | Svrha |
|---------|--------|
| `NuaSpa.Api` | REST API, SignalR hub (`/hubs/notifications`) |
| `NuaSpa.Application` | Poslovna logika, servisi, DTO-ovi |
| `NuaSpa.Domain` | Entiteti, EF konfiguracije |
| `NuaSpa.Infrastructure` | Migracije |
| `NuaSpa.Worker` | RabbitMQ consumer za e-mail |

## Glavni API moduli

- `Rezervacija` — state machine (Pending → Confirmed → Completed / Cancelled)
- `Placanje` — Stripe PaymentIntent, confirm, refund, webhook
- `SistemskaNotifikacija` — in-app inbox (pročitano/nepročitano)
- `Obavijest` — news feed (naslov, tekst, slika, datum)
- `Usluga`, `Zaposlenik`, `Recenzija`, `AdminFinance`, `Izvjestaj`

## Repozitorij

Flutter klijent: [NuaSpa-App](https://github.com/lana-mustafic/NuaSpa-App)

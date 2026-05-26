using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Common;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.Services;

public class SistemskaNotifikacijaService : ISistemskaNotifikacijaService
{
    private readonly NuaSpaContext _context;
    private readonly INotificationPushService _push;

    public SistemskaNotifikacijaService(NuaSpaContext context, INotificationPushService push)
    {
        _context = context;
        _push = push;
    }

    public async Task NotifyUsersAsync(
        IEnumerable<int> korisnikIds,
        SistemskaNotifikacijaTip tip,
        string naslov,
        string tekst,
        int? rezervacijaId = null,
        CancellationToken ct = default)
    {
        var ids = korisnikIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var id in ids)
        {
            _context.SistemskaNotifikacije.Add(new SistemskaNotifikacija
            {
                KorisnikId = id,
                Tip = tip,
                Naslov = naslov.Trim(),
                Tekst = tekst.Trim(),
                Procitana = false,
                RezervacijaId = rezervacijaId,
                CreatedAt = now,
            });
        }

        await _context.SaveChangesAsync(ct);

        foreach (var id in ids)
        {
            await _push.PushUpdatedAsync(id, ct);
        }
    }

    public async Task NotifyRezervacijaKreiranaAsync(Rezervacija rezervacija, CancellationToken ct = default)
    {
        var info = await LoadRezervacijaInfoAsync(rezervacija, ct);
        var dt = FormatDt(info.Datum);

        await NotifyUsersAsync(
            new[] { info.KorisnikId },
            SistemskaNotifikacijaTip.RezervacijaKreirana,
            "Rezervacija zaprimljena",
            $"Vaša rezervacija za {info.UslugaNaziv} ({dt}) je zaprimljena i čeka potvrdu.",
            info.Id,
            ct);

        var adminIds = await GetAdminUserIdsAsync(ct);
        await NotifyUsersAsync(
            adminIds,
            SistemskaNotifikacijaTip.RezervacijaKreirana,
            "Nova rezervacija",
            $"Novi termin: {info.UslugaNaziv} — {info.KlijentIme} ({dt}).",
            info.Id,
            ct);

        var therapistUserId = await GetTherapistUserIdAsync(info.ZaposlenikId, ct);
        if (therapistUserId.HasValue)
        {
            await NotifyUsersAsync(
                new[] { therapistUserId.Value },
                SistemskaNotifikacijaTip.RezervacijaKreirana,
                "Novi termin",
                $"Novi termin: {info.UslugaNaziv} — {info.KlijentIme} ({dt}).",
                info.Id,
                ct);
        }
    }

    public async Task NotifyRezervacijaPotvrdenaAsync(Rezervacija rezervacija, CancellationToken ct = default)
    {
        var info = await LoadRezervacijaInfoAsync(rezervacija, ct);
        var dt = FormatDt(info.Datum);

        await NotifyUsersAsync(
            new[] { info.KorisnikId },
            SistemskaNotifikacijaTip.RezervacijaPotvrdena,
            "Rezervacija potvrđena",
            $"Vaš termin za {info.UslugaNaziv} ({dt}) je potvrđen.",
            info.Id,
            ct);

        var therapistUserId = await GetTherapistUserIdAsync(info.ZaposlenikId, ct);
        if (therapistUserId.HasValue)
        {
            await NotifyUsersAsync(
                new[] { therapistUserId.Value },
                SistemskaNotifikacijaTip.StatusPromjena,
                "Termin potvrđen",
                $"Termin {info.UslugaNaziv} ({dt}) je potvrđen.",
                info.Id,
                ct);
        }
    }

    public async Task NotifyRezervacijaOtkazanaAsync(
        Rezervacija rezervacija,
        string razlog,
        CancellationToken ct = default)
    {
        var info = await LoadRezervacijaInfoAsync(rezervacija, ct);
        var dt = FormatDt(info.Datum);
        var reason = string.IsNullOrWhiteSpace(razlog) ? "—" : razlog.Trim();

        await NotifyUsersAsync(
            new[] { info.KorisnikId },
            SistemskaNotifikacijaTip.RezervacijaOtkazana,
            "Rezervacija otkazana",
            $"Termin za {info.UslugaNaziv} ({dt}) je otkazan. Razlog: {reason}",
            info.Id,
            ct);

        var adminIds = await GetAdminUserIdsAsync(ct);
        await NotifyUsersAsync(
            adminIds,
            SistemskaNotifikacijaTip.RezervacijaOtkazana,
            "Otkazana rezervacija",
            $"Otkazan termin {info.UslugaNaziv} — {info.KlijentIme}. Razlog: {reason}",
            info.Id,
            ct);

        var therapistUserId = await GetTherapistUserIdAsync(info.ZaposlenikId, ct);
        if (therapistUserId.HasValue)
        {
            await NotifyUsersAsync(
                new[] { therapistUserId.Value },
                SistemskaNotifikacijaTip.RezervacijaOtkazana,
                "Termin otkazan",
                $"Termin {info.UslugaNaziv} ({dt}) je otkazan.",
                info.Id,
                ct);
        }
    }

    public async Task NotifyRezervacijaZavrsenaAsync(Rezervacija rezervacija, CancellationToken ct = default)
    {
        var info = await LoadRezervacijaInfoAsync(rezervacija, ct);
        var dt = FormatDt(info.Datum);

        await NotifyUsersAsync(
            new[] { info.KorisnikId },
            SistemskaNotifikacijaTip.RezervacijaZavrsena,
            "Termin završen",
            $"Tretman {info.UslugaNaziv} ({dt}) je označen kao završen.",
            info.Id,
            ct);
    }

    public async Task NotifyPlacanjeUspjesnoAsync(
        Rezervacija rezervacija,
        decimal iznos,
        CancellationToken ct = default)
    {
        var info = await LoadRezervacijaInfoAsync(rezervacija, ct);
        var amount = iznos.ToString("0.00", CultureInfo.InvariantCulture);

        await NotifyUsersAsync(
            new[] { info.KorisnikId },
            SistemskaNotifikacijaTip.PlacanjeUspjesno,
            "Plaćanje uspješno",
            $"Uspješno plaćeno {amount} KM za {info.UslugaNaziv}.",
            info.Id,
            ct);

        var adminIds = await GetAdminUserIdsAsync(ct);
        await NotifyUsersAsync(
            adminIds,
            SistemskaNotifikacijaTip.PlacanjeUspjesno,
            "Novo plaćanje",
            $"Plaćeno {amount} KM — {info.KlijentIme}, {info.UslugaNaziv}.",
            info.Id,
            ct);
    }

    public async Task NotifyPlacanjeRefundiranoAsync(
        Rezervacija rezervacija,
        decimal iznos,
        CancellationToken ct = default)
    {
        var info = await LoadRezervacijaInfoAsync(rezervacija, ct);
        var amount = iznos.ToString("0.00", CultureInfo.InvariantCulture);

        await NotifyUsersAsync(
            new[] { info.KorisnikId },
            SistemskaNotifikacijaTip.PlacanjeRefundirano,
            "Povrat sredstava",
            $"Refund {amount} KM za {info.UslugaNaziv} je obrađen.",
            info.Id,
            ct);

        var adminIds = await GetAdminUserIdsAsync(ct);
        await NotifyUsersAsync(
            adminIds,
            SistemskaNotifikacijaTip.PlacanjeRefundirano,
            "Refund izvršen",
            $"Refund {amount} KM — {info.KlijentIme}, {info.UslugaNaziv}.",
            info.Id,
            ct);
    }

    public async Task<IReadOnlyList<SistemskaNotifikacijaDto>> GetForUserAsync(
        int korisnikId,
        int take,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 100);

        return await _context.SistemskaNotifikacije
            .AsNoTracking()
            .Where(n => n.KorisnikId == korisnikId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .Select(n => new SistemskaNotifikacijaDto
            {
                Id = n.Id,
                Tip = n.Tip,
                Naslov = n.Naslov,
                Tekst = n.Tekst,
                Procitana = n.Procitana,
                DatumVrijeme = n.CreatedAt,
                RezervacijaId = n.RezervacijaId,
            })
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(int korisnikId, CancellationToken ct = default)
    {
        return await _context.SistemskaNotifikacije
            .AsNoTracking()
            .Where(n => n.KorisnikId == korisnikId && !n.IsDeleted && !n.Procitana)
            .CountAsync(ct);
    }

    public async Task<bool> MarkReadAsync(int korisnikId, int notifikacijaId, CancellationToken ct = default)
    {
        var entity = await _context.SistemskaNotifikacije
            .FirstOrDefaultAsync(n => n.Id == notifikacijaId && n.KorisnikId == korisnikId && !n.IsDeleted, ct);

        if (entity == null)
        {
            return false;
        }

        if (!entity.Procitana)
        {
            entity.Procitana = true;
            await _context.SaveChangesAsync(ct);
            await _push.PushUpdatedAsync(korisnikId, ct);
        }

        return true;
    }

    public async Task MarkAllReadAsync(int korisnikId, CancellationToken ct = default)
    {
        var unread = await _context.SistemskaNotifikacije
            .Where(n => n.KorisnikId == korisnikId && !n.IsDeleted && !n.Procitana)
            .ToListAsync(ct);

        if (unread.Count == 0)
        {
            return;
        }

        foreach (var n in unread)
        {
            n.Procitana = true;
        }

        await _context.SaveChangesAsync(ct);
        await _push.PushUpdatedAsync(korisnikId, ct);
    }

    private async Task<List<int>> GetAdminUserIdsAsync(CancellationToken ct)
    {
        return await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.RoleId == RoleNames.AdminRoleId)
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(ct);
    }

    private async Task<int?> GetTherapistUserIdAsync(int zaposlenikId, CancellationToken ct)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.ZaposlenikId == zaposlenikId)
            .Select(u => (int?)u.Id)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<RezervacijaInfo> LoadRezervacijaInfoAsync(Rezervacija rezervacija, CancellationToken ct)
    {
        if (rezervacija.Korisnik != null && rezervacija.Usluga != null)
        {
            return new RezervacijaInfo(
                rezervacija.Id,
                rezervacija.KorisnikId,
                rezervacija.ZaposlenikId,
                rezervacija.DatumRezervacije,
                rezervacija.Usluga.Naziv,
                $"{rezervacija.Korisnik.Ime} {rezervacija.Korisnik.Prezime}".Trim());
        }

        var row = await _context.Rezervacije
            .AsNoTracking()
            .Include(r => r.Korisnik)
            .Include(r => r.Usluga)
            .FirstAsync(r => r.Id == rezervacija.Id, ct);

        return new RezervacijaInfo(
            row.Id,
            row.KorisnikId,
            row.ZaposlenikId,
            row.DatumRezervacije,
            row.Usluga?.Naziv ?? "Usluga",
            row.Korisnik == null
                ? "Klijent"
                : $"{row.Korisnik.Ime} {row.Korisnik.Prezime}".Trim());
    }

    private static string FormatDt(DateTime dt) =>
        dt.ToLocalTime().ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

    private sealed record RezervacijaInfo(
        int Id,
        int KorisnikId,
        int ZaposlenikId,
        DateTime Datum,
        string UslugaNaziv,
        string KlijentIme);
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.SearchObjects;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminKlijentController : ControllerBase
{
    private const string KlijentRoleNormalized = "KLIJENT";

    private readonly NuaSpaContext _context;
    private readonly UserManager<Korisnik> _userManager;

    public AdminKlijentController(NuaSpaContext context, UserManager<Korisnik> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<int?> GetKlijentRoleIdAsync(CancellationToken ct = default)
    {
        return await _context.Roles.AsNoTracking()
            .Where(r => r.NormalizedName == KlijentRoleNormalized)
            .Select(r => (int?)r.Id)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Agregati za dashboard (opcionalno isti tekstualni filter kao lista).
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<AdminClientStatsDto>> Stats(
        [FromQuery] string? q = null,
        CancellationToken ct = default)
    {
        var roleId = await GetKlijentRoleIdAsync(ct);
        if (roleId == null) return Ok(new AdminClientStatsDto());

        var baseQuery = KlijentiQuery(roleId.Value, search: null, q);

        var users = await baseQuery.Select(k => new { k.Id, k.IsVipKlijent }).ToListAsync(ct);
        var ids = users.Select(u => u.Id).ToList();
        if (ids.Count == 0)
            return Ok(new AdminClientStatsDto());

        var vipDict = users.ToDictionary(u => u.Id, u => u.IsVipKlijent);
        var (visitMap, spentMap) = await BuildAggMapsAsync(ids, ct);

        var stats = new AdminClientStatsDto
        {
            UkupnoKlijenata = ids.Count,
            UkupnoPosjeta = 0,
            UkupnaPotrosnja = 0m,
            VipKlijenata = 0,
        };

        foreach (var id in ids)
        {
            visitMap.TryGetValue(id, out var v);
            spentMap.TryGetValue(id, out var spent);
            var visits = v?.UkupnoPosjeta ?? 0;
            var total = spent;
            var manual = vipDict.GetValueOrDefault(id);
            var computedVip = manual || visits >= 10 || total >= 600m;
            if (computedVip) stats.VipKlijenata++;
            stats.UkupnoPosjeta += visits;
            stats.UkupnaPotrosnja += total;
        }

        return Ok(stats);
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminClientRowDTO>>> Get(
        [FromQuery] KorisnikSearchObject? search = null,
        [FromQuery] string? q = null,
        [FromQuery] int take = 500,
        CancellationToken ct = default)
    {
        var roleId = await GetKlijentRoleIdAsync(ct);
        if (roleId == null) return Ok(new List<AdminClientRowDTO>());

        var safeTake = take <= 0 ? 500 : Math.Min(take, 1000);

        var query = KlijentiQuery(roleId.Value, search, q)
            .OrderByDescending(k => k.DatumRegistracije);

        var rows = await query
            .Select(k => new
            {
                k.Id,
                k.Ime,
                k.Prezime,
                Email = k.Email ?? "",
                Telefon = k.PhoneNumber ?? "",
                k.DatumRegistracije,
                PreferiraniZaposlenikId = k.ZaposlenikId,
                k.IsVipKlijent,
                k.Status,
            })
            .Take(safeTake)
            .ToListAsync(ct);

        var ids = rows.Select(x => x.Id).ToList();
        if (ids.Count == 0) return Ok(new List<AdminClientRowDTO>());

        var (visitMap, spentMap) = await BuildAggMapsAsync(ids, ct);
        var lastTherapist = await BuildLastTherapistMapAsync(ids, ct);

        var prefIds = rows
            .Select(r => r.PreferiraniZaposlenikId)
            .Where(id => id is > 0)
            .Cast<int>()
            .Distinct()
            .ToList();
        var prefMap = prefIds.Count == 0
            ? new Dictionary<int, (string Ime, string Prezime)>()
            : await _context.Zaposlenici.AsNoTracking()
                .Where(z => prefIds.Contains(z.Id))
                .ToDictionaryAsync(z => z.Id, z => (z.Ime, z.Prezime), ct);

        var result = rows.Select(r =>
        {
            visitMap.TryGetValue(r.Id, out var a);
            spentMap.TryGetValue(r.Id, out var ukupno);
            var visits = a?.UkupnoPosjeta ?? 0;
            var total = ukupno;
            var computedVip = r.IsVipKlijent || visits >= 10 || total >= 600m;

            int? terId = null;
            string? tIme = null;
            string? tPrez = null;

            if (r.PreferiraniZaposlenikId is > 0 &&
                prefMap.TryGetValue(r.PreferiraniZaposlenikId.Value, out var pref))
            {
                terId = r.PreferiraniZaposlenikId;
                tIme = pref.Ime;
                tPrez = pref.Prezime;
            }
            else if (lastTherapist.TryGetValue(r.Id, out var lt))
            {
                terId = lt.ZaposlenikId;
                tIme = lt.Ime;
                tPrez = lt.Prezime;
            }

            return new AdminClientRowDTO
            {
                Id = r.Id,
                Ime = r.Ime,
                Prezime = r.Prezime,
                Email = r.Email,
                Telefon = r.Telefon,
                DatumRegistracije = r.DatumRegistracije,
                ZadnjaPosjeta = a?.ZadnjaPosjeta,
                UkupnoPosjeta = visits,
                UkupnoPotroseno = total,
                IsVip = computedVip,
                PreferiraniZaposlenikId = r.PreferiraniZaposlenikId,
                TerapeutZaposlenikId = terId,
                TerapeutIme = tIme,
                TerapeutPrezime = tPrez,
                IsVipKlijent = r.IsVipKlijent,
                Status = r.Status,
            };
        }).ToList();

        return Ok(result);
    }

    private IQueryable<Korisnik> KlijentiQuery(
        int klijentRoleId,
        KorisnikSearchObject? search,
        string? q)
    {
        var query = _context.Users.AsNoTracking()
            .Where(k => _context.UserRoles.Any(ur => ur.UserId == k.Id && ur.RoleId == klijentRoleId));

        var qq = (search?.Q ?? q)?.Trim();
        if (!string.IsNullOrWhiteSpace(qq))
        {
            var t = qq.ToLower();
            query = query.Where(k =>
                (k.Ime + " " + k.Prezime).ToLower().Contains(t) ||
                (k.Email ?? "").ToLower().Contains(t) ||
                (k.UserName ?? "").ToLower().Contains(t) ||
                (k.PhoneNumber ?? "").ToLower().Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(search?.Ime))
        {
            var ime = search.Ime.Trim();
            query = query.Where(k => k.Ime.Contains(ime));
        }

        if (!string.IsNullOrWhiteSpace(search?.Prezime))
        {
            var prezime = search.Prezime.Trim();
            query = query.Where(k => k.Prezime.Contains(prezime));
        }

        return query;
    }

    private sealed record VisitAgg(int UkupnoPosjeta, DateTime? ZadnjaPosjeta);

    private async Task<(Dictionary<int, VisitAgg> visitMap, Dictionary<int, decimal> spentMap)> BuildAggMapsAsync(
        List<int> ids,
        CancellationToken ct)
    {
        var visitMap = await _context.Rezervacije.AsNoTracking()
            .Where(r => !r.IsOtkazana && ids.Contains(r.KorisnikId))
            .GroupBy(r => r.KorisnikId)
            .Select(g => new
            {
                KorisnikId = g.Key,
                UkupnoPosjeta = g.Count(),
                ZadnjaPosjeta = g.Max(x => x.DatumRezervacije),
            })
            .ToDictionaryAsync(x => x.KorisnikId, x => new VisitAgg(x.UkupnoPosjeta, x.ZadnjaPosjeta), ct);

        var spentMap = await _context.Rezervacije.AsNoTracking()
            .Where(r => r.IsPlacena && !r.IsOtkazana && ids.Contains(r.KorisnikId))
            .Join(
                _context.Usluge.AsNoTracking(),
                r => r.UslugaId,
                u => u.Id,
                (r, u) => new { r.KorisnikId, u.Cijena }
            )
            .GroupBy(x => x.KorisnikId)
            .Select(g => new { KorisnikId = g.Key, UkupnoPotroseno = g.Sum(x => x.Cijena) })
            .ToDictionaryAsync(x => x.KorisnikId, x => x.UkupnoPotroseno, ct);

        return (visitMap, spentMap);
    }

    private sealed record LastTherapistRow(int ZaposlenikId, string Ime, string Prezime);

    private async Task<Dictionary<int, LastTherapistRow>> BuildLastTherapistMapAsync(List<int> ids, CancellationToken ct)
    {
        var rez = await _context.Rezervacije.AsNoTracking()
            .Where(r => !r.IsOtkazana && ids.Contains(r.KorisnikId))
            .Select(r => new
            {
                r.KorisnikId,
                r.DatumRezervacije,
                r.ZaposlenikId,
                TerapeutIme = r.Zaposlenik.Ime,
                TerapeutPrezime = r.Zaposlenik.Prezime,
            })
            .ToListAsync(ct);

        return rez
            .GroupBy(x => x.KorisnikId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var x = g.OrderByDescending(t => t.DatumRezervacije).First();
                    return new LastTherapistRow(x.ZaposlenikId, x.TerapeutIme, x.TerapeutPrezime);
                });
    }

    [HttpPost]
    public async Task<ActionResult<AdminClientRowDTO>> Create(
        [FromBody] AdminKlijentCreateDto dto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var roleId = await GetKlijentRoleIdAsync(ct);
        if (roleId == null) return BadRequest("Uloga Klijent nije pronađena u bazi.");

        var existsMail = await _userManager.FindByEmailAsync(dto.Email.Trim());
        if (existsMail != null) return Conflict("Email je već registriran.");

        var existsUser = await _userManager.FindByNameAsync(dto.UserName.Trim());
        if (existsUser != null) return Conflict("Korisničko ime je zauzeto.");

        if (dto.ZaposlenikId is > 0)
        {
            var zOk = await _context.Zaposlenici.AsNoTracking().AnyAsync(z => z.Id == dto.ZaposlenikId, ct);
            if (!zOk) return BadRequest("ZaposlenikId ne postoji.");
        }

        var gradOk = await _context.Gradovi.AsNoTracking().AnyAsync(g => g.Id == dto.GradId, ct);
        if (!gradOk) return BadRequest("GradId ne postoji.");

        var user = new Korisnik
        {
            UserName = dto.UserName.Trim(),
            Email = dto.Email.Trim(),
            Ime = dto.Ime.Trim(),
            Prezime = dto.Prezime.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(dto.Telefon) ? null : dto.Telefon.Trim(),
            GradId = dto.GradId,
            DatumRegistracije = DateTime.UtcNow,
            Status = true,
            ZaposlenikId = dto.ZaposlenikId,
            IsVipKlijent = dto.IsVipKlijent,
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description).ToList());

        await _userManager.AddToRoleAsync(user, "Klijent");

        var list = await GetInternalRowsForIdsAsync(new List<int> { user.Id }, ct);
        return Ok(list[0]);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminClientRowDTO>> GetById(int id, CancellationToken ct = default)
    {
        var roleId = await GetKlijentRoleIdAsync(ct);
        if (roleId == null) return NotFound();

        var isClient = await _context.UserRoles.AsNoTracking()
            .AnyAsync(ur => ur.UserId == id && ur.RoleId == roleId.Value, ct);
        if (!isClient) return NotFound();

        var rows = await GetInternalRowsForIdsAsync(new List<int> { id }, ct);
        if (rows.Count == 0) return NotFound();
        return Ok(rows[0]);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<AdminClientRowDTO>> Patch(
        int id,
        [FromBody] AdminKlijentUpdateDto dto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "Klijent"))
            return BadRequest("User is not a client.");

        if (dto.Status == false)
        {
            var hasUpcoming = await _context.Rezervacije.AsNoTracking()
                .AnyAsync(
                    r => r.KorisnikId == id && !r.IsOtkazana && r.DatumRezervacije > DateTime.UtcNow,
                    ct);
            if (hasUpcoming)
                return Conflict("Client has upcoming appointments. Cancel or reschedule them first.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Ime))
            user.Ime = dto.Ime.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Prezime))
            user.Prezime = dto.Prezime.Trim();

        if (dto.Email != null)
        {
            var email = dto.Email.Trim();
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email cannot be empty.");

            var existingMail = await _userManager.FindByEmailAsync(email);
            if (existingMail != null && existingMail.Id != user.Id)
                return Conflict("Email is already registered.");

            var setMail = await _userManager.SetEmailAsync(user, email);
            if (!setMail.Succeeded)
                return BadRequest(setMail.Errors.Select(e => e.Description).ToList());
        }

        if (dto.Telefon != null)
            user.PhoneNumber = string.IsNullOrWhiteSpace(dto.Telefon) ? null : dto.Telefon.Trim();

        if (dto.Status.HasValue)
            user.Status = dto.Status.Value;

        if (dto.IsVipKlijent.HasValue) user.IsVipKlijent = dto.IsVipKlijent.Value;

        if (dto.ZaposlenikId.HasValue)
        {
            if (dto.ZaposlenikId.Value <= 0) user.ZaposlenikId = null;
            else
            {
                var zOk = await _context.Zaposlenici.AsNoTracking()
                    .AnyAsync(z => z.Id == dto.ZaposlenikId.Value, ct);
                if (!zOk) return BadRequest("Therapist id does not exist.");
                user.ZaposlenikId = dto.ZaposlenikId.Value;
            }
        }

        if (dto.NapomenaZaTerapeuta != null)
            user.NapomenaZaTerapeuta = dto.NapomenaZaTerapeuta;

        var upd = await _userManager.UpdateAsync(user);
        if (!upd.Succeeded)
            return BadRequest(upd.Errors.Select(e => e.Description).ToList());

        var list = await GetInternalRowsForIdsAsync(new List<int> { id }, ct);
        return Ok(list[0]);
    }

    /// <summary>
    /// Ponovno učitava jednog klijenta u istom formatu kao GET lista.
    /// </summary>
    private async Task<List<AdminClientRowDTO>> GetInternalRowsForIdsAsync(List<int> ids, CancellationToken ct)
    {
        var rows = await _context.Users.AsNoTracking()
            .Where(k => ids.Contains(k.Id))
            .Select(k => new
            {
                k.Id,
                k.Ime,
                k.Prezime,
                Email = k.Email ?? "",
                Telefon = k.PhoneNumber ?? "",
                k.DatumRegistracije,
                PreferiraniZaposlenikId = k.ZaposlenikId,
                k.IsVipKlijent,
                k.Status,
            })
            .ToListAsync(ct);

        var (visitMap, spentMap) = await BuildAggMapsAsync(ids, ct);
        var lastTherapist = await BuildLastTherapistMapAsync(ids, ct);

        var prefIds = rows
            .Select(r => r.PreferiraniZaposlenikId)
            .Where(id => id is > 0)
            .Cast<int>()
            .Distinct()
            .ToList();
        var prefMap = prefIds.Count == 0
            ? new Dictionary<int, (string Ime, string Prezime)>()
            : await _context.Zaposlenici.AsNoTracking()
                .Where(z => prefIds.Contains(z.Id))
                .ToDictionaryAsync(z => z.Id, z => (z.Ime, z.Prezime), ct);

        var result = rows.Select(r =>
        {
            visitMap.TryGetValue(r.Id, out var a);
            spentMap.TryGetValue(r.Id, out var ukupno);
            var visits = a?.UkupnoPosjeta ?? 0;
            var total = ukupno;
            var computedVip = r.IsVipKlijent || visits >= 10 || total >= 600m;

            int? terId = null;
            string? tIme = null;
            string? tPrez = null;

            if (r.PreferiraniZaposlenikId is > 0 &&
                prefMap.TryGetValue(r.PreferiraniZaposlenikId.Value, out var pref))
            {
                terId = r.PreferiraniZaposlenikId;
                tIme = pref.Ime;
                tPrez = pref.Prezime;
            }
            else if (lastTherapist.TryGetValue(r.Id, out var lt))
            {
                terId = lt.ZaposlenikId;
                tIme = lt.Ime;
                tPrez = lt.Prezime;
            }

            return new AdminClientRowDTO
            {
                Id = r.Id,
                Ime = r.Ime,
                Prezime = r.Prezime,
                Email = r.Email,
                Telefon = r.Telefon,
                DatumRegistracije = r.DatumRegistracije,
                ZadnjaPosjeta = a?.ZadnjaPosjeta,
                UkupnoPosjeta = visits,
                UkupnoPotroseno = total,
                IsVip = computedVip,
                PreferiraniZaposlenikId = r.PreferiraniZaposlenikId,
                TerapeutZaposlenikId = terId,
                TerapeutIme = tIme,
                TerapeutPrezime = tPrez,
                IsVipKlijent = r.IsVipKlijent,
                Status = r.Status,
            };
        }).ToList();

        return result;
    }
}

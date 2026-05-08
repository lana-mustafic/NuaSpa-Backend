using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.DTOs;
using NuaSpa.Domain;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminKlijentController : ControllerBase
{
    private readonly NuaSpaContext _context;

    public AdminKlijentController(NuaSpaContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminClientRowDTO>>> Get(
        [FromQuery] string? q = null,
        [FromQuery] int take = 200)
    {
        var safeTake = take <= 0 ? 200 : Math.Min(take, 1000);
        var query = _context.Users.AsNoTracking();

        var qq = q?.Trim();
        if (!string.IsNullOrWhiteSpace(qq))
        {
            var t = qq.ToLower();
            query = query.Where(k =>
                (k.Ime + " " + k.Prezime).ToLower().Contains(t) ||
                (k.Email ?? "").ToLower().Contains(t) ||
                (k.UserName ?? "").ToLower().Contains(t)
            );
        }

        // Left join agregati iz Rezervacije.
        var agg = _context.Rezervacije.AsNoTracking()
            .Where(r => r.IsPlacena)
            .GroupBy(r => r.KorisnikId)
            .Select(g => new
            {
                KorisnikId = g.Key,
                UkupnoPosjeta = g.Count(),
                ZadnjaPosjeta = g.Max(x => x.DatumRezervacije),
            });

        var spent = _context.Rezervacije.AsNoTracking()
            .Where(r => r.IsPlacena)
            .Join(
                _context.Usluge.AsNoTracking(),
                r => r.UslugaId,
                u => u.Id,
                (r, u) => new { r.KorisnikId, u.Cijena }
            )
            .GroupBy(x => x.KorisnikId)
            .Select(g => new { KorisnikId = g.Key, UkupnoPotroseno = g.Sum(x => x.Cijena) });

        var rows = await query
            .OrderByDescending(k => k.DatumRegistracije)
            .Select(k => new
            {
                k.Id,
                k.Ime,
                k.Prezime,
                Email = k.Email ?? "",
                Telefon = k.PhoneNumber ?? "",
                k.DatumRegistracije,
            })
            .Take(safeTake)
            .ToListAsync();

        var ids = rows.Select(x => x.Id).ToList();
        var aggMap = await agg.Where(a => ids.Contains(a.KorisnikId)).ToDictionaryAsync(a => a.KorisnikId, a => a);
        var spentMap = await spent.Where(s => ids.Contains(s.KorisnikId)).ToDictionaryAsync(s => s.KorisnikId, s => s.UkupnoPotroseno);

        return Ok(rows.Select(r =>
        {
            aggMap.TryGetValue(r.Id, out var a);
            spentMap.TryGetValue(r.Id, out var ukupno);
            var visits = a?.UkupnoPosjeta ?? 0;
            var total = ukupno;
            // Simple VIP heuristic: >= 10 paid visits OR >= 600 KM total.
            var vip = visits >= 10 || total >= 600m;
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
                IsVip = vip,
            };
        }).ToList());
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.DTOs;
using NuaSpa.Domain;

namespace NuaSpa.Api.Controllers;

/// <summary>Referentni podaci (šifarnici) za forme i validaciju.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookupController : ControllerBase
{
    private readonly NuaSpaContext _context;

    public LookupController(NuaSpaContext context)
    {
        _context = context;
    }

    [HttpGet("drzave")]
    [Authorize(Roles = "Admin,Klijent")]
    public async Task<ActionResult<List<DrzavaLookupDto>>> GetDrzave(
        [FromQuery] string? naziv = null,
        CancellationToken ct = default)
    {
        var query = _context.Drzave.AsNoTracking().Where(d => !d.IsDeleted);

        if (!string.IsNullOrWhiteSpace(naziv))
        {
            var t = naziv.Trim();
            query = query.Where(d => d.Naziv.Contains(t));
        }

        var list = await query
            .OrderBy(d => d.Naziv)
            .Select(d => new DrzavaLookupDto { Id = d.Id, Naziv = d.Naziv })
            .ToListAsync(ct);

        return Ok(list);
    }

    [HttpGet("gradovi")]
    [Authorize(Roles = "Admin,Klijent")]
    public async Task<ActionResult<List<GradLookupDto>>> GetGradovi(
        [FromQuery] int? drzavaId = null,
        [FromQuery] string? naziv = null,
        CancellationToken ct = default)
    {
        var query = _context.Gradovi.AsNoTracking()
            .Include(g => g.Drzava)
            .Where(g => !g.IsDeleted);

        if (drzavaId is > 0)
        {
            query = query.Where(g => g.DrzavaId == drzavaId);
        }

        if (!string.IsNullOrWhiteSpace(naziv))
        {
            var t = naziv.Trim();
            query = query.Where(g =>
                g.Naziv.Contains(t) || g.PostanskiBroj.Contains(t));
        }

        var list = await query
            .OrderBy(g => g.Naziv)
            .Select(g => new GradLookupDto
            {
                Id = g.Id,
                Naziv = g.Naziv,
                PostanskiBroj = g.PostanskiBroj,
                DrzavaId = g.DrzavaId,
                DrzavaNaziv = g.Drzava.Naziv,
            })
            .ToListAsync(ct);

        return Ok(list);
    }
}

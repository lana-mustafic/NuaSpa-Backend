using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.DTOs;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ResursiController : ControllerBase
{
    private const int DefaultSpaCentarId = 1;
    private readonly NuaSpaContext _context;

    public ResursiController(NuaSpaContext context)
    {
        _context = context;
    }

    [HttpGet("spa-centar")]
    public async Task<ActionResult<SpaCentarDTO>> GetSpaCentar()
    {
        var entity = await _context.SpaCentri.AsNoTracking().FirstOrDefaultAsync(x => x.Id == DefaultSpaCentarId);
        if (entity == null) return NotFound();
        return Ok(new SpaCentarDTO
        {
            Id = entity.Id,
            Naziv = entity.Naziv,
            Adresa = entity.Adresa,
            Email = entity.Email,
            Telefon = entity.Telefon,
            Opis = entity.Opis
        });
    }

    [HttpPut("spa-centar")]
    public async Task<ActionResult<SpaCentarDTO>> UpdateSpaCentar([FromBody] SpaCentarDTO dto)
    {
        var entity = await _context.SpaCentri.FirstOrDefaultAsync(x => x.Id == DefaultSpaCentarId);
        if (entity == null) return NotFound();

        entity.Naziv = string.IsNullOrWhiteSpace(dto.Naziv) ? entity.Naziv : dto.Naziv.Trim();
        entity.Adresa = dto.Adresa?.Trim();
        entity.Email = dto.Email?.Trim();
        entity.Telefon = dto.Telefon?.Trim();
        entity.Opis = dto.Opis?.Trim();
        await _context.SaveChangesAsync();

        return await GetSpaCentar();
    }

    [HttpGet("radno-vrijeme")]
    public async Task<ActionResult<List<RadnoVrijemeDTO>>> GetRadnoVrijeme()
    {
        var list = await _context.RadnaVremena
            .AsNoTracking()
            .Where(x => x.SpaCentarId == DefaultSpaCentarId)
            .OrderBy(x => x.DanUSedmici)
            .Select(x => new RadnoVrijemeDTO
            {
                Id = x.Id,
                SpaCentarId = x.SpaCentarId,
                DanUSedmici = x.DanUSedmici,
                IsClosed = x.IsClosed,
                OtvaraMin = x.OtvaraMin,
                ZatvaraMin = x.ZatvaraMin
            })
            .ToListAsync();
        return Ok(list);
    }

    [HttpPut("radno-vrijeme")]
    public async Task<ActionResult<List<RadnoVrijemeDTO>>> UpdateRadnoVrijeme([FromBody] List<RadnoVrijemeDTO> items)
    {
        var entities = await _context.RadnaVremena
            .Where(x => x.SpaCentarId == DefaultSpaCentarId)
            .ToListAsync();

        // Update by DanUSedmici (1..7)
        foreach (var dto in items)
        {
            var day = dto.DanUSedmici;
            if (day < 1 || day > 7) continue;
            var e = entities.FirstOrDefault(x => x.DanUSedmici == day);
            if (e == null) continue;

            e.IsClosed = dto.IsClosed;
            e.OtvaraMin = dto.IsClosed ? null : dto.OtvaraMin;
            e.ZatvaraMin = dto.IsClosed ? null : dto.ZatvaraMin;
        }

        await _context.SaveChangesAsync();
        return await GetRadnoVrijeme();
    }

    [HttpGet("prostorije")]
    public async Task<ActionResult<List<ProstorijaDTO>>> GetProstorije()
    {
        var list = await _context.Prostorije
            .AsNoTracking()
            .Where(x => x.SpaCentarId == DefaultSpaCentarId)
            .OrderByDescending(x => x.IsAktivna)
            .ThenBy(x => x.Naziv)
            .Select(x => new ProstorijaDTO
            {
                Id = x.Id,
                SpaCentarId = x.SpaCentarId,
                Naziv = x.Naziv,
                Opis = x.Opis,
                Kapacitet = x.Kapacitet,
                IsAktivna = x.IsAktivna
            })
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("prostorije")]
    public async Task<ActionResult<ProstorijaDTO>> CreateProstorija([FromBody] ProstorijaDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Naziv)) return BadRequest("Naziv je obavezan.");
        var e = new Prostorija
        {
            SpaCentarId = DefaultSpaCentarId,
            Naziv = dto.Naziv.Trim(),
            Opis = dto.Opis?.Trim(),
            Kapacitet = dto.Kapacitet <= 0 ? 1 : dto.Kapacitet,
            IsAktivna = dto.IsAktivna,
            CreatedAt = DateTime.UtcNow
        };
        _context.Prostorije.Add(e);
        await _context.SaveChangesAsync();
        return Ok(new ProstorijaDTO
        {
            Id = e.Id,
            SpaCentarId = e.SpaCentarId,
            Naziv = e.Naziv,
            Opis = e.Opis,
            Kapacitet = e.Kapacitet,
            IsAktivna = e.IsAktivna
        });
    }

    [HttpPut("prostorije/{id}")]
    public async Task<ActionResult> UpdateProstorija(int id, [FromBody] ProstorijaDTO dto)
    {
        var e = await _context.Prostorije.FirstOrDefaultAsync(x => x.Id == id && x.SpaCentarId == DefaultSpaCentarId);
        if (e == null) return NotFound();
        if (!string.IsNullOrWhiteSpace(dto.Naziv)) e.Naziv = dto.Naziv.Trim();
        e.Opis = dto.Opis?.Trim();
        if (dto.Kapacitet > 0) e.Kapacitet = dto.Kapacitet;
        e.IsAktivna = dto.IsAktivna;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("prostorije/{id}")]
    public async Task<ActionResult> DeleteProstorija(int id)
    {
        var e = await _context.Prostorije.FirstOrDefaultAsync(x => x.Id == id && x.SpaCentarId == DefaultSpaCentarId);
        if (e == null) return NotFound();
        _context.Prostorije.Remove(e);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("oprema")]
    public async Task<ActionResult<List<OpremaDTO>>> GetOprema()
    {
        var list = await _context.Oprema
            .AsNoTracking()
            .Where(x => x.SpaCentarId == DefaultSpaCentarId)
            .OrderByDescending(x => x.IsIspravna)
            .ThenBy(x => x.Naziv)
            .Select(x => new OpremaDTO
            {
                Id = x.Id,
                SpaCentarId = x.SpaCentarId,
                Naziv = x.Naziv,
                Napomena = x.Napomena,
                Kolicina = x.Kolicina,
                IsIspravna = x.IsIspravna
            })
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("oprema")]
    public async Task<ActionResult<OpremaDTO>> CreateOprema([FromBody] OpremaDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Naziv)) return BadRequest("Naziv je obavezan.");
        var e = new Oprema
        {
            SpaCentarId = DefaultSpaCentarId,
            Naziv = dto.Naziv.Trim(),
            Napomena = dto.Napomena?.Trim(),
            Kolicina = dto.Kolicina <= 0 ? 1 : dto.Kolicina,
            IsIspravna = dto.IsIspravna,
            CreatedAt = DateTime.UtcNow
        };
        _context.Oprema.Add(e);
        await _context.SaveChangesAsync();
        return Ok(new OpremaDTO
        {
            Id = e.Id,
            SpaCentarId = e.SpaCentarId,
            Naziv = e.Naziv,
            Napomena = e.Napomena,
            Kolicina = e.Kolicina,
            IsIspravna = e.IsIspravna
        });
    }

    [HttpPut("oprema/{id}")]
    public async Task<ActionResult> UpdateOprema(int id, [FromBody] OpremaDTO dto)
    {
        var e = await _context.Oprema.FirstOrDefaultAsync(x => x.Id == id && x.SpaCentarId == DefaultSpaCentarId);
        if (e == null) return NotFound();
        if (!string.IsNullOrWhiteSpace(dto.Naziv)) e.Naziv = dto.Naziv.Trim();
        e.Napomena = dto.Napomena?.Trim();
        if (dto.Kolicina > 0) e.Kolicina = dto.Kolicina;
        e.IsIspravna = dto.IsIspravna;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("oprema/{id}")]
    public async Task<ActionResult> DeleteOprema(int id)
    {
        var e = await _context.Oprema.FirstOrDefaultAsync(x => x.Id == id && x.SpaCentarId == DefaultSpaCentarId);
        if (e == null) return NotFound();
        _context.Oprema.Remove(e);
        await _context.SaveChangesAsync();
        return Ok();
    }
}


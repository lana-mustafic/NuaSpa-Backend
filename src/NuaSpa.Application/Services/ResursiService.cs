using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Application.Services;

public class ResursiService : IResursiService
{
    private const int DefaultSpaCentarId = 1;

    private readonly NuaSpaContext _context;

    public ResursiService(NuaSpaContext context)
    {
        _context = context;
    }

    public async Task<SpaCentarDTO?> GetSpaCentarAsync(CancellationToken ct)
    {
        var entity = await _context.SpaCentri
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == DefaultSpaCentarId, ct);

        if (entity == null) return null;

        return new SpaCentarDTO
        {
            Id = entity.Id,
            Naziv = entity.Naziv,
            Adresa = entity.Adresa,
            Email = entity.Email,
            Telefon = entity.Telefon,
            Opis = entity.Opis
        };
    }

    public async Task<SpaCentarDTO> UpdateSpaCentarAsync(SpaCentarDTO dto, CancellationToken ct)
    {
        var entity = await _context.SpaCentri
            .FirstOrDefaultAsync(x => x.Id == DefaultSpaCentarId, ct);

        if (entity == null)
        {
            throw new NotFoundException("Spa centar nije pronađen.");
        }

        entity.Naziv = string.IsNullOrWhiteSpace(dto.Naziv) ? entity.Naziv : dto.Naziv.Trim();
        entity.Adresa = dto.Adresa?.Trim();
        entity.Email = dto.Email?.Trim();
        entity.Telefon = dto.Telefon?.Trim();
        entity.Opis = dto.Opis?.Trim();

        await _context.SaveChangesAsync(ct);
        var updated = await GetSpaCentarAsync(ct);
        if (updated == null) throw new NotFoundException("Spa centar nije pronađen.");
        return updated;
    }

    public async Task<List<RadnoVrijemeDTO>> GetRadnoVrijemeAsync(CancellationToken ct)
    {
        return await _context.RadnaVremena
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
            .ToListAsync(ct);
    }

    public async Task<List<RadnoVrijemeDTO>> UpdateRadnoVrijemeAsync(
        List<RadnoVrijemeDTO> items,
        CancellationToken ct)
    {
        var entities = await _context.RadnaVremena
            .Where(x => x.SpaCentarId == DefaultSpaCentarId)
            .ToListAsync(ct);

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

        await _context.SaveChangesAsync(ct);
        return await GetRadnoVrijemeAsync(ct);
    }

    public async Task<List<ProstorijaDTO>> GetProstorijeAsync(CancellationToken ct)
    {
        return await _context.Prostorije
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
            .ToListAsync(ct);
    }

    public async Task<ProstorijaDTO> CreateProstorijaAsync(ProstorijaDTO dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Naziv))
        {
            throw new BusinessRuleException("Naziv je obavezan.");
        }

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
        await _context.SaveChangesAsync(ct);

        return new ProstorijaDTO
        {
            Id = e.Id,
            SpaCentarId = e.SpaCentarId,
            Naziv = e.Naziv,
            Opis = e.Opis,
            Kapacitet = e.Kapacitet,
            IsAktivna = e.IsAktivna
        };
    }

    public async Task UpdateProstorijaAsync(int id, ProstorijaDTO dto, CancellationToken ct)
    {
        var e = await _context.Prostorije
            .FirstOrDefaultAsync(x => x.Id == id && x.SpaCentarId == DefaultSpaCentarId, ct);

        if (e == null)
        {
            throw new NotFoundException("Prostorija nije pronađena.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Naziv)) e.Naziv = dto.Naziv.Trim();
        e.Opis = dto.Opis?.Trim();
        if (dto.Kapacitet > 0) e.Kapacitet = dto.Kapacitet;
        e.IsAktivna = dto.IsAktivna;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteProstorijaAsync(int id, CancellationToken ct)
    {
        var e = await _context.Prostorije
            .FirstOrDefaultAsync(x => x.Id == id && x.SpaCentarId == DefaultSpaCentarId, ct);

        if (e == null)
        {
            throw new NotFoundException("Prostorija nije pronađena.");
        }

        _context.Prostorije.Remove(e);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<OpremaDTO>> GetOpremaAsync(CancellationToken ct)
    {
        return await _context.Oprema
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
            .ToListAsync(ct);
    }

    public async Task<OpremaDTO> CreateOpremaAsync(OpremaDTO dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Naziv))
        {
            throw new BusinessRuleException("Naziv je obavezan.");
        }

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
        await _context.SaveChangesAsync(ct);

        return new OpremaDTO
        {
            Id = e.Id,
            SpaCentarId = e.SpaCentarId,
            Naziv = e.Naziv,
            Napomena = e.Napomena,
            Kolicina = e.Kolicina,
            IsIspravna = e.IsIspravna
        };
    }

    public async Task UpdateOpremaAsync(int id, OpremaDTO dto, CancellationToken ct)
    {
        var e = await _context.Oprema
            .FirstOrDefaultAsync(x => x.Id == id && x.SpaCentarId == DefaultSpaCentarId, ct);

        if (e == null)
        {
            throw new NotFoundException("Oprema nije pronađena.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Naziv)) e.Naziv = dto.Naziv.Trim();
        e.Napomena = dto.Napomena?.Trim();
        if (dto.Kolicina > 0) e.Kolicina = dto.Kolicina;
        e.IsIspravna = dto.IsIspravna;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteOpremaAsync(int id, CancellationToken ct)
    {
        var e = await _context.Oprema
            .FirstOrDefaultAsync(x => x.Id == id && x.SpaCentarId == DefaultSpaCentarId, ct);

        if (e == null)
        {
            throw new NotFoundException("Oprema nije pronađena.");
        }

        _context.Oprema.Remove(e);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<ResourceAvailabilityDTO> GetAvailabilityAsync(
        DateTime slot,
        int? excludeRezervacijaId,
        CancellationToken ct)
    {
        var takenRoomIds = await _context.Rezervacije
            .AsNoTracking()
            .Where(r =>
                !r.IsOtkazana &&
                r.ProstorijaId != null &&
                r.DatumRezervacije == slot &&
                (!excludeRezervacijaId.HasValue || r.Id != excludeRezervacijaId.Value))
            .Select(r => r.ProstorijaId!.Value)
            .Distinct()
            .ToListAsync(ct);

        var freeRooms = await _context.Prostorije
            .AsNoTracking()
            .Where(p =>
                p.SpaCentarId == DefaultSpaCentarId &&
                p.IsAktivna &&
                !takenRoomIds.Contains(p.Id))
            .OrderBy(p => p.Naziv)
            .Select(p => new ProstorijaDTO
            {
                Id = p.Id,
                SpaCentarId = p.SpaCentarId,
                Naziv = p.Naziv,
                Opis = p.Opis,
                Kapacitet = p.Kapacitet,
                IsAktivna = p.IsAktivna
            })
            .ToListAsync(ct);

        var reserved = await _context.RezervacijeOprema
            .AsNoTracking()
            .Where(x =>
                !x.Rezervacija.IsOtkazana &&
                x.Rezervacija.DatumRezervacije == slot &&
                (!excludeRezervacijaId.HasValue || x.RezervacijaId != excludeRezervacijaId.Value))
            .GroupBy(x => x.OpremaId)
            .Select(g => new { OpremaId = g.Key, Qty = g.Sum(x => x.Kolicina) })
            .ToListAsync(ct);

        var reservedMap = reserved.ToDictionary(x => x.OpremaId, x => x.Qty);

        var equipment = await _context.Oprema
            .AsNoTracking()
            .Where(o => o.SpaCentarId == DefaultSpaCentarId && o.IsIspravna)
            .OrderBy(o => o.Naziv)
            .Select(o => new { o.Id, o.Naziv, o.Kolicina })
            .ToListAsync(ct);

        var equipmentDtos = equipment
            .Select(o =>
            {
                var res = reservedMap.TryGetValue(o.Id, out var q) ? q : 0;
                var remaining = Math.Max(0, o.Kolicina - res);
                return new OpremaAvailabilityDTO
                {
                    OpremaId = o.Id,
                    Naziv = o.Naziv,
                    Total = o.Kolicina,
                    Reserved = res,
                    Remaining = remaining
                };
            })
            .ToList();

        return new ResourceAvailabilityDTO
        {
            Slot = slot,
            FreeRooms = freeRooms,
            Equipment = equipmentDtos
        };
    }
}


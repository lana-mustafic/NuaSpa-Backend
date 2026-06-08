using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.Services;

public class AdminKlijentService : IAdminKlijentService
{
    private static readonly string KlijentRoleNormalized = RoleConstants.Klijent.ToUpperInvariant();

    private readonly NuaSpaContext _context;
    private readonly UserManager<Korisnik> _userManager;

    public AdminKlijentService(NuaSpaContext context, UserManager<Korisnik> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<int?> GetKlijentRoleIdAsync(CancellationToken ct)
    {
        return await _context.Roles.AsNoTracking()
            .Where(r => r.NormalizedName == KlijentRoleNormalized)
            .Select(r => (int?)r.Id)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>Single-spa default: lowest Id in Gradovi (seed Sarajevo = 1).</summary>
    private async Task<int> GetDefaultSpaGradIdAsync(CancellationToken ct)
    {
        var id = await _context.Gradovi.AsNoTracking()
            .OrderBy(g => g.Id)
            .Select(g => g.Id)
            .FirstOrDefaultAsync(ct);

        if (id <= 0)
        {
            throw new BusinessRuleException(
                "Nema konfigurisanog grada u bazi. Dodajte grad u referentnu tablicu Gradovi.");
        }

        return id;
    }

    private IQueryable<Korisnik> KlijentiQuery(
        int klijentRoleId,
        KorisnikSearchObject? search,
        string? q)
    {
        var query = _context.Users.AsNoTracking()
            .Where(k => _context.UserRoles.Any(
                ur => ur.UserId == k.Id && ur.RoleId == klijentRoleId));

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
                ZadnjaPosjeta = g.Max(x => x.DatumRezervacije)
            })
            .ToDictionaryAsync(
                x => x.KorisnikId,
                x => new VisitAgg(x.UkupnoPosjeta, x.ZadnjaPosjeta),
                ct);

        var spentMap = await _context.Placanja.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Where(p => p.Status == PlacanjeStatus.Completed)
            .Where(p =>
                p.Rezervacija != null
                && !p.Rezervacija.IsDeleted
                && ids.Contains(p.Rezervacija.KorisnikId))
            .GroupBy(p => p.Rezervacija!.KorisnikId)
            .Select(g => new
            {
                KorisnikId = g.Key,
                UkupnoPotroseno = g.Sum(x => x.NaplaceniIznos ?? x.Iznos),
            })
            .ToDictionaryAsync(x => x.KorisnikId, x => x.UkupnoPotroseno, ct);

        return (visitMap, spentMap);
    }

    public async Task<AdminClientStatsDto> GetStatsAsync(string? q, CancellationToken ct)
    {
        var roleId = await GetKlijentRoleIdAsync(ct);
        if (roleId == null) return new AdminClientStatsDto();

        var baseQuery = KlijentiQuery(roleId.Value, search: null, q);
        var idsQuery = baseQuery.Select(k => k.Id);

        var stats = new AdminClientStatsDto
        {
            UkupnoKlijenata = await baseQuery.CountAsync(ct),
            UkupnoPosjeta = await _context.Rezervacije.AsNoTracking()
                .Where(r => !r.IsOtkazana && idsQuery.Contains(r.KorisnikId))
                .CountAsync(ct),
            UkupnaPotrosnja = await _context.Placanja.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Where(p => p.Status == PlacanjeStatus.Completed)
                .Where(p =>
                    p.Rezervacija != null
                    && !p.Rezervacija.IsDeleted
                    && idsQuery.Contains(p.Rezervacija.KorisnikId))
                .SumAsync(p => p.NaplaceniIznos ?? p.Iznos, ct),
        };

        var perClientVisits = await _context.Rezervacije.AsNoTracking()
            .Where(r => !r.IsOtkazana && idsQuery.Contains(r.KorisnikId))
            .GroupBy(r => r.KorisnikId)
            .Select(g => new { KorisnikId = g.Key, Visits = g.Count() })
            .ToListAsync(ct);

        var perClientSpent = await _context.Placanja.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Where(p => p.Status == PlacanjeStatus.Completed)
            .Where(p =>
                p.Rezervacija != null
                && !p.Rezervacija.IsDeleted
                && idsQuery.Contains(p.Rezervacija.KorisnikId))
            .GroupBy(p => p.Rezervacija!.KorisnikId)
            .Select(g => new
            {
                KorisnikId = g.Key,
                Spent = g.Sum(x => x.NaplaceniIznos ?? x.Iznos),
            })
            .ToListAsync(ct);

        var spentByClient = perClientSpent.ToDictionary(x => x.KorisnikId, x => x.Spent);
        var perClient = perClientVisits
            .Select(v => new
            {
                v.KorisnikId,
                v.Visits,
                Spent = spentByClient.GetValueOrDefault(v.KorisnikId),
            })
            .ToList();

        var vipManualIds = await baseQuery
            .Where(k => k.IsVipKlijent)
            .Select(k => k.Id)
            .ToListAsync(ct);

        var vipComputedIds = perClient
            .Where(p => p.Visits >= 10 || p.Spent >= 600m)
            .Select(p => p.KorisnikId);

        stats.VipKlijenata = vipManualIds
            .Union(vipComputedIds)
            .Distinct()
            .Count();

        return stats;
    }

    public async Task<PagedResult<AdminClientRowDTO>> GetAsync(
        KorisnikSearchObject? search,
        string? q,
        CancellationToken ct)
    {
        var roleId = await GetKlijentRoleIdAsync(ct);
        if (roleId == null)
        {
            return new PagedResult<AdminClientRowDTO>();
        }

        var (page, pageSize) = PaginationHelper.FromSearch(search);
        var query = KlijentiQuery(roleId.Value, search, q)
            .OrderByDescending(k => k.DatumRegistracije);

        var total = await query.CountAsync(ct);
        var rows = await query
            .Select(k => new
            {
                k.Id,
                k.Ime,
                k.Prezime,
                Email = k.Email ?? "",
                UserName = k.UserName ?? "",
                Telefon = k.PhoneNumber ?? "",
                k.DatumRegistracije,
                k.IsVipKlijent,
                k.Status,
                k.NapomenaZaTerapeuta,
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var ids = rows.Select(x => x.Id).ToList();
        if (ids.Count == 0)
        {
            return new PagedResult<AdminClientRowDTO>
            {
                Ukupno = total,
                Stranica = page,
                VelicinaStranice = pageSize,
            };
        }

        var (visitMap, spentMap) = await BuildAggMapsAsync(ids, ct);

        var result = rows.Select(r =>
        {
            visitMap.TryGetValue(r.Id, out var a);
            spentMap.TryGetValue(r.Id, out var ukupno);
            var visits = a?.UkupnoPosjeta ?? 0;
            var total = ukupno;
            var computedVip = r.IsVipKlijent || visits >= 10 || total >= 600m;

            return new AdminClientRowDTO
            {
                Id = r.Id,
                Ime = r.Ime,
                Prezime = r.Prezime,
                Email = r.Email,
                UserName = r.UserName,
                Telefon = r.Telefon,
                DatumRegistracije = r.DatumRegistracije,
                ZadnjaPosjeta = a?.ZadnjaPosjeta,
                UkupnoPosjeta = visits,
                UkupnoPotroseno = total,
                IsVip = computedVip,
                IsVipKlijent = r.IsVipKlijent,
                Status = r.Status,
                NapomenaZaTerapeuta = r.NapomenaZaTerapeuta,
            };
        }).ToList();

        return new PagedResult<AdminClientRowDTO>
        {
            Ukupno = total,
            Stranica = page,
            VelicinaStranice = pageSize,
            Items = result,
        };
    }

    public async Task<AdminClientRowDTO> CreateAsync(AdminKlijentCreateDto dto, CancellationToken ct)
    {
        var roleId = await GetKlijentRoleIdAsync(ct);
        if (roleId == null)
        {
            throw new BusinessRuleException("Uloga Klijent nije pronađena u bazi.");
        }

        var existsMail = await _userManager.FindByEmailAsync(dto.Email.Trim());
        if (existsMail != null)
        {
            throw new ConflictException("Email je već registriran.");
        }

        var existsUser = await _userManager.FindByNameAsync(dto.UserName.Trim());
        if (existsUser != null)
        {
            throw new ConflictException("Korisničko ime je zauzeto.");
        }

        var defaultGradId = await GetDefaultSpaGradIdAsync(ct);

        var user = new Korisnik
        {
            UserName = dto.UserName.Trim(),
            Email = dto.Email.Trim(),
            Ime = dto.Ime.Trim(),
            Prezime = dto.Prezime.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(dto.Telefon) ? null : dto.Telefon.Trim(),
            GradId = defaultGradId,
            DatumRegistracije = DateTime.UtcNow,
            Status = true,
            ZaposlenikId = null,
            IsVipKlijent = dto.IsVipKlijent,
            NapomenaZaTerapeuta = string.IsNullOrWhiteSpace(dto.NapomenaZaTerapeuta)
                ? null
                : dto.NapomenaZaTerapeuta.Trim(),
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            throw new BusinessRuleException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        var addRole = await _userManager.AddToRoleAsync(user, RoleConstants.Klijent);
        if (!addRole.Succeeded)
        {
            throw new BusinessRuleException(string.Join("; ", addRole.Errors.Select(e => e.Description)));
        }

        var list = await GetInternalRowsForIdsAsync(new List<int> { user.Id }, ct);
        return list[0];
    }

    public async Task<AdminClientRowDTO> GetByIdAsync(int id, CancellationToken ct)
    {
        var roleId = await GetKlijentRoleIdAsync(ct);
        if (roleId == null) throw new NotFoundException("Klijent uloga nije pronađena.");

        var isClient = await _context.UserRoles.AsNoTracking()
            .AnyAsync(ur => ur.UserId == id && ur.RoleId == roleId.Value, ct);
        if (!isClient) throw new NotFoundException("Klijent nije pronađen.");

        var rows = await GetInternalRowsForIdsAsync(new List<int> { id }, ct);
        if (rows.Count == 0) throw new NotFoundException("Klijent nije pronađen.");
        return rows[0];
    }

    public async Task<AdminClientRowDTO> PatchAsync(int id, AdminKlijentUpdateDto dto, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) throw new NotFoundException("Korisnik nije pronađen.");

        if (!await _userManager.IsInRoleAsync(user, RoleConstants.Klijent))
        {
            throw new BusinessRuleException("Korisnik nije klijent.");
        }

        if (dto.Status == false)
        {
            var hasUpcoming = await _context.Rezervacije.AsNoTracking()
                .AnyAsync(r =>
                    r.KorisnikId == id &&
                    !r.IsOtkazana &&
                    r.DatumRezervacije > DateTime.UtcNow, ct);

            if (hasUpcoming)
            {
                throw new ConflictException(
                    "Klijent ima buduće termine. Prvo ih otkažite ili premjestite.");
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.Ime)) user.Ime = dto.Ime.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Prezime)) user.Prezime = dto.Prezime.Trim();

        if (dto.Email != null)
        {
            var email = dto.Email.Trim();
            if (string.IsNullOrEmpty(email))
            {
                throw new BusinessRuleException("Email ne može biti prazan.");
            }

            var existingMail = await _userManager.FindByEmailAsync(email);
            if (existingMail != null && existingMail.Id != user.Id)
            {
                throw new ConflictException("Email je već registriran.");
            }

            var setMail = await _userManager.SetEmailAsync(user, email);
            if (!setMail.Succeeded)
            {
                throw new BusinessRuleException(string.Join("; ", setMail.Errors.Select(e => e.Description)));
            }
        }

        if (dto.Telefon != null)
        {
            user.PhoneNumber = string.IsNullOrWhiteSpace(dto.Telefon) ? null : dto.Telefon.Trim();
        }

        if (dto.Status.HasValue) user.Status = dto.Status.Value;
        if (dto.IsVipKlijent.HasValue) user.IsVipKlijent = dto.IsVipKlijent.Value;

        if (dto.NapomenaZaTerapeuta != null) user.NapomenaZaTerapeuta = dto.NapomenaZaTerapeuta;

        if (!string.IsNullOrWhiteSpace(dto.NovaLozinka))
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var pwResult = await _userManager.ResetPasswordAsync(user, resetToken, dto.NovaLozinka);
            if (!pwResult.Succeeded)
            {
                throw new BusinessRuleException(
                    string.Join("; ", pwResult.Errors.Select(e => e.Description)));
            }
        }

        var upd = await _userManager.UpdateAsync(user);
        if (!upd.Succeeded)
        {
            throw new BusinessRuleException(string.Join("; ", upd.Errors.Select(e => e.Description)));
        }

        var list = await GetInternalRowsForIdsAsync(new List<int> { id }, ct);
        if (list.Count == 0) throw new NotFoundException("Korisnik nije pronađen.");
        return list[0];
    }

    private async Task<List<AdminClientRowDTO>> GetInternalRowsForIdsAsync(
        List<int> ids,
        CancellationToken ct)
    {
        var rows = await _context.Users.AsNoTracking()
            .Where(k => ids.Contains(k.Id))
            .Select(k => new
            {
                k.Id,
                k.Ime,
                k.Prezime,
                Email = k.Email ?? "",
                UserName = k.UserName ?? "",
                Telefon = k.PhoneNumber ?? "",
                k.DatumRegistracije,
                k.IsVipKlijent,
                k.Status,
                k.NapomenaZaTerapeuta,
            })
            .ToListAsync(ct);

        var (visitMap, spentMap) = await BuildAggMapsAsync(ids, ct);

        var result = rows.Select(r =>
        {
            visitMap.TryGetValue(r.Id, out var a);
            spentMap.TryGetValue(r.Id, out var ukupno);
            var visits = a?.UkupnoPosjeta ?? 0;
            var total = ukupno;
            var computedVip = r.IsVipKlijent || visits >= 10 || total >= 600m;

            return new AdminClientRowDTO
            {
                Id = r.Id,
                Ime = r.Ime,
                Prezime = r.Prezime,
                Email = r.Email,
                UserName = r.UserName,
                Telefon = r.Telefon,
                DatumRegistracije = r.DatumRegistracije,
                ZadnjaPosjeta = a?.ZadnjaPosjeta,
                UkupnoPosjeta = visits,
                UkupnoPotroseno = total,
                IsVip = computedVip,
                IsVipKlijent = r.IsVipKlijent,
                Status = r.Status,
                NapomenaZaTerapeuta = r.NapomenaZaTerapeuta,
            };
        }).ToList();

        return result;
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.Services
{
    /// <summary>
    /// Content-based filtering (CBF) + popularnost za cold-start.
    /// Signali: rezervacije, favoriti, pretrage, pregledi usluga.
    /// </summary>
    public class PreporukaService : IPreporukaService
    {
        private const int SignalWindowDays = 90;
        private const double WeightBooking = 4.0;
        private const double WeightFavorite = 3.0;
        private const double WeightSearch = 2.0;
        private const double WeightView = 2.0;
        private const double WeightPopular = 1.5;

        private readonly NuaSpaContext _context;
        private readonly IMapper _mapper;

        public PreporukaService(NuaSpaContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task LogAktivnostAsync(int korisnikId, KorisnikAktivnostCreateDto dto)
        {
            if (!Enum.IsDefined(typeof(KorisnikAktivnostTip), dto.Tip))
            {
                return;
            }

            var tip = dto.Tip;
            var since = DateTime.UtcNow.AddMinutes(-5);

            if (tip == KorisnikAktivnostTip.Search)
            {
                var term = dto.SearchTerm?.Trim();
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return;
                }

                var dup = await _context.KorisnikAktivnosti.AsNoTracking()
                    .AnyAsync(a =>
                        a.KorisnikId == korisnikId
                        && a.Tip == KorisnikAktivnostTip.Search
                        && a.SearchTerm == term
                        && a.CreatedAt >= since);
                if (dup)
                {
                    return;
                }

                int? katId = dto.KategorijaUslugaId;
                if (katId is not > 0)
                {
                    var lower = term.ToLower();
                    katId = await _context.Usluge.AsNoTracking()
                        .Where(u => !u.IsDeleted && u.Naziv.ToLower().Contains(lower))
                        .Select(u => (int?)u.KategorijaUslugaId)
                        .FirstOrDefaultAsync();
                }

                _context.KorisnikAktivnosti.Add(new KorisnikAktivnost
                {
                    KorisnikId = korisnikId,
                    Tip = KorisnikAktivnostTip.Search,
                    SearchTerm = term.Length > 200 ? term[..200] : term,
                    KategorijaUslugaId = katId is > 0 ? katId : null,
                    CreatedAt = DateTime.UtcNow,
                });
            }
            else if (tip == KorisnikAktivnostTip.ViewService)
            {
                if (dto.UslugaId is not > 0)
                {
                    return;
                }

                var usluga = await _context.Usluge.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == dto.UslugaId && !u.IsDeleted);
                if (usluga == null)
                {
                    return;
                }

                var viewSince = DateTime.UtcNow.AddHours(-1);
                var dupView = await _context.KorisnikAktivnosti.AsNoTracking()
                    .AnyAsync(a =>
                        a.KorisnikId == korisnikId
                        && a.Tip == KorisnikAktivnostTip.ViewService
                        && a.UslugaId == dto.UslugaId
                        && a.CreatedAt >= viewSince);
                if (dupView)
                {
                    return;
                }

                _context.KorisnikAktivnosti.Add(new KorisnikAktivnost
                {
                    KorisnikId = korisnikId,
                    Tip = KorisnikAktivnostTip.ViewService,
                    UslugaId = usluga.Id,
                    KategorijaUslugaId = usluga.KategorijaUslugaId,
                    CreatedAt = DateTime.UtcNow,
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PreporucenaUslugaDto>> GetPreporukeAsync(
            int korisnikId,
            int take = 10)
        {
            take = Math.Clamp(take, 1, 30);
            var since = DateTime.UtcNow.AddDays(-SignalWindowDays);

            var usluge = await _context.Usluge
                .AsNoTracking()
                .Include(u => u.KategorijaUsluga)
                .Where(u => !u.IsDeleted)
                .ToListAsync();

            if (usluge.Count == 0)
            {
                return Array.Empty<PreporucenaUslugaDto>();
            }

            var favIds = await _context.Favoriti.AsNoTracking()
                .Where(f => f.KorisnikId == korisnikId)
                .Select(f => f.UslugaId)
                .ToListAsync();
            var favSet = favIds.ToHashSet();

            var bookingCats = await _context.Rezervacije.AsNoTracking()
                .Where(r => r.KorisnikId == korisnikId && !r.IsOtkazana)
                .Join(_context.Usluge, r => r.UslugaId, u => u.Id, (r, u) => u.KategorijaUslugaId)
                .ToListAsync();

            var aktivnosti = await _context.KorisnikAktivnosti.AsNoTracking()
                .Where(a => a.KorisnikId == korisnikId && a.CreatedAt >= since)
                .ToListAsync();

            var categoryNames = await _context.KategorijeUsluga.AsNoTracking()
                .Where(k => !k.IsDeleted)
                .ToDictionaryAsync(k => k.Id, k => k.Naziv);

            var categoryScores = new Dictionary<int, double>();
            void AddCatScore(int catId, double w)
            {
                if (catId <= 0) return;
                categoryScores[catId] = categoryScores.GetValueOrDefault(catId) + w;
            }

            foreach (var c in bookingCats)
            {
                AddCatScore(c, WeightBooking);
            }

            foreach (var f in favIds)
            {
                var u = usluge.FirstOrDefault(x => x.Id == f);
                if (u != null)
                {
                    AddCatScore(u.KategorijaUslugaId, WeightFavorite);
                }
            }

            foreach (var a in aktivnosti.Where(x => x.Tip == KorisnikAktivnostTip.Search))
            {
                if (a.KategorijaUslugaId is int searchCat and > 0)
                {
                    AddCatScore(searchCat, WeightSearch);
                }
            }

            foreach (var a in aktivnosti.Where(x => x.Tip == KorisnikAktivnostTip.ViewService))
            {
                if (a.KategorijaUslugaId is int viewCat and > 0)
                {
                    AddCatScore(viewCat, WeightView);
                }
            }

            var viewedUslugaIds = aktivnosti
                .Where(a => a.Tip == KorisnikAktivnostTip.ViewService && a.UslugaId.HasValue)
                .Select(a => a.UslugaId!.Value)
                .ToHashSet();

            var searchTerms = aktivnosti
                .Where(a => a.Tip == KorisnikAktivnostTip.Search && !string.IsNullOrWhiteSpace(a.SearchTerm))
                .Select(a => a.SearchTerm!.Trim().ToLower())
                .Distinct()
                .ToList();

            var popularity = await _context.Rezervacije.AsNoTracking()
                .Where(r => !r.IsOtkazana)
                .GroupBy(r => r.UslugaId)
                .Select(g => new { UslugaId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UslugaId, x => x.Count);

            var maxPop = popularity.Values.DefaultIfEmpty(0).Max();
            var maxCat = categoryScores.Values.DefaultIfEmpty(0).Max();

            var ranked = new List<(Usluga U, double Score, string Kod, string Tekst)>();

            foreach (var u in usluge)
            {
                var catScore = categoryScores.GetValueOrDefault(u.KategorijaUslugaId);
                var normCat = maxCat > 0 ? catScore / maxCat : 0;

                var pop = popularity.GetValueOrDefault(u.Id);
                var normPop = maxPop > 0 ? (double)pop / maxPop : 0;

                var favBoost = favSet.Contains(u.Id) ? 0.15 : 0;
                var score = normCat * 0.55 + normPop * WeightPopular * 0.1 + favBoost;

                var (kod, tekst) = Explain(
                    u,
                    categoryScores,
                    categoryNames,
                    favSet,
                    viewedUslugaIds,
                    searchTerms,
                    bookingCats,
                    pop,
                    maxPop);

                ranked.Add((u, score, kod, tekst));
            }

            var hasPersonal = categoryScores.Count > 0
                || searchTerms.Count > 0
                || viewedUslugaIds.Count > 0;

            var ordered = ranked
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.U.Naziv)
                .Take(take)
                .ToList();

            if (!hasPersonal)
            {
                ordered = usluge
                    .Select(u =>
                    {
                        var pop = popularity.GetValueOrDefault(u.Id);
                        var normPop = maxPop > 0 ? (double)pop / maxPop : 0;
                        return (
                            u,
                            normPop,
                            PreporukaRazlogKod.Popular,
                            pop > 0
                                ? "Popularno među našim gostima."
                                : "Preporučena usluga iz našeg kataloga.");
                    })
                    .OrderByDescending(x => x.Item2)
                    .ThenBy(x => x.u.Naziv)
                    .Take(take)
                    .Select(x => (x.u, x.Item2, x.Item3, x.Item4))
                    .ToList();
            }

            return ordered.Select(x => new PreporucenaUslugaDto
            {
                Usluga = _mapper.Map<UslugaDTO>(x.U),
                RazlogKod = x.Kod,
                RazlogTekst = x.Tekst,
                Skor = Math.Round(x.Score, 3),
            }).ToList();
        }

        private static (string Kod, string Tekst) Explain(
            Usluga u,
            Dictionary<int, double> categoryScores,
            Dictionary<int, string> categoryNames,
            HashSet<int> favSet,
            HashSet<int> viewedUslugaIds,
            List<string> searchTerms,
            List<int> bookingCats,
            int pop,
            int maxPop)
        {
            var catName = categoryNames.GetValueOrDefault(u.KategorijaUslugaId) ?? "ova kategorija";

            if (favSet.Contains(u.Id))
            {
                return (
                    PreporukaRazlogKod.FavoriteCategory,
                    $"Dodali ste sličnu uslugu u favorite — kategorija {catName}.");
            }

            if (bookingCats.Contains(u.KategorijaUslugaId))
            {
                return (
                    PreporukaRazlogKod.PastBookingCategory,
                    $"Već ste rezervisali tretmane iz kategorije {catName}.");
            }

            if (viewedUslugaIds.Contains(u.Id))
            {
                return (
                    PreporukaRazlogKod.ViewedSimilar,
                    "Nedavno ste pregledali ovu uslugu.");
            }

            var viewedInCat = viewedUslugaIds.Count > 0
                && categoryScores.GetValueOrDefault(u.KategorijaUslugaId) > 0;
            if (viewedInCat)
            {
                return (
                    PreporukaRazlogKod.ViewedSimilar,
                    $"Pregledali ste slične usluge u kategoriji {catName}.");
            }

            if (searchTerms.Any(t =>
                    u.Naziv.ToLower().Contains(t)
                    || catName.ToLower().Contains(t)))
            {
                return (
                    PreporukaRazlogKod.SearchInterest,
                    $"Nedavno ste pretraživali sadržaj vezan za {catName}.");
            }

            if (categoryScores.GetValueOrDefault(u.KategorijaUslugaId) > 0)
            {
                return (
                    PreporukaRazlogKod.NewInCategory,
                    $"Na osnovu vašeg interesa za kategoriju {catName}.");
            }

            if (pop > 0 && pop >= maxPop * 0.5)
            {
                return (
                    PreporukaRazlogKod.Popular,
                    "Popularno među našim gostima.");
            }

            return (
                PreporukaRazlogKod.Popular,
                "Preporučena usluga iz našeg kataloga.");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Application.Services
{
    public class AdminFinanceService : IAdminFinanceService
    {
        private readonly NuaSpaContext _db;

        public AdminFinanceService(NuaSpaContext db)
        {
            _db = db;
        }

        public async Task<AdminFinanceDashboardDto> GetDashboardAsync(
            DateTime from,
            DateTime toExclusive,
            int page,
            int pageSize,
            string? search,
            string? status,
            string? methodCategory,
            int? uslugaId,
            CancellationToken cancellationToken = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var span = toExclusive - from;
            if (span <= TimeSpan.Zero)
            {
                toExclusive = from.AddDays(1);
                span = toExclusive - from;
            }

            var prevFrom = from - span;
            var prevToExclusive = from;

            var statusNorm = (status ?? "all").Trim().ToLowerInvariant();
            var methodNorm = (methodCategory ?? "all").Trim().ToLowerInvariant();
            var searchNorm = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

            var baseQ = FilteredPlacanjaQuery(from, toExclusive, searchNorm, statusNorm, methodNorm, uslugaId);

            var ukupno = await baseQ.CountAsync(cancellationToken);

            var rowsEntities = await baseQ
                .OrderByDescending(p => p.DatumPlacanja)
                .ThenByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var dtoRows = rowsEntities.Select(p => new AdminFinanceTransactionRowDto
            {
                PlacanjeId = p.Id,
                TransakcijskiId = FormatPayId(p.Id, p.DatumPlacanja),
                KlijentPunoIme = FormatKlijent(p.Rezervacija),
                UslugaTekst = FormatUslugaTekst(p.Rezervacija),
                DatumVrijeme = p.DatumPlacanja,
                Iznos = p.Iznos,
                MetodaLabel = FormatMethodLabel(p.MetodaPlacanja, p.TransakcijskiBroj),
                Status = MapStatus(p.Rezervacija?.IsPlacena, p.Rezervacija?.IsOtkazana),
            }).ToList();

            // Jedan DbContext ne smije izvršavati više upita paralelno (Task.WhenAll bi bacalo 500).
            var kpiCur = await ComputeKpiAsync(from, toExclusive, cancellationToken);
            var kpiPrev = await ComputeKpiAsync(prevFrom, prevToExclusive, cancellationToken);
            var metode = await MethodSharesAsync(from, toExclusive, cancellationToken);
            var trend = await RevenueTrendAsync(from, toExclusive, cancellationToken);
            var aktivnost = await RecentActivityAsync(from, toExclusive, cancellationToken);

            return new AdminFinanceDashboardDto
            {
                Kpi = BuildKpiDto(kpiCur, kpiPrev),
                Redovi = dtoRows,
                Ukupno = ukupno,
                Stranica = page,
                VelicinaStranice = pageSize,
                MetodePostotak = metode,
                PrihodDnevno = trend,
                NedavnaAktivnost = aktivnost,
            };
        }

        public async Task<byte[]> GetDashboardCsvAsync(
            DateTime from,
            DateTime toExclusive,
            string? search,
            string? status,
            string? methodCategory,
            int? uslugaId,
            CancellationToken cancellationToken = default)
        {
            var statusNorm = (status ?? "all").Trim().ToLowerInvariant();
            var methodNorm = (methodCategory ?? "all").Trim().ToLowerInvariant();
            var searchNorm = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

            var q = FilteredPlacanjaQuery(from, toExclusive, searchNorm, statusNorm, methodNorm, uslugaId)
                .OrderByDescending(p => p.DatumPlacanja)
                .ThenByDescending(p => p.Id);

            var all = await q.ToListAsync(cancellationToken);

            var sb = new StringBuilder();
            sb.Append('\uFEFF');
            sb.AppendLine("Transaction ID;Client;Service;DateTime;Amount;Method;Status");

            foreach (var r in all)
            {
                var id = FormatPayId(r.Id, r.DatumPlacanja);
                var client = FormatKlijent(r.Rezervacija);
                var svc = FormatUslugaTekst(r.Rezervacija);
                var dt = r.DatumPlacanja.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                var amt = r.Iznos.ToString("0.##", CultureInfo.InvariantCulture);
                var meth = FormatMethodLabel(r.MetodaPlacanja, r.TransakcijskiBroj);
                var st = MapStatus(r.Rezervacija?.IsPlacena, r.Rezervacija?.IsOtkazana);
                sb.AppendLine($"{id};{Csv(client)};{Csv(svc)};{dt};{amt};{Csv(meth)};{st}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string Csv(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Contains(';') || s.Contains('"') || s.Contains('\n'))
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }

        private IQueryable<Placanje> FilteredPlacanjaQuery(
            DateTime from,
            DateTime toExclusive,
            string? searchNorm,
            string statusNorm,
            string methodNorm,
            int? uslugaId)
        {
            var q = _db.Placanja.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Where(p => p.DatumPlacanja >= from && p.DatumPlacanja < toExclusive);

            if (uslugaId.HasValue)
            {
                var uid = uslugaId.Value;
                q = q.Where(p => p.Rezervacija != null && p.Rezervacija.UslugaId == uid);
            }

            if (!string.IsNullOrEmpty(searchNorm))
            {
                var t = searchNorm.ToLowerInvariant();
                q = q.Where(p =>
                    (p.TransakcijskiBroj ?? "").ToLower().Contains(t) ||
                    (p.Rezervacija != null &&
                     p.Rezervacija.Korisnik != null &&
                     p.Rezervacija.Usluga != null &&
                     (p.Rezervacija.Korisnik.Ime.ToLower().Contains(t) ||
                      p.Rezervacija.Korisnik.Prezime.ToLower().Contains(t) ||
                      p.Rezervacija.Usluga.Naziv.ToLower().Contains(t))));
            }

            q = statusNorm switch
            {
                "paid" => q.Where(p =>
                    p.Rezervacija == null || (p.Rezervacija.IsPlacena && !p.Rezervacija.IsOtkazana)),
                "unpaid" => q.Where(p =>
                    p.Rezervacija != null && !p.Rezervacija.IsPlacena && !p.Rezervacija.IsOtkazana),
                "refunded" => q.Where(p =>
                    p.Rezervacija != null && p.Rezervacija.IsOtkazana && p.Rezervacija.IsPlacena),
                _ => q,
            };

            if (methodNorm == "card")
            {
                q = q.Where(p =>
                    (p.MetodaPlacanja ?? "").ToLower().Contains("stripe") ||
                    (p.MetodaPlacanja ?? "").ToLower().Contains("visa") ||
                    (p.MetodaPlacanja ?? "").ToLower().Contains("master") ||
                    (p.MetodaPlacanja ?? "").ToLower().Contains("card"));
            }
            else if (methodNorm == "cash")
            {
                q = q.Where(p => (p.MetodaPlacanja ?? "").ToLower().Contains("gotovin"));
            }
            else if (methodNorm == "digital")
            {
                q = q.Where(p =>
                    (p.MetodaPlacanja ?? "").ToLower().Contains("apple") ||
                    (p.MetodaPlacanja ?? "").ToLower().Contains("google") ||
                    (p.MetodaPlacanja ?? "").ToLower().Contains("paypal"));
            }

            return q
                .Include(p => p.Rezervacija!)
                    .ThenInclude(r => r.Korisnik)
                .Include(p => p.Rezervacija!)
                    .ThenInclude(r => r.Usluga);
        }

        private sealed class KpiSnapshot
        {
            public decimal UkupniPrihod { get; set; }
            public int PlaceneRezervacije { get; set; }
            public int NeplaceneRezervacije { get; set; }
            public decimal IznosRefundacija { get; set; }
            public int BrojPlacanjaZaProsjek { get; set; }
        }

        private async Task<KpiSnapshot> ComputeKpiAsync(
            DateTime from,
            DateTime toExclusive,
            CancellationToken ct)
        {
            var placanja = await _db.Placanja.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Where(p => p.DatumPlacanja >= from && p.DatumPlacanja < toExclusive)
                .Include(p => p.Rezervacija)
                .ToListAsync(ct);

            decimal revenue = 0;
            decimal refunds = 0;
            var paidTx = 0;

            foreach (var p in placanja)
            {
                var st = MapStatus(
                    p.Rezervacija?.IsPlacena,
                    p.Rezervacija?.IsOtkazana);
                if (st == "paid" && (p.Rezervacija == null || !p.Rezervacija.IsOtkazana))
                {
                    revenue += p.Iznos;
                    paidTx++;
                }

                if (st == "refunded")
                    refunds += p.Iznos;
            }

            var placeneRez = await _db.Rezervacije.AsNoTracking()
                .Where(r => !r.IsDeleted)
                .Where(r => r.IsPlacena && !r.IsOtkazana)
                .Where(r => r.DatumRezervacije >= from && r.DatumRezervacije < toExclusive)
                .CountAsync(ct);

            var neplaceneRez = await _db.Rezervacije.AsNoTracking()
                .Where(r => !r.IsDeleted)
                .Where(r => !r.IsPlacena && !r.IsOtkazana)
                .Where(r => r.DatumRezervacije >= from && r.DatumRezervacije < toExclusive)
                .CountAsync(ct);

            return new KpiSnapshot
            {
                UkupniPrihod = revenue,
                PlaceneRezervacije = placeneRez,
                NeplaceneRezervacije = neplaceneRez,
                IznosRefundacija = refunds,
                BrojPlacanjaZaProsjek = paidTx,
            };
        }

        private static AdminFinanceKpiDto BuildKpiDto(KpiSnapshot cur, KpiSnapshot prev)
        {
            decimal avgCur = cur.BrojPlacanjaZaProsjek > 0
                ? cur.UkupniPrihod / cur.BrojPlacanjaZaProsjek
                : 0;
            decimal avgPrev = prev.BrojPlacanjaZaProsjek > 0
                ? prev.UkupniPrihod / prev.BrojPlacanjaZaProsjek
                : 0;

            return new AdminFinanceKpiDto
            {
                UkupniPrihod = cur.UkupniPrihod,
                PostotakPromjeneUkupniPrihod = PctChange(cur.UkupniPrihod, prev.UkupniPrihod),
                PlaceneRezervacije = cur.PlaceneRezervacije,
                PostotakPromjenePlaceneRezervacije = PctChangeI(cur.PlaceneRezervacije, prev.PlaceneRezervacije),
                ProsjecnaVrijednost = avgCur,
                PostotakPromjeneProsjecnaVrijednost = PctChange(avgCur, avgPrev),
                NeplaceneRezervacije = cur.NeplaceneRezervacije,
                PostotakPromjeneNeplaceneRezervacije = PctChangeI(cur.NeplaceneRezervacije, prev.NeplaceneRezervacije),
                IznosRefundacija = cur.IznosRefundacija,
                PostotakPromjeneRefundacija = PctChange(cur.IznosRefundacija, prev.IznosRefundacija),
            };
        }

        private static double? PctChange(decimal cur, decimal prev)
        {
            if (prev == 0) return cur == 0 ? null : 100;
            return (double)((cur - prev) / prev * 100m);
        }

        private static double? PctChangeI(int cur, int prev)
        {
            if (prev == 0) return cur == 0 ? null : 100;
            return (double)(100m * (cur - prev) / prev);
        }

        private async Task<IList<AdminFinanceMethodShareDto>> MethodSharesAsync(
            DateTime from,
            DateTime toExclusive,
            CancellationToken ct)
        {
            var methods = await _db.Placanja.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Where(p => p.DatumPlacanja >= from && p.DatumPlacanja < toExclusive)
                .Select(p => p.MetodaPlacanja)
                .ToListAsync(ct);

            if (methods.Count == 0)
            {
                return new List<AdminFinanceMethodShareDto>
                {
                    new() { Kljuc = "card", Label = "Kartica", Postotak = 0 },
                    new() { Kljuc = "cash", Label = "Gotovina", Postotak = 0 },
                    new() { Kljuc = "digital", Label = "Digital Wallets", Postotak = 0 },
                    new() { Kljuc = "other", Label = "Ostalo", Postotak = 0 },
                };
            }

            var buckets = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["card"] = 0,
                ["cash"] = 0,
                ["digital"] = 0,
                ["other"] = 0,
            };

            foreach (var m in methods)
            {
                var key = BucketMethod(m);
                buckets[key]++;
            }

            var total = (double)methods.Count;
            return new List<AdminFinanceMethodShareDto>
            {
                new()
                {
                    Kljuc = "card",
                    Label = "Kartica",
                    Postotak = Math.Round(100.0 * buckets["card"] / total, 1),
                },
                new()
                {
                    Kljuc = "cash",
                    Label = "Gotovina",
                    Postotak = Math.Round(100.0 * buckets["cash"] / total, 1),
                },
                new()
                {
                    Kljuc = "digital",
                    Label = "Digital Wallets",
                    Postotak = Math.Round(100.0 * buckets["digital"] / total, 1),
                },
                new()
                {
                    Kljuc = "other",
                    Label = "Ostalo",
                    Postotak = Math.Round(100.0 * buckets["other"] / total, 1),
                },
            };
        }

        private static string BucketMethod(string? metoda)
        {
            var m = (metoda ?? "").ToLowerInvariant();
            if (m.Contains("stripe") || m.Contains("visa") || m.Contains("master") || m.Contains("card"))
                return "card";
            if (m.Contains("gotovin"))
                return "cash";
            if (m.Contains("apple") || m.Contains("google") || m.Contains("paypal"))
                return "digital";
            return "other";
        }

        private async Task<IList<AdminFinanceTrendPointDto>> RevenueTrendAsync(
            DateTime from,
            DateTime toExclusive,
            CancellationToken ct)
        {
            var placanja = await _db.Placanja.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Where(p => p.DatumPlacanja >= from && p.DatumPlacanja < toExclusive)
                .Include(p => p.Rezervacija)
                .ToListAsync(ct);

            var byDay = new Dictionary<DateTime, decimal>();
            for (var d = from.Date; d < toExclusive.Date; d = d.AddDays(1))
                byDay[d] = 0m;

            foreach (var p in placanja)
            {
                var st = MapStatus(p.Rezervacija?.IsPlacena, p.Rezervacija?.IsOtkazana);
                if (st != "paid" || (p.Rezervacija != null && p.Rezervacija.IsOtkazana))
                    continue;

                var day = p.DatumPlacanja.Date;
                if (!byDay.ContainsKey(day))
                    byDay[day] = 0m;
                byDay[day] += p.Iznos;
            }

            return byDay
                .OrderBy(kv => kv.Key)
                .Select(kv => new AdminFinanceTrendPointDto { Datum = kv.Key, Iznos = kv.Value })
                .ToList();
        }

        private async Task<IList<AdminFinanceActivityDto>> RecentActivityAsync(
            DateTime from,
            DateTime toExclusive,
            CancellationToken ct)
        {
            var items = await _db.Placanja.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Where(p => p.DatumPlacanja >= from && p.DatumPlacanja < toExclusive)
                .Include(p => p.Rezervacija!)
                    .ThenInclude(r => r.Korisnik)
                .OrderByDescending(p => p.DatumPlacanja)
                .Take(12)
                .ToListAsync(ct);

            var list = new List<AdminFinanceActivityDto>();
            foreach (var p in items)
            {
                var st = MapStatus(p.Rezervacija?.IsPlacena, p.Rezervacija?.IsOtkazana);
                var klijent = FormatKlijent(p.Rezervacija);

                string tip;
                string opis;
                if (st == "refunded")
                {
                    tip = "refund";
                    opis = "Refund izvršen";
                }
                else if (st == "paid")
                {
                    tip = "payment";
                    opis = "Plaćanje uspješno";
                }
                else
                {
                    tip = "pending";
                    opis = "Plaćanje na čekanju";
                }

                list.Add(new AdminFinanceActivityDto
                {
                    Tip = tip,
                    Opis = opis,
                    Klijent = klijent,
                    Iznos = p.Iznos,
                    DatumVrijeme = p.DatumPlacanja,
                });
            }

            return list;
        }

        private static string FormatKlijent(Rezervacija? r)
        {
            if (r?.Korisnik == null) return "—";
            return $"{r.Korisnik.Ime} {r.Korisnik.Prezime}".Trim();
        }

        private static string FormatUslugaTekst(Rezervacija? r)
        {
            if (r?.Usluga == null) return "—";
            return FormatUsluga(r.Usluga.Naziv, r.Usluga.TrajanjeMinuta);
        }

        private static string MapStatus(bool? placena, bool? otkazana)
        {
            if (placena == null && otkazana == null)
                return "paid";
            var pl = placena ?? false;
            var ot = otkazana ?? false;
            if (ot && pl)
                return "refunded";
            if (pl && !ot)
                return "paid";
            return "unpaid";
        }

        private static string FormatPayId(int id, DateTime datum)
        {
            return $"#PAY-{datum.Year}-{id:D5}";
        }

        private static string FormatUsluga(string? naziv, int? trajanjeMin)
        {
            if (string.IsNullOrWhiteSpace(naziv))
                return "—";
            if (trajanjeMin.HasValue && trajanjeMin.Value > 0)
                return $"{naziv} ({trajanjeMin} min)";
            return naziv;
        }

        private static string FormatMethodLabel(string metoda, string transBroj)
        {
            var m = (metoda ?? "").ToLowerInvariant();
            if (m.Contains("stripe") || m.Contains("visa") || m.Contains("master") || m.Contains("card"))
            {
                if (!string.IsNullOrEmpty(transBroj) && transBroj.StartsWith("pi_", StringComparison.OrdinalIgnoreCase))
                    return "Stripe / Card";
                return "Stripe / Card";
            }

            if (m.Contains("gotovin"))
                return "Gotovina";
            if (m.Contains("apple"))
                return "Apple Pay";
            return string.IsNullOrWhiteSpace(metoda) ? "—" : metoda;
        }
    }
}

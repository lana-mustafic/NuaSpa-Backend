using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

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
                RezervacijaId = p.RezervacijaId,
                TransakcijskiId = FormatPayId(p.Id, p.DatumPlacanja),
                StripePaymentIntentId = p.TransakcijskiBroj.StartsWith("pi_", StringComparison.OrdinalIgnoreCase)
                    ? p.TransakcijskiBroj
                    : null,
                KlijentPunoIme = FormatKlijent(p.Rezervacija),
                UslugaTekst = FormatUslugaTekst(p.Rezervacija),
                DatumVrijeme = p.DatumPlacanja,
                DatumZavrsetka = p.DatumZavrsetka,
                Iznos = EffectiveAmount(p),
                NaplaceniIznos = p.NaplaceniIznos,
                MetodaLabel = FormatMethodLabel(p.MetodaPlacanja, p.TransakcijskiBroj),
                StripeRefundId = p.StripeRefundId,
                Status = MapStatus(p),
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

        public async Task<AdminFinanceCsvResultDto> GetDashboardCsvAsync(
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

            const int csvCap = PaginationConstants.MaxPageSize * 10;
            var totalMatching = await q.CountAsync(cancellationToken);
            var all = await q
                .Take(csvCap)
                .ToListAsync(cancellationToken);
            var truncated = totalMatching > csvCap;

            var sb = new StringBuilder();
            sb.Append('\uFEFF');
            if (truncated)
            {
                sb.AppendLine(
                    $"# WARNING: Export limited to {csvCap} of {totalMatching} matching transactions. Narrow filters or date range.");
            }

            sb.AppendLine("Transaction ID;Client;Service;DateTime;Amount;Method;Status");

            foreach (var r in all)
            {
                var id = FormatPayId(r.Id, r.DatumPlacanja);
                var client = FormatKlijent(r.Rezervacija);
                var svc = FormatUslugaTekst(r.Rezervacija);
                var dt = r.DatumPlacanja.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                var amt = EffectiveAmount(r).ToString("0.##", CultureInfo.InvariantCulture);
                var meth = FormatMethodLabel(r.MetodaPlacanja, r.TransakcijskiBroj);
                var st = MapStatus(r);
                sb.AppendLine($"{id};{Csv(client)};{Csv(svc)};{dt};{amt};{Csv(meth)};{st}");
            }

            return new AdminFinanceCsvResultDto
            {
                Bytes = Encoding.UTF8.GetBytes(sb.ToString()),
                Truncated = truncated,
                ExportedRows = all.Count,
                TotalMatchingRows = totalMatching,
            };
        }

        private static decimal EffectiveAmount(Placanje p) => p.NaplaceniIznos ?? p.Iznos;

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
                .Where(p => p.DatumPlacanja >= from && p.DatumPlacanja < toExclusive)
                .Where(p => p.Rezervacija == null || !p.Rezervacija.IsDeleted);

            if (uslugaId.HasValue)
            {
                var uid = uslugaId.Value;
                q = q.Where(p => p.Rezervacija != null && p.Rezervacija.UslugaId == uid);
            }

            if (!string.IsNullOrEmpty(searchNorm))
            {
                var t = searchNorm.ToLowerInvariant();
                var payId = TryParsePayIdFromSearch(searchNorm);
                q = q.Where(p =>
                    (payId != null && p.Id == payId.Value) ||
                    (p.TransakcijskiBroj ?? "").ToLower().Contains(t) ||
                    (p.Rezervacija != null &&
                     p.Rezervacija.Korisnik != null &&
                     p.Rezervacija.Usluga != null &&
                     ((p.Rezervacija.Korisnik.Ime + " " + p.Rezervacija.Korisnik.Prezime).ToLower().Contains(t) ||
                      p.Rezervacija.Korisnik.Ime.ToLower().Contains(t) ||
                      p.Rezervacija.Korisnik.Prezime.ToLower().Contains(t) ||
                      p.Rezervacija.Usluga.Naziv.ToLower().Contains(t))));
            }

            q = statusNorm switch
            {
                "paid" => q.Where(p => p.Status == PlacanjeStatus.Completed),
                "unpaid" => q.Where(p => p.Status == PlacanjeStatus.Pending),
                "failed" => q.Where(p => p.Status == PlacanjeStatus.Failed),
                "refunded" => q.Where(p => p.Status == PlacanjeStatus.Refunded),
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
            var placanjaQ = _db.Placanja.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Where(p => p.DatumPlacanja >= from && p.DatumPlacanja < toExclusive);

            var revenue = await placanjaQ
                .Where(p => p.Status == PlacanjeStatus.Completed)
                .SumAsync(p => p.NaplaceniIznos ?? p.Iznos, ct);

            var paidTx = await placanjaQ
                .CountAsync(p => p.Status == PlacanjeStatus.Completed, ct);

            var refunds = await placanjaQ
                .Where(p => p.Status == PlacanjeStatus.Refunded)
                .SumAsync(p => p.NaplaceniIznos ?? p.Iznos, ct);

            // Payment-date aligned counts (consistent with transaction table filters).
            var placeneRez = await placanjaQ
                .CountAsync(p => p.Status == PlacanjeStatus.Completed, ct);

            var neplaceneRez = await placanjaQ
                .CountAsync(p => p.Status == PlacanjeStatus.Pending, ct);

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
            if (prev == 0) return cur == 0 ? null : null;
            return (double)((cur - prev) / prev * 100m);
        }

        private static double? PctChangeI(int cur, int prev)
        {
            if (prev == 0) return cur == 0 ? null : null;
            return (double)(100m * (cur - prev) / prev);
        }

        private static int? TryParsePayIdFromSearch(string search)
        {
            var payMatch = Regex.Match(search, @"#?pay-?\d{4}-?(\d+)", RegexOptions.IgnoreCase);
            if (payMatch.Success && int.TryParse(payMatch.Groups[1].Value, out var fromPay))
            {
                return fromPay;
            }

            if (int.TryParse(search.Trim().TrimStart('#'), out var numeric))
            {
                return numeric;
            }

            return null;
        }

        private async Task<IList<AdminFinanceMethodShareDto>> MethodSharesAsync(
            DateTime from,
            DateTime toExclusive,
            CancellationToken ct)
        {
            var baseQ = _db.Placanja.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Where(p => p.DatumPlacanja >= from && p.DatumPlacanja < toExclusive)
                .Where(p => p.Status == PlacanjeStatus.Completed)
                .Where(p => p.Rezervacija == null || !p.Rezervacija.IsDeleted);

            var completed = await baseQ.ToListAsync(ct);
            if (completed.Count == 0)
            {
                return new List<AdminFinanceMethodShareDto>
                {
                    new() { Kljuc = "card", Label = "Card", Postotak = 0 },
                    new() { Kljuc = "cash", Label = "Cash", Postotak = 0 },
                    new() { Kljuc = "digital", Label = "Digital Wallets", Postotak = 0 },
                    new() { Kljuc = "other", Label = "Other", Postotak = 0 },
                };
            }

            var card = completed.Count(p => BucketMethod(p.MetodaPlacanja) == "card");
            var cash = completed.Count(p => BucketMethod(p.MetodaPlacanja) == "cash");
            var digital = completed.Count(p => BucketMethod(p.MetodaPlacanja) == "digital");
            var other = completed.Count - card - cash - digital;
            if (other < 0) other = 0;

            var totalD = (double)completed.Count;
            var shares = new[]
            {
                ("card", "Card", card),
                ("cash", "Cash", cash),
                ("digital", "Digital Wallets", digital),
                ("other", "Other", other),
            };
            var rounded = shares
                .Select(s => (s.Item1, s.Item2, Math.Round(100.0 * s.Item3 / totalD, 1)))
                .ToList();
            var drift = 100.0 - rounded.Sum(x => x.Item3);
            if (Math.Abs(drift) >= 0.1 && rounded.Count > 0)
            {
                var idx = rounded.FindIndex(x => x.Item3 > 0);
                if (idx >= 0)
                {
                    var fix = rounded[idx];
                    rounded[idx] = (fix.Item1, fix.Item2, Math.Round(fix.Item3 + drift, 1));
                }
            }

            return rounded.Select(s => new AdminFinanceMethodShareDto
            {
                Kljuc = s.Item1,
                Label = s.Item2,
                Postotak = s.Item3,
            }).ToList();
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
            var daily = await _db.Placanja.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Where(p => p.DatumPlacanja >= from && p.DatumPlacanja < toExclusive)
                .Where(p => p.Status == PlacanjeStatus.Completed)
                .GroupBy(p => p.DatumPlacanja.Date)
                .Select(g => new
                {
                    Datum = g.Key,
                    Iznos = g.Sum(p => p.NaplaceniIznos ?? p.Iznos),
                })
                .ToDictionaryAsync(x => x.Datum, x => x.Iznos, ct);

            var points = new List<AdminFinanceTrendPointDto>();
            for (var d = from.Date; d < toExclusive.Date; d = d.AddDays(1))
            {
                points.Add(new AdminFinanceTrendPointDto
                {
                    Datum = d,
                    Iznos = daily.GetValueOrDefault(d),
                });
            }

            return points;
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
                var st = MapStatus(p);
                var klijent = FormatKlijent(p.Rezervacija);

                string tip;
                string opis;
                if (st == "refunded")
                {
                    tip = "refund";
                    opis = "Refund processed";
                }
                else if (st == "paid")
                {
                    tip = "payment";
                    opis = "Payment received";
                }
                else if (st == "failed")
                {
                    tip = "failed";
                    opis = "Payment failed";
                }
                else
                {
                    tip = "pending";
                    opis = "Payment pending";
                }

                list.Add(new AdminFinanceActivityDto
                {
                    Tip = tip,
                    Opis = opis,
                    Klijent = klijent,
                    Iznos = EffectiveAmount(p),
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

        private static string MapStatus(Placanje placanje)
        {
            return placanje.Status switch
            {
                PlacanjeStatus.Refunded => "refunded",
                PlacanjeStatus.Completed => "paid",
                PlacanjeStatus.Pending => "unpaid",
                PlacanjeStatus.Failed => "failed",
                _ => "unpaid",
            };
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
                return "Card";
            }

            if (m.Contains("gotovin") || m.Contains("cash"))
                return "Cash";
            if (m.Contains("apple"))
                return "Apple Pay";
            if (m.Contains("google"))
                return "Google Pay";
            if (m.Contains("paypal"))
                return "PayPal";
            if (m.Contains("spa") || m.Contains("at spa"))
                return "Pay at spa";
            return string.IsNullOrWhiteSpace(metoda) ? "—" : metoda;
        }
    }
}

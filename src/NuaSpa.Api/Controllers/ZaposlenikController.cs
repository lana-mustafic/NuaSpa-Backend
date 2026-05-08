using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;

namespace NuaSpa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ZaposlenikController : BaseController<ZaposlenikDTO, object>
    {
        private readonly NuaSpaContext _context;

        public ZaposlenikController(IZaposlenikService service, NuaSpaContext context) : base(service)
        {
            _context = context;
        }

        [HttpGet("{id}/kpi")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TherapistKpiDTO>> GetKpis(
            int id,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            if (to < from) return BadRequest("Neispravan period (to < from).");
            if ((to.Date - from.Date).TotalDays > 366)
            {
                return BadRequest("Period je prevelik (max 366 dana).");
            }

            var start = from.Date;
            var endExclusive = to.Date.AddDays(1);

            var rezQuery = _context.Rezervacije
                .AsNoTracking()
                .Where(r => r.ZaposlenikId == id && r.DatumRezervacije >= start && r.DatumRezervacije < endExclusive);

            var ukupno = await rezQuery.CountAsync();
            var potvrdjene = await rezQuery.Where(r => r.IsPotvrdjena && !r.IsOtkazana).CountAsync();
            var otkazane = await rezQuery.Where(r => r.IsOtkazana).CountAsync();
            var placene = await rezQuery.Where(r => r.IsPlacena && !r.IsOtkazana).CountAsync();

            var prihod = await rezQuery
                .Where(r => r.IsPlacena && !r.IsOtkazana)
                .Join(
                    _context.Usluge.AsNoTracking(),
                    r => r.UslugaId,
                    u => u.Id,
                    (r, u) => u.Cijena
                )
                .SumAsync(x => (decimal?)x) ?? 0m;

            // Heuristika ocjene: recenzije za usluge koje je ovaj terapeut obavio kod tog korisnika.
            // (Recenzija nije direktno vezana na terapeuta u modelu.)
            var ratings = await (
                from r in _context.Recenzije.AsNoTracking()
                join rez in _context.Rezervacije.AsNoTracking()
                    on new { r.UslugaId, r.KorisnikId } equals new { rez.UslugaId, rez.KorisnikId }
                where rez.ZaposlenikId == id
                      && rez.DatumRezervacije >= start
                      && rez.DatumRezervacije < endExclusive
                select (double?)r.Ocjena
            ).ToListAsync();

            var avg = ratings.Count == 0 ? 0.0 : Math.Round(ratings.Average() ?? 0.0, 2);

            return Ok(new TherapistKpiDTO
            {
                ZaposlenikId = id,
                From = start,
                To = to.Date,
                UkupnoRezervacija = ukupno,
                PotvrdjeneRezervacije = potvrdjene,
                OtkazaneRezervacije = otkazane,
                PlaceneRezervacije = placene,
                Prihod = prihod,
                ProsjecnaOcjena = avg,
            });
        }
    }
}


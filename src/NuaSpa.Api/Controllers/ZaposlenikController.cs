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
        private readonly IZaposlenikService _zaposlenikService;

        public ZaposlenikController(IZaposlenikService service, NuaSpaContext context) : base(service)
        {
            _context = context;
            _zaposlenikService = service;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ZaposlenikDTO>> Update(int id, [FromBody] ZaposlenikDTO dto)
        {
            if (id != dto.Id && dto.Id != 0) return BadRequest("ID u ruti i tijelu zahtjeva se ne poklapaju.");

            var entity = await _context.Zaposlenici.FindAsync(id);
            if (entity == null) return NotFound();

            entity.Ime = dto.Ime.Trim();
            entity.Prezime = dto.Prezime.Trim();
            entity.Specijalizacija = dto.Specijalizacija.Trim();
            entity.Telefon = string.IsNullOrWhiteSpace(dto.Telefon)
                ? null
                : dto.Telefon.Trim();

            await _context.SaveChangesAsync();

            return Ok(new ZaposlenikDTO
            {
                Id = entity.Id,
                Ime = entity.Ime,
                Prezime = entity.Prezime,
                Specijalizacija = entity.Specijalizacija,
                Telefon = entity.Telefon,
                DatumZaposlenja = entity.DatumZaposlenja,
            });
        }

        [HttpGet("{id}/admin-profile")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TherapistAdminProfileDto>> GetAdminProfile(
            int id,
            [FromQuery] int maxReviews = 20)
        {
            var dto = await _zaposlenikService.GetAdminProfileAsync(id, maxReviews);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPatch("{id}/interna-napomena")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateInternaNapomena(
            int id,
            [FromBody] TherapistNotepadUpdateDto body)
        {
            var ok = await _zaposlenikService.UpdateInternaNapomenaAsync(id, body?.Napomena);
            if (!ok)
            {
                return BadRequest(
                    "Terapeut nema povezan korisnički nalog — interna napomena se ne može spremiti.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Zaposlenici.FindAsync(id);
            if (entity == null) return NotFound();

            var hasReservations = await _context.Rezervacije.AnyAsync(r => r.ZaposlenikId == id);
            if (hasReservations)
            {
                return Conflict(new { message = "Terapeut ima rezervacije i ne može biti obrisan." });
            }

            // Unlink therapist from user accounts and client preferred-therapist refs
            // (FK_AspNetUsers_Zaposlenici_ZaposlenikId blocks delete otherwise).
            await _context.Users
                .Where(k => k.ZaposlenikId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(k => k.ZaposlenikId, (int?)null));

            _context.Zaposlenici.Remove(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "Terapeut se ne može obrisati zbog povezanih podataka u bazi.",
                });
            }

            return NoContent();
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


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Api.Extensions;
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

        /// <summary>Override base route so literal paths like <c>me</c> are not parsed as numeric ids.</summary>
        [HttpGet("{id:int}")]
        public new async Task<ActionResult<ZaposlenikDTO>> GetById(int id)
        {
            var dto = await _zaposlenikService.GetById(id);
            if (dto == null || dto.Id == 0)
            {
                return NotFound();
            }

            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public override async Task<ZaposlenikDTO> Insert([FromBody] ZaposlenikDTO dto)
        {
            if (dto.KategorijaUslugaId is > 0)
            {
                var katOk = await _context.KategorijeUsluga.AsNoTracking()
                    .AnyAsync(k => k.Id == dto.KategorijaUslugaId);
                if (!katOk)
                {
                    throw new BadHttpRequestException("KategorijaUslugaId ne postoji.");
                }
            }

            var specError = await _zaposlenikService.ValidateSpecijalizacijaAsync(
                dto.KategorijaUslugaId,
                dto.Specijalizacija);
            if (specError != null)
            {
                throw new BadHttpRequestException(specError);
            }

            return await _zaposlenikService.Insert(dto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ZaposlenikDTO>> Update(int id, [FromBody] ZaposlenikDTO dto)
        {
            if (id != dto.Id && dto.Id != 0)
            {
                return BadRequest("ID u ruti i tijelu zahtjeva se ne poklapaju.");
            }

            if (dto.KategorijaUslugaId is > 0)
            {
                var katOk = await _context.KategorijeUsluga.AsNoTracking()
                    .AnyAsync(k => k.Id == dto.KategorijaUslugaId);
                if (!katOk) return BadRequest("KategorijaUslugaId ne postoji.");
            }

            var specError = await _zaposlenikService.ValidateSpecijalizacijaAsync(
                dto.KategorijaUslugaId,
                dto.Specijalizacija);
            if (specError != null)
            {
                return BadRequest(specError);
            }

            var updated = await _zaposlenikService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        [HttpGet("{id}/admin-profile")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TherapistAdminProfileDto>> GetAdminProfile(
            int id,
            [FromQuery] int maxReviews = 20,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var dto = await _zaposlenikService.GetAdminProfileAsync(id, maxReviews, from, to);
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

        [HttpGet("for-service/{uslugaId:int}")]
        [Authorize(Roles = "Admin,Klijent")]
        public async Task<ActionResult<IEnumerable<ZaposlenikDTO>>> GetForService(
            int uslugaId,
            [FromQuery] bool bookableOnly = true)
        {
            var list = await _zaposlenikService.GetForServiceAsync(uslugaId, bookableOnly);
            return Ok(list);
        }

        [HttpGet("for-category/{kategorijaUslugaId:int}")]
        [Authorize(Roles = "Admin,Klijent")]
        public async Task<ActionResult<IEnumerable<ZaposlenikDTO>>> GetForCategory(
            int kategorijaUslugaId,
            [FromQuery] bool bookableOnly = true)
        {
            var list = await _zaposlenikService.GetForCategoryAsync(
                kategorijaUslugaId,
                bookableOnly);
            return Ok(list);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Zaposlenik")]
        public async Task<ActionResult<ZaposlenikDTO>> GetMe()
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }
            var dto = await _zaposlenikService.GetMeAsync(id);
            if (dto == null || dto.Id == 0) return NotFound();
            return Ok(dto);
        }

        [HttpPatch("me")]
        [Authorize(Roles = "Zaposlenik")]
        public async Task<ActionResult<ZaposlenikDTO>> UpdateMe([FromBody] TherapistSelfProfileUpdateDto body)
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }
            var updated = await _zaposlenikService.UpdateMeAsync(id, body);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpGet("me/dashboard")]
        [Authorize(Roles = "Zaposlenik")]
        public async Task<ActionResult<TherapistDashboardDto>> GetMyDashboard([FromQuery] DateTime? day = null)
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }
            var dto = await _zaposlenikService.GetDashboardAsync(id, day);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpGet("me/reviews")]
        [Authorize(Roles = "Zaposlenik")]
        public async Task<ActionResult<IReadOnlyList<TherapistReviewRowDto>>> GetMyReviews(
            [FromQuery] int maxReviews = 30)
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }
            var list = await _zaposlenikService.GetMyReviewsAsync(id, maxReviews);
            return Ok(list);
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

            var dto = await _zaposlenikService.GetKpiAsync(id, from, to);
            if (dto == null) return NotFound();

            return Ok(dto);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Api.Extensions;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;

namespace NuaSpa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RezervacijaController : ControllerBase
    {
        private readonly IRezervacijaService _rezervacijaService;

        public RezervacijaController(IRezervacijaService rezervacijaService)
        {
            _rezervacijaService = rezervacijaService;
        }

        [HttpPost]
        [Authorize(Roles = "Klijent,Admin")]
        public async Task<ActionResult<RezervacijaDTO>> Create([FromBody] RezervacijaCreateDTO dto)
        {
            try
            {
                var korisnikId = User.GetNuaSpaUserId();
                var created = await _rezervacijaService.CreateAsync(korisnikId, dto);
                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RezervacijaDTO>>> Get([FromQuery] RezervacijaSearchObject? search = null)
        {
            var isAdmin = User.IsInRole("Admin");
            var isKlijent = User.IsInRole("Klijent");
            var isZaposlenik = User.IsInRole("Zaposlenik");

            if (!isAdmin && !isKlijent && !isZaposlenik)
            {
                return Unauthorized("Nemate permisije za listanje rezervacija.");
            }

            if (isZaposlenik)
            {
                var zaposlenikId = User.GetNuaSpaZaposlenikId();
                var zaposResult = await _rezervacijaService.GetForZaposlenikAsync(
                    zaposlenikId,
                    search?.Datum,
                    search?.IsPotvrdjena,
                    search?.IncludeOtkazane ?? false
                );

                return Ok(zaposResult);
            }

            int? korisnikId = null;
            if (isKlijent)
            {
                korisnikId = User.GetNuaSpaUserId();
            }
            else
            {
                korisnikId = search?.KorisnikId;
            }

            var result = await _rezervacijaService.GetAsync(
                korisnikId,
                search?.Datum,
                search?.IsPotvrdjena,
                search?.IncludeOtkazane ?? false
            );

            return Ok(result);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin,Zaposlenik")]
        public async Task<ActionResult> UpdatePotvrdjena(int id, [FromBody] RezervacijaUpdateDTO dto)
        {
            if (User.IsInRole("Admin"))
            {
                var updatedAdmin = await _rezervacijaService.UpdatePotvrdjenaAsync(id, dto.IsPotvrdjena);
                if (!updatedAdmin) return NotFound("Rezervacija nije pronađena.");
                return Ok();
            }

            var zaposlenikId = User.GetNuaSpaZaposlenikId();
            var updated = await _rezervacijaService.UpdatePotvrdjenaForZaposlenikAsync(id, zaposlenikId, dto.IsPotvrdjena);
            if (!updated) return NotFound("Rezervacija nije pronađena (ili nemate pristup).");
            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RezervacijaDTO>> Edit(int id, [FromBody] RezervacijaEditDTO dto)
        {
            try
            {
                var updated = await _rezervacijaService.EditAsync(id, dto);
                if (updated == null) return NotFound("Rezervacija nije pronađena.");
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "Admin,Klijent,Zaposlenik")]
        public async Task<ActionResult> Cancel(int id, [FromBody] RezervacijaCancelDTO dto)
        {
            int? requireKorisnikId = null;
            int? requireZaposlenikId = null;

            if (User.IsInRole("Klijent") && !User.IsInRole("Admin"))
            {
                requireKorisnikId = User.GetNuaSpaUserId();
            }
            if (User.IsInRole("Zaposlenik") && !User.IsInRole("Admin"))
            {
                requireZaposlenikId = User.GetNuaSpaZaposlenikId();
            }

            var ok = await _rezervacijaService.CancelAsync(
                id,
                requireKorisnikId,
                requireZaposlenikId,
                dto?.RazlogOtkaza
            );

            if (!ok) return BadRequest("Nije moguće otkazati rezervaciju.");
            return Ok();
        }

        [HttpGet("dostupni-termini")]
        [Authorize(Roles = "Klijent,Admin,Zaposlenik")]
        public async Task<ActionResult<List<DateTime>>> GetDostupniTermini([FromQuery] int zaposlenikId, [FromQuery] DateTime datum)
        {
            // Teraput smije tražiti samo svoje termine.
            if (User.IsInRole("Zaposlenik"))
            {
                var myId = User.GetNuaSpaZaposlenikId();
                if (myId != zaposlenikId) return Forbid();
            }

            var slots = await _rezervacijaService.GetAvailableSlotsAsync(zaposlenikId, datum);
            return Ok(slots);
        }

        [HttpGet("calendar")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<RezervacijaCalendarItemDTO>>> GetCalendar(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int? zaposlenikId = null,
            [FromQuery] bool includeOtkazane = false)
        {
            if (to < from) return BadRequest("Neispravan period (to < from).");
            // Guardrail: max 31 days per request to keep payload bounded.
            if ((to.Date - from.Date).TotalDays > 31)
            {
                return BadRequest("Period je prevelik (max 31 dana).");
            }

            var items = await _rezervacijaService.GetCalendarAsync(
                from,
                to,
                zaposlenikId,
                includeOtkazane);
            return Ok(items);
        }
    }
}


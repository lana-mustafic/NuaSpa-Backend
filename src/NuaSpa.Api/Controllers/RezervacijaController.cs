using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var korisnikId = GetUserId();
            var created = await _rezervacijaService.CreateAsync(korisnikId, dto);
            return Ok(created);
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
                var zaposlenikId = GetZaposlenikId();
                var zaposResult = await _rezervacijaService.GetForZaposlenikAsync(
                    zaposlenikId,
                    search?.Datum,
                    search?.IsPotvrdjena
                );

                return Ok(zaposResult);
            }

            int? korisnikId = null;
            if (isKlijent)
            {
                korisnikId = GetUserId();
            }
            else
            {
                korisnikId = search?.KorisnikId;
            }

            var result = await _rezervacijaService.GetAsync(
                korisnikId,
                search?.Datum,
                search?.IsPotvrdjena
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

            var zaposlenikId = GetZaposlenikId();
            var updated = await _rezervacijaService.UpdatePotvrdjenaForZaposlenikAsync(id, zaposlenikId, dto.IsPotvrdjena);
            if (!updated) return NotFound("Rezervacija nije pronađena (ili nemate pristup).");
            return Ok();
        }

        [HttpGet("dostupni-termini")]
        [Authorize(Roles = "Klijent,Admin,Zaposlenik")]
        public async Task<ActionResult<List<DateTime>>> GetDostupniTermini([FromQuery] int zaposlenikId, [FromQuery] DateTime datum)
        {
            // Teraput smije tražiti samo svoje termine.
            if (User.IsInRole("Zaposlenik"))
            {
                var myId = GetZaposlenikId();
                if (myId != zaposlenikId) return Forbid();
            }

            var slots = await _rezervacijaService.GetAvailableSlotsAsync(zaposlenikId, datum);
            return Ok(slots);
        }

        private int GetUserId()
        {
            var idStr = User.FindFirstValue(JwtRegisteredClaimNames.NameId)
                         ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(idStr, out var userId))
            {
                throw new UnauthorizedAccessException("Ne mogu pročitati korisnički id iz JWT-a.");
            }

            return userId;
        }

        private int GetZaposlenikId()
        {
            var idStr = User.FindFirstValue("ZaposlenikId");
            if (!int.TryParse(idStr, out var zaposlenikId))
            {
                throw new UnauthorizedAccessException("Korisnik nema ZaposlenikId claim u tokenu.");
            }

            return zaposlenikId;
        }
    }
}


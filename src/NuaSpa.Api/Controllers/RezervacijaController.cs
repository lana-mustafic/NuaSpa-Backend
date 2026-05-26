using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Api.Extensions;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.Interfaces.Messaging;
using NuaSpa.Application.SearchObjects;

namespace NuaSpa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RezervacijaController : ControllerBase
    {
        private readonly IRezervacijaService _rezervacijaService;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly ILogger<RezervacijaController> _logger;

        public RezervacijaController(
            IRezervacijaService rezervacijaService,
            INotificationPublisher notificationPublisher,
            ILogger<RezervacijaController> logger)
        {
            _rezervacijaService = rezervacijaService;
            _notificationPublisher = notificationPublisher;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = RoleConstants.KlijentAdmin)]
        public async Task<ActionResult<RezervacijaDTO>> Create([FromBody] RezervacijaCreateDTO dto)
        {
            if (!User.IsInRole(RoleConstants.Admin) &&
                dto.KorisnikId.HasValue &&
                dto.KorisnikId.Value != User.GetNuaSpaUserId())
            {
                return Forbid();
            }

            var korisnikId = User.IsInRole(RoleConstants.Admin) && dto.KorisnikId.HasValue
                ? dto.KorisnikId.Value
                : User.GetNuaSpaUserId();
            var isAdminBooking = User.IsInRole(RoleConstants.Admin);
            var created = await _rezervacijaService.CreateAsync(korisnikId, dto, isAdminBooking);
            try
            {
                await _notificationPublisher.PublishRezervacijaPotvrdaAsync(created);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ notifikacija za rezervaciju {Id} nije poslana.", created.Id);
            }

            return Ok(created);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<RezervacijaDTO>> GetById(int id)
        {
            var dto = await _rezervacijaService.GetByIdAsync(id);
            if (dto == null)
            {
                return NotFound("Rezervacija nije pronađena.");
            }

            var isAdmin = User.IsInRole(RoleConstants.Admin);
            if (User.IsInRole(RoleConstants.Klijent) && !isAdmin)
            {
                if (dto.KorisnikId != User.GetNuaSpaUserId())
                {
                    return Forbid();
                }
            }

            if (User.IsInRole(RoleConstants.Zaposlenik) && !isAdmin)
            {
                if (!User.TryGetNuaSpaZaposlenikId(out var zaposlenikId) ||
                    dto.ZaposlenikId != zaposlenikId)
                {
                    return Forbid();
                }
            }

            return Ok(dto);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RezervacijaDTO>>> Get([FromQuery] RezervacijaSearchObject? search = null)
        {
            var isAdmin = User.IsInRole(RoleConstants.Admin);
            var isKlijent = User.IsInRole(RoleConstants.Klijent);
            var isZaposlenik = User.IsInRole(RoleConstants.Zaposlenik);

            if (!isAdmin && !isKlijent && !isZaposlenik)
            {
                return Unauthorized("Nemate permisije za listanje rezervacija.");
            }

            if (isZaposlenik)
            {
                if (!User.TryGetNuaSpaZaposlenikId(out var zaposlenikId))
                {
                    return Forbid();
                }

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

            int? zaposlenikFilter = isAdmin ? search?.ZaposlenikId : null;

            var result = await _rezervacijaService.GetAsync(
                korisnikId,
                search?.Datum,
                search?.IsPotvrdjena,
                search?.IncludeOtkazane ?? false,
                zaposlenikFilter
            );

            return Ok(result);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = RoleConstants.AdminZaposlenik)]
        public async Task<ActionResult> UpdatePotvrdjena(int id, [FromBody] RezervacijaUpdateDTO dto)
        {
            var actorUserId = User.GetNuaSpaUserId();
            try
            {
                if (User.IsInRole(RoleConstants.Admin))
                {
                    var updatedAdmin = await _rezervacijaService.UpdatePotvrdjenaAsync(
                        id, dto.IsPotvrdjena, actorUserId);
                    if (!updatedAdmin) return NotFound("Rezervacija nije pronađena.");
                }
                else
                {
                    if (!User.TryGetNuaSpaZaposlenikId(out var zaposlenikId))
                    {
                        return Forbid();
                    }

                    var updated = await _rezervacijaService.UpdatePotvrdjenaForZaposlenikAsync(
                        id, zaposlenikId, dto.IsPotvrdjena, actorUserId);
                    if (!updated) return NotFound("Rezervacija nije pronađena (ili nemate pristup).");
                }

                if (dto.IsPotvrdjena)
                {
                    var confirmed = await _rezervacijaService.GetByIdAsync(id);
                    if (confirmed != null)
                    {
                        try
                        {
                            await _notificationPublisher.PublishRezervacijaPotvrdaAsync(confirmed);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Notifikacija potvrde rezervacije {Id} nije poslana.", id);
                        }
                    }
                }

                return Ok();
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id:int}/complete")]
        [Authorize(Roles = RoleConstants.AdminZaposlenik)]
        public async Task<ActionResult> Complete(int id)
        {
            try
            {
                var ok = await _rezervacijaService.CompleteAsync(
                    id,
                    User.GetNuaSpaUserId(),
                    allowBeforeEnd: User.IsInRole(RoleConstants.Admin));
                if (!ok) return NotFound("Rezervacija nije pronađena.");
                return Ok();
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<ActionResult<RezervacijaDTO>> Edit(int id, [FromBody] RezervacijaEditDTO dto)
        {
            var updated = await _rezervacijaService.EditAsync(id, dto);
            if (updated == null) return NotFound("Rezervacija nije pronađena.");
            return Ok(updated);
        }

        [HttpPatch("{id:int}/vip")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<ActionResult> SetVip(int id, [FromBody] RezervacijaVipDto dto)
        {
            if (dto == null) return BadRequest();
            var ok = await _rezervacijaService.SetIsVipAsync(id, dto.IsVip);
            if (!ok) return NotFound("Rezervacija nije pronađena ili je otkazana.");
            return Ok();
        }

        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = RoleConstants.AdminKlijentZaposlenik)]
        public async Task<ActionResult> Cancel(int id, [FromBody] RezervacijaCancelDTO dto)
        {
            int? requireKorisnikId = null;
            int? requireZaposlenikId = null;

            if (User.IsInRole(RoleConstants.Klijent) && !User.IsInRole(RoleConstants.Admin))
            {
                requireKorisnikId = User.GetNuaSpaUserId();
            }
            if (User.IsInRole(RoleConstants.Zaposlenik) && !User.IsInRole(RoleConstants.Admin))
            {
                if (!User.TryGetNuaSpaZaposlenikId(out var zid))
                {
                    return Forbid();
                }

                requireZaposlenikId = zid;
            }

            try
            {
                var ok = await _rezervacijaService.CancelAsync(
                    id,
                    requireKorisnikId,
                    requireZaposlenikId,
                    User.GetNuaSpaUserId(),
                    dto?.RazlogOtkaza ?? string.Empty);

                if (!ok) return BadRequest(new { message = "Nije moguće otkazati rezervaciju." });

                var cancelled = await _rezervacijaService.GetByIdAsync(id);
                if (cancelled != null)
                {
                    var uloga = User.IsInRole(RoleConstants.Admin)
                        ? "Administrator"
                        : User.IsInRole(RoleConstants.Zaposlenik)
                            ? "Terapeut"
                            : "Klijent";
                    try
                    {
                        await _notificationPublisher.PublishRezervacijaOtkazanaAsync(
                            cancelled,
                            dto!.RazlogOtkaza!.Trim(),
                            uloga);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Notifikacija otkazivanja rezervacije {Id} nije poslana.", id);
                    }
                }

                return Ok();
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var (ok, message) = await _rezervacijaService.DeleteAdminAsync(id);
            return Conflict(new { message = message ?? "Hard delete nije dozvoljen. Koristite otkazivanje." });
        }

        [HttpGet("dostupni-termini")]
        [Authorize(Roles = RoleConstants.AdminKlijentZaposlenik)]
        public async Task<ActionResult<List<DateTime>>> GetDostupniTermini(
            [FromQuery] int zaposlenikId,
            [FromQuery] DateTime datum,
            [FromQuery] int? uslugaId = null)
        {
            // Teraput smije tražiti samo svoje termine.
            if (User.IsInRole(RoleConstants.Zaposlenik))
            {
                if (!User.TryGetNuaSpaZaposlenikId(out var myId) || myId != zaposlenikId)
                {
                    return Forbid();
                }
            }

            var slots = await _rezervacijaService.GetAvailableSlotsAsync(
                zaposlenikId,
                datum,
                uslugaId);
            return Ok(slots);
        }

        [HttpGet("calendar")]
        [Authorize(Roles = RoleConstants.AdminZaposlenik)]
        public async Task<ActionResult<List<RezervacijaCalendarItemDTO>>> GetCalendar(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int? zaposlenikId = null,
            [FromQuery] int? uslugaId = null,
            [FromQuery] int? prostorijaId = null,
            [FromQuery] string? q = null,
            [FromQuery] bool includeOtkazane = false)
        {
            if (to < from) return BadRequest("Neispravan period (to < from).");
            // Guardrail: max 31 days per request to keep payload bounded.
            if ((to.Date - from.Date).TotalDays > 31)
            {
                return BadRequest("Period je prevelik (max 31 dana).");
            }

            var isAdmin = User.IsInRole(RoleConstants.Admin);
            int? zFilter = zaposlenikId;
            if (!isAdmin)
            {
                if (!User.TryGetNuaSpaZaposlenikId(out var myZ))
                {
                    return Forbid();
                }

                if (zFilter.HasValue && zFilter.Value != myZ)
                {
                    return Forbid();
                }

                zFilter = myZ;
            }

            var items = await _rezervacijaService.GetCalendarAsync(
                from,
                to,
                zFilter,
                includeOtkazane,
                uslugaId,
                prostorijaId,
                q);
            return Ok(items);
        }

        /// <summary>
        /// Štiklirana povijest termina jednog klijenta. Terapeut samo za zajedničke rezervacije; admin sve.
        /// </summary>
        [HttpGet("povijest-za-klijenta")]
        [Authorize(Roles = RoleConstants.AdminZaposlenik)]
        [ProducesResponseType(typeof(List<RezervacijaPovijestItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RezervacijaPovijestItemDto>>> GetPovijestZaKlijenta(
            [FromQuery] int korisnikId,
            [FromQuery] int? excludeRezervacijaId = null,
            [FromQuery] int take = 20)
        {
            if (korisnikId <= 0)
                return BadRequest("Neispravan korisnikId.");

            var isAdmin = User.IsInRole(RoleConstants.Admin);
            var zaposId = 0;
            if (User.IsInRole(RoleConstants.Zaposlenik))
            {
                if (!User.TryGetNuaSpaZaposlenikId(out zaposId))
                {
                    return Forbid();
                }
            }

            var list = await _rezervacijaService.GetPovijestZaKlijentaAsync(
                isAdmin,
                zaposId,
                korisnikId,
                excludeRezervacijaId,
                take);

            return Ok(list);
        }
    }
}


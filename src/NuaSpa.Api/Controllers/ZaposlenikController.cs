using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Api.Extensions;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;
using NuaSpa.Application.Common;

namespace NuaSpa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ZaposlenikController : BaseController<ZaposlenikDTO, ZaposlenikSearchObject>
    {
        private readonly IZaposlenikService _zaposlenikService;
        private readonly IWebHostEnvironment _env;

    public ZaposlenikController(
            IZaposlenikService service,
            IWebHostEnvironment env) : base(service)
        {
            _zaposlenikService = service;
            _env = env;
        }

        [HttpGet]
        [Authorize(Roles = RoleConstants.Admin)]
        public override async Task<ActionResult<PagedResult<ZaposlenikDTO>>> Get(
            [FromQuery] ZaposlenikSearchObject? search = null)
        {
            var page = await _zaposlenikService.Get(search);
            return Ok(page);
        }

        [HttpGet("admin-roster")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<ActionResult<TherapistAdminRosterDto>> GetAdminRoster(
            [FromQuery] DateTime? kpiFrom = null,
            [FromQuery] DateTime? kpiTo = null,
            [FromQuery] DateTime? weekStart = null)
        {
            var dto = await _zaposlenikService.GetAdminRosterAsync(kpiFrom, kpiTo, weekStart);
            return Ok(dto);
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
        [Authorize(Roles = RoleConstants.Admin)]
        public override async Task<ActionResult<ZaposlenikDTO>> Insert([FromBody] ZaposlenikDTO dto)
        {
            var created = await _zaposlenikService.Insert(dto);
            return Ok(created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<ActionResult<ZaposlenikDTO>> Update(int id, [FromBody] ZaposlenikDTO dto)
        {
            if (id != dto.Id && dto.Id != 0)
            {
                return BadRequest("ID u ruti i tijelu zahtjeva se ne poklapaju.");
            }

            var updated = await _zaposlenikService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        [HttpGet("{id}/admin-profile")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<ActionResult<TherapistAdminProfileDto>> GetAdminProfile(
            int id,
            [FromQuery] int maxReviews = 20,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] DateTime? weekStart = null)
        {
            var dto = await _zaposlenikService.GetAdminProfileAsync(
                id, maxReviews, from, to, weekStart);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPatch("{id}/interna-napomena")]
        [Authorize(Roles = RoleConstants.Admin)]
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
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            await _zaposlenikService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("for-service/{uslugaId:int}")]
        [Authorize(Roles = RoleConstants.AdminKlijent)]
        public async Task<ActionResult<IEnumerable<ZaposlenikDTO>>> GetForService(
            int uslugaId,
            [FromQuery] bool bookableOnly = true)
        {
            var list = await _zaposlenikService.GetForServiceAsync(uslugaId, bookableOnly);
            return Ok(list);
        }

        [HttpGet("for-category/{kategorijaUslugaId:int}")]
        [Authorize(Roles = RoleConstants.AdminKlijent)]
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
        [Authorize(Roles = RoleConstants.Zaposlenik)]
        public async Task<ActionResult<ZaposlenikDTO>> GetMe()
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }
            var dto = await _zaposlenikService.GetMeAsync(id);
            if (dto == null || dto.Id == 0)
            {
                return NotFound(new
                {
                    message =
                        "Therapist profile not found. Contact your spa administrator to link your account.",
                });
            }
            return Ok(dto);
        }

        [HttpGet("me/profile")]
        [Authorize(Roles = RoleConstants.Zaposlenik)]
        public async Task<ActionResult<TherapistMyProfileDto>> GetMyProfile()
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }

            var dto = await _zaposlenikService.GetMyProfileAsync(id);
            if (dto == null)
            {
                return NotFound(new
                {
                    message =
                        "Therapist profile not found. Contact your spa administrator to link your account.",
                });
            }

            return Ok(dto);
        }

        [HttpGet("me/services")]
        [Authorize(Roles = RoleConstants.Zaposlenik)]
        public async Task<ActionResult<IReadOnlyList<UslugaDTO>>> GetMyServices()
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }

            var list = await _zaposlenikService.GetMyServicesAsync(id);
            return Ok(list);
        }

        [HttpGet("me/services/{uslugaId:int}")]
        [Authorize(Roles = RoleConstants.Zaposlenik)]
        public async Task<ActionResult<TherapistServiceDetailDto>> GetMyServiceDetail(int uslugaId)
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }

            var detail = await _zaposlenikService.GetMyServiceDetailAsync(id, uslugaId);
            if (detail == null)
            {
                return NotFound();
            }

            if (!detail.IsAuthorized)
            {
                return Forbid();
            }

            return Ok(detail);
        }

        [HttpPatch("me")]
        [Authorize(Roles = RoleConstants.Zaposlenik)]
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

        /// <summary>Upload therapist profile photo; updates SlikaUrl on the staff record.</summary>
        [HttpPost("me/avatar")]
        [Authorize(Roles = RoleConstants.Zaposlenik)]
        [RequestSizeLimit(4_000_000)]
        public async Task<ActionResult<object>> UploadMyAvatar(
            IFormFile? file,
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file selected." });
            }

            if (file.Length > 4_000_000)
            {
                return BadRequest(new { message = "File is too large (max 4 MB)." });
            }

            await using var readStream = file.OpenReadStream();
            if (!UploadImageValidator.TryValidate(
                    file.FileName,
                    file.ContentType,
                    readStream,
                    out var validationError))
            {
                return BadRequest(new { message = validationError });
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var dir = Path.Combine(webRoot, "uploads", "terapeuti");
            Directory.CreateDirectory(dir);

            var safeName = $"{id}_{Guid.NewGuid():N}{ext}";
            var physical = Path.Combine(dir, safeName);

            readStream.Position = 0;
            await using (var outStream = System.IO.File.Create(physical))
            {
                await readStream.CopyToAsync(outStream, cancellationToken);
            }

            var url = $"/api/files/terapeuti/{safeName}";
            var updated = await _zaposlenikService.UpdateMyAvatarUrlAsync(id, url);
            if (updated == null)
            {
                return NotFound();
            }

            return Ok(new { url, profile = updated });
        }

        [HttpGet("me/dashboard")]
        [Authorize(Roles = RoleConstants.Zaposlenik)]
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

        [HttpGet("me/appointments")]
        [Authorize(Roles = RoleConstants.Zaposlenik)]
        public async Task<ActionResult<TherapistAppointmentsListDto>> GetMyAppointments(
            [FromQuery] TherapistAppointmentsSearchObject search)
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }

            var (page, pageSize) = PaginationHelper.FromSearch(search);
            var dto = await _zaposlenikService.GetMyAppointmentsAsync(
                id,
                search?.Tab ?? "upcoming",
                search?.Day,
                search?.Search,
                search?.StatusFilter ?? "all",
                page,
                pageSize,
                search?.UslugaId);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpGet("me/schedule")]
        [Authorize(Roles = RoleConstants.Zaposlenik)]
        public async Task<ActionResult<TherapistScheduleDto>> GetMySchedule(
            [FromQuery] TherapistScheduleSearchObject search)
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }

            var dto = await _zaposlenikService.GetMyScheduleAsync(
                id,
                search?.Day,
                search?.CalendarMonth);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpGet("me/reviews")]
        [Authorize(Roles = RoleConstants.Zaposlenik)]
        public async Task<ActionResult<PagedResult<TherapistReviewRowDto>>> GetMyReviews(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? uslugaId = null)
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }
            var pageResult = await _zaposlenikService.GetMyReviewsPagedAsync(
                id,
                page,
                pageSize,
                uslugaId);
            return Ok(pageResult);
        }

        [HttpGet("me/reviews/summary")]
        [Authorize(Roles = RoleConstants.Zaposlenik)]
        public async Task<ActionResult<TherapistMyReviewsSummaryDto>> GetMyReviewsSummary(
            [FromQuery] int? uslugaId = null)
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }
            var summary = await _zaposlenikService.GetMyReviewsSummaryAsync(id, uslugaId);
            return Ok(summary);
        }

        [HttpGet("{id}/kpi")]
        [Authorize(Roles = RoleConstants.Admin)]
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



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.SearchObjects;
using NuaSpa.Application.Common;

namespace NuaSpa.Api.Controllers
{
    [Authorize]
    public class KategorijaUslugaController : BaseController<KategorijaUslugaDTO, KategorijaUslugaSearchObject>
    {
        private readonly IKategorijaUslugaService _kategorijaService;

        public KategorijaUslugaController(IKategorijaUslugaService service) : base(service)
        {
            _kategorijaService = service;
        }

        [HttpPost]
        [Authorize(Roles = RoleConstants.Admin)]
        public override async Task<ActionResult<KategorijaUslugaDTO>> Insert([FromBody] KategorijaUslugaDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Naziv))
            {
                throw new BadHttpRequestException("Naziv kategorije je obavezan.");
            }

            dto.Naziv = dto.Naziv.Trim();
            return await base.Insert(dto);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<ActionResult<KategorijaUslugaDTO>> Update(int id, [FromBody] KategorijaUslugaDTO dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID u ruti i u tijelu zahtjeva se ne poklapaju.");
            }

            try
            {
                return Ok(await _kategorijaService.UpdateAsync(dto));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var (ok, message) = await _kategorijaService.DeleteAsync(id);
            if (!ok)
            {
                return Conflict(new { message });
            }

            return NoContent();
        }
    }
}
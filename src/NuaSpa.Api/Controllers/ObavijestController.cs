using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ObavijestController : ControllerBase
{
    private readonly IObavijestService _service;
    private readonly IWebHostEnvironment _env;

    public ObavijestController(IObavijestService service, IWebHostEnvironment env)
    {
        _service = service;
        _env = env;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PagedResult<ObavijestDto>>> GetPublished(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationConstants.DefaultPageSize,
        CancellationToken ct = default)
    {
        if (User.IsInRole(RoleConstants.Admin))
        {
            return Ok(await _service.GetAllAdminAsync(page, pageSize, ct));
        }

        return Ok(await _service.GetPublishedAsync(page, pageSize, ct));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ObavijestDto>> GetById(int id, CancellationToken ct = default)
    {
        var item = await _service.GetByIdAsync(id, ct);
        if (item == null)
        {
            return NotFound();
        }

        if (!User.IsInRole(RoleConstants.Admin) && !item.Aktivna)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<ActionResult<ObavijestDto>> Create(
        [FromBody] ObavijestCreateDto dto,
        CancellationToken ct = default)
    {
        try
        {
            var created = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<ActionResult<ObavijestDto>> Update(
        int id,
        [FromBody] ObavijestUpdateDto dto,
        CancellationToken ct = default)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto, ct);
            if (updated == null)
            {
                return NotFound();
            }

            return Ok(updated);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var ok = await _service.DeleteAsync(id, ct);
        if (!ok)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("upload-image")]
    [Authorize(Roles = RoleConstants.Admin)]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult<object>> UploadImage(IFormFile? file, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Datoteka nije poslana." });
        }

        if (file.Length > 5_000_000)
        {
            return BadRequest(new { message = "Datoteka je prevelika (maks. 5 MB)." });
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
        var dir = Path.Combine(webRoot, "uploads", "obavijesti");
        Directory.CreateDirectory(dir);

        var safeName = $"{Guid.NewGuid():N}{ext}";
        var physical = Path.Combine(dir, safeName);

        if (readStream.CanSeek)
        {
            readStream.Position = 0;
        }

        await using (var outStream = System.IO.File.Create(physical))
        {
            await readStream.CopyToAsync(outStream, ct);
        }

        return Ok(new { url = $"/api/files/obavijesti/{safeName}" });
    }
}

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
    public async Task<ActionResult<IReadOnlyList<ObavijestDto>>> GetPublished(CancellationToken ct = default)
    {
        if (User.IsInRole(RoleConstants.Admin))
        {
            return Ok(await _service.GetAllAdminAsync(ct));
        }

        return Ok(await _service.GetPublishedAsync(ct));
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

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
        {
            return BadRequest(new { message = "Dozvoljeni formati: jpg, png, webp." });
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var dir = Path.Combine(webRoot, "uploads", "obavijesti");
        Directory.CreateDirectory(dir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(dir, fileName);
        await using (var stream = System.IO.File.Create(path))
        {
            await file.CopyToAsync(stream, ct);
        }

        return Ok(new { url = $"/uploads/obavijesti/{fileName}" });
    }
}

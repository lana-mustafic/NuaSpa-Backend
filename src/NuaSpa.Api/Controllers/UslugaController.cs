using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;
using NuaSpa.Application.Interfaces.Messaging;

namespace NuaSpa.Api.Controllers;

[Authorize]
public class UslugaController : BaseController<UslugaDTO, UslugaSearchObject>
{
    private readonly IRabbitMQProducer _rabbitMQProducer;
    private readonly IUslugaService _uslugaService;
    private readonly IWebHostEnvironment _env;

    public UslugaController(
        IUslugaService service,
        IRabbitMQProducer rabbitMQProducer,
        IWebHostEnvironment env) : base(service)
    {
        _uslugaService = service;
        _rabbitMQProducer = rabbitMQProducer;
        _env = env;
    }

    /// <summary>Admin: učitava sliku usluge u wwwroot i vraća javni URL.</summary>
    [HttpPost("upload-image")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(8_000_000)]
    public async Task<ActionResult<object>> UploadImage(
        IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Nije odabrana datoteka." });
        }

        if (file.Length > 8_000_000)
        {
            return BadRequest(new { message = "Datoteka je prevelika (maks. 8 MB)." });
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        string[] allowed = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
        if (string.IsNullOrEmpty(ext) || !allowed.Contains(ext))
        {
            return BadRequest(new { message = "Dopušteni formati: JPG, PNG, WEBP, GIF." });
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var dir = Path.Combine(webRoot, "uploads", "usluge");
        Directory.CreateDirectory(dir);

        var safeName = $"{Guid.NewGuid():N}{ext}";
        var physical = Path.Combine(dir, safeName);

        await using (var stream = System.IO.File.Create(physical))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var url = $"{Request.Scheme}://{Request.Host}/uploads/usluge/{safeName}";
        return Ok(new { url });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public override async Task<UslugaDTO> Insert([FromBody] UslugaDTO dto)
    {
        var result = await base.Insert(dto);
        await _rabbitMQProducer.SendMessage(result, "usluge_queue");
        return result;
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UslugaDTO>> Update(int id, [FromBody] UslugaDTO dto)
    {
        if (id != dto.Id)
        {
            return BadRequest("ID u ruti i u tijelu zahtjeva se ne poklapaju.");
        }

        try
        {
            var updated = await _uslugaService.UpdateAsync(dto);
            await _rabbitMQProducer.SendMessage(updated, "usluge_queue");
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, message) = await _uslugaService.DeleteAsync(id);
        if (!ok)
        {
            return Conflict(new { message });
        }

        return NoContent();
    }
}
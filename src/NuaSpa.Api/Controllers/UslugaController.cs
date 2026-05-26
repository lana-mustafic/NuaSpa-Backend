using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;
using NuaSpa.Application.Interfaces.Messaging;

namespace NuaSpa.Api.Controllers;

[Authorize]
public class UslugaController : BaseController<UslugaDTO, UslugaSearchObject>
{
    private readonly INotificationPublisher _notificationPublisher;
    private readonly IUslugaService _uslugaService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<UslugaController> _logger;

    public UslugaController(
        IUslugaService service,
        INotificationPublisher notificationPublisher,
        IWebHostEnvironment env,
        ILogger<UslugaController> logger) : base(service)
    {
        _uslugaService = service;
        _notificationPublisher = notificationPublisher;
        _env = env;
        _logger = logger;
    }

    /// <summary>Admin: učitava sliku usluge u wwwroot i vraća javni URL.</summary>
    [HttpPost("upload-image")]
    [Authorize(Roles = RoleConstants.Admin)]
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
        var dir = Path.Combine(webRoot, "uploads", "usluge");
        Directory.CreateDirectory(dir);

        var safeName = $"{Guid.NewGuid():N}{ext}";
        var physical = Path.Combine(dir, safeName);

        readStream.Position = 0;
        await using (var outStream = System.IO.File.Create(physical))
        {
            await readStream.CopyToAsync(outStream, cancellationToken);
        }

        var url = $"{Request.Scheme}://{Request.Host}/api/files/usluge/{safeName}";
        return Ok(new { url });
    }

    [HttpPost]
    [Authorize(Roles = RoleConstants.Admin)]
    public override async Task<ActionResult<UslugaDTO>> Insert([FromBody] UslugaDTO dto)
    {
        var created = await _uslugaService.Insert(dto);
        try
        {
            await _notificationPublisher.PublishUslugaKreiranaAsync(created);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ notifikacija za novu uslugu nije poslana.");
        }

        return Ok(created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<ActionResult<UslugaDTO>> Update(int id, [FromBody] UslugaDTO dto)
    {
        if (id != dto.Id)
        {
            return BadRequest("ID u ruti i u tijelu zahtjeva se ne poklapaju.");
        }

        try
        {
            var updated = await _uslugaService.UpdateAsync(dto);
            return Ok(updated);
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
        var (ok, message) = await _uslugaService.DeleteAsync(id);
        if (!ok)
        {
            return Conflict(new { message });
        }

        return NoContent();
    }
}
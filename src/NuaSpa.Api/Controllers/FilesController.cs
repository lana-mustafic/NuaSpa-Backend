using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NuaSpa.Api.Controllers;

/// <summary>Autorizovani pristup uploadanim datotekama (ne javni static).</summary>
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IWebHostEnvironment env, ILogger<FilesController> logger)
    {
        _env = env;
        _logger = logger;
    }

    /// <summary>Javno — koristi se u &lt;img&gt; / Image.network bez JWT zaglavlja.</summary>
    [AllowAnonymous]
    [HttpGet("usluge/{fileName}")]
    [ResponseCache(Duration = 3600)]
    public IActionResult GetUslugaImage(string fileName)
    {
        if (!TryResolveUploadedFile("usluge", fileName, out var physical, out var contentType))
        {
            return NotFound();
        }

        return PhysicalFile(physical, contentType);
    }

    [AllowAnonymous]
    [HttpGet("terapeuti/{fileName}")]
    [ResponseCache(Duration = 3600)]
    public IActionResult GetTherapistAvatar(string fileName)
    {
        if (!TryResolveUploadedFile("terapeuti", fileName, out var physical, out var contentType))
        {
            return NotFound();
        }

        return PhysicalFile(physical, contentType);
    }

    [AllowAnonymous]
    [HttpGet("obavijesti/{fileName}")]
    [ResponseCache(Duration = 3600)]
    public IActionResult GetObavijestImage(string fileName)
    {
        if (!TryResolveUploadedFile("obavijesti", fileName, out var physical, out var contentType))
        {
            return NotFound();
        }

        return PhysicalFile(physical, contentType);
    }

    private bool TryResolveUploadedFile(
        string folder,
        string fileName,
        out string physicalPath,
        out string contentType)
    {
        physicalPath = string.Empty;
        contentType = "application/octet-stream";

        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            fileName.Contains("..", StringComparison.Ordinal))
        {
            return false;
        }

        var safeName = Path.GetFileName(fileName);
        if (!string.Equals(safeName, fileName, StringComparison.Ordinal))
        {
            return false;
        }

        var ext = Path.GetExtension(safeName).ToLowerInvariant();
        contentType = ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => string.Empty,
        };

        if (string.IsNullOrEmpty(contentType))
        {
            return false;
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        physicalPath = Path.GetFullPath(Path.Combine(webRoot, "uploads", folder, safeName));
        var root = Path.GetFullPath(Path.Combine(webRoot, "uploads", folder));

        if (!physicalPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) ||
            !System.IO.File.Exists(physicalPath))
        {
            _logger.LogDebug("Tražena slika nije pronađena: {Folder}/{File}", folder, safeName);
            return false;
        }

        return true;
    }
}

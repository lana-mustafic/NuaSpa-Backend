using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.Common;

namespace NuaSpa.Api.Controllers;

/// <summary>Autorizovani pristup uploadanim datotekama (ne javni static).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IWebHostEnvironment env, ILogger<FilesController> logger)
    {
        _env = env;
        _logger = logger;
    }

    [HttpGet("usluge/{fileName}")]
    [ResponseCache(Duration = 3600)]
    public IActionResult GetUslugaImage(string fileName)
    {
        if (!TryResolveUslugaFile(fileName, out var physical, out var contentType))
        {
            return NotFound();
        }

        return PhysicalFile(physical, contentType);
    }

    private bool TryResolveUslugaFile(string fileName, out string physicalPath, out string contentType)
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
        physicalPath = Path.GetFullPath(Path.Combine(webRoot, "uploads", "usluge", safeName));
        var root = Path.GetFullPath(Path.Combine(webRoot, "uploads", "usluge"));

        if (!physicalPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) ||
            !System.IO.File.Exists(physicalPath))
        {
            _logger.LogDebug("Tražena slika usluge nije pronađena: {File}", safeName);
            return false;
        }

        return true;
    }
}

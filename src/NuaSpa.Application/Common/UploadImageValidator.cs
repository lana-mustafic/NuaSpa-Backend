namespace NuaSpa.Application.Common;

/// <summary>Validacija MIME tipa i magic bytes za slike (ne samo ekstenzija).</summary>
public static class UploadImageValidator
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
    };

    public static bool TryValidate(
        string fileName,
        string? contentType,
        Stream content,
        out string? errorMessage)
    {
        errorMessage = null;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            errorMessage = "Dopušteni formati: JPG, PNG, WEBP, GIF.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(contentType) &&
            !AllowedContentTypes.Contains(contentType.Split(';')[0].Trim()))
        {
            errorMessage = "MIME tip datoteke nije dopušten za upload slike.";
            return false;
        }

        if (!content.CanRead)
        {
            errorMessage = "Datoteka se ne može pročitati.";
            return false;
        }

        if (!MatchesMagicBytes(ext, content, out var magicError))
        {
            errorMessage = magicError;
            return false;
        }

        return true;
    }

    private static bool MatchesMagicBytes(string ext, Stream stream, out string? error)
    {
        error = null;
        var original = stream.CanSeek ? stream.Position : 0L;
        try
        {
            var header = new byte[12];
            var read = stream.Read(header, 0, header.Length);
            if (read < 4)
            {
                error = "Datoteka je prazna ili oštećena.";
                return false;
            }

            return ext switch
            {
                ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
                ".png" => header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,
                ".gif" => header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38,
                ".webp" => read >= 12 &&
                           header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                           header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50,
                _ => false,
            };
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = original;
            }
        }
    }
}

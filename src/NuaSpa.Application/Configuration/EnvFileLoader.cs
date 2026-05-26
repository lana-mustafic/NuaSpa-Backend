using DotNetEnv;

namespace NuaSpa.Application.Configuration;

/// <summary>
/// Učitava <c>.env</c> iz korijena repozitorija (NuaSpa-Backend/.env) ili roditeljskih mapa.
/// Vrijednosti postaju environment varijable koje ASP.NET Core mapira na IConfiguration.
/// </summary>
public static class EnvFileLoader
{
    public static string? LoadedPath { get; private set; }

    public static void Load(string? startDirectory = null)
    {
        if (LoadedPath != null)
        {
            return;
        }

        foreach (var path in EnumerateCandidates(startDirectory))
        {
            if (!File.Exists(path))
            {
                continue;
            }

            Env.Load(path);
            LoadedPath = path;
            return;
        }
    }

    private static IEnumerable<string> EnumerateCandidates(string? startDirectory)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in CollectFromDirectory(startDirectory ?? Directory.GetCurrentDirectory()))
        {
            if (seen.Add(path))
            {
                yield return path;
            }
        }

        foreach (var path in CollectFromDirectory(AppContext.BaseDirectory))
        {
            if (seen.Add(path))
            {
                yield return path;
            }
        }
    }

    private static IEnumerable<string> CollectFromDirectory(string directory)
    {
        var current = directory;

        for (var i = 0; i < 12 && !string.IsNullOrEmpty(current); i++)
        {
            yield return Path.GetFullPath(Path.Combine(current, ".env"));

            var parent = Directory.GetParent(current);
            if (parent == null)
            {
                break;
            }

            current = parent.FullName;
        }
    }
}

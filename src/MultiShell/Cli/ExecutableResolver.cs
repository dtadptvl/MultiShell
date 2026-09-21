using System.IO;

namespace MultiShell.Cli;

public static class ExecutableResolver
{
    private static readonly string[] DefaultExtensions = [".exe", ".com", ".cmd", ".bat", ".ps1"];

    public static IReadOnlyList<string> FindAll(CliPreset preset, string? preferred = null)
    {
        var results = new List<string>();

        if (!string.IsNullOrWhiteSpace(preferred))
        {
            AddUnique(results, ResolveOne(preferred));
        }

        foreach (var candidate in preset.ExecutableCandidates)
        {
            foreach (var path in ResolveAll(candidate))
            {
                AddUnique(results, path);
            }
        }

        return results;
    }

    public static string? ResolveOne(string value) =>
        ResolveAll(value).FirstOrDefault();

    private static IEnumerable<string> ResolveAll(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield break;
        }

        var expanded = Environment.ExpandEnvironmentVariables(value.Trim().Trim('"'));

        if (Path.IsPathRooted(expanded) ||
            expanded.Contains(Path.DirectorySeparatorChar) ||
            expanded.Contains(Path.AltDirectorySeparatorChar))
        {
            if (File.Exists(expanded))
            {
                yield return Path.GetFullPath(expanded);
            }

            yield break;
        }

        var hasExtension = Path.HasExtension(expanded);
        IReadOnlyList<string> extensions = hasExtension
            ? new[] { string.Empty }
            : GetPathExtensions();

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var directory in GetSearchDirectories())
        {
            foreach (var extension in extensions)
            {
                var path = Path.Combine(directory, expanded + extension);
                if (!File.Exists(path))
                {
                    continue;
                }

                var fullPath = Path.GetFullPath(path);
                if (seen.Add(fullPath))
                {
                    yield return fullPath;
                }
            }
        }
    }

    private static void AddUnique(List<string> results, string? path)
    {
        if (path is not null &&
            !results.Contains(path, StringComparer.OrdinalIgnoreCase))
        {
            results.Add(path);
        }
    }

    private static IEnumerable<string> GetSearchDirectories()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (seen.Add(AppContext.BaseDirectory))
        {
            yield return AppContext.BaseDirectory;
        }

        foreach (var raw in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                     .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var directory = raw.Trim('"');
            if (!string.IsNullOrWhiteSpace(directory) &&
                Directory.Exists(directory) &&
                seen.Add(directory))
            {
                yield return directory;
            }
        }
    }

    private static IReadOnlyList<string> GetPathExtensions()
    {
        var configured = (Environment.GetEnvironmentVariable("PATHEXT") ?? string.Empty)
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.StartsWith('.') ? x : "." + x)
            .ToList();

        foreach (var extension in DefaultExtensions)
        {
            if (!configured.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                configured.Add(extension);
            }
        }

        return configured;
    }
}

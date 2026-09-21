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
            var resolved = ResolveOne(preferred);
            if (resolved is not null)
            {
                results.Add(resolved);
            }
        }

        foreach (var candidate in preset.ExecutableCandidates)
        {
            var resolved = ResolveOne(candidate);
            if (resolved is not null &&
                !results.Contains(resolved, StringComparer.OrdinalIgnoreCase))
            {
                results.Add(resolved);
            }
        }

        return results;
    }

    public static string? ResolveOne(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var expanded = Environment.ExpandEnvironmentVariables(value.Trim().Trim('"'));

        if (Path.IsPathRooted(expanded) || expanded.Contains(Path.DirectorySeparatorChar) ||
            expanded.Contains(Path.AltDirectorySeparatorChar))
        {
            return File.Exists(expanded) ? Path.GetFullPath(expanded) : null;
        }

        var hasExtension = Path.HasExtension(expanded);
        var extensions = hasExtension
            ? [string.Empty]
            : GetPathExtensions();

        foreach (var directory in GetSearchDirectories())
        {
            foreach (var extension in extensions)
            {
                var path = Path.Combine(directory, expanded + extension);
                if (File.Exists(path))
                {
                    return Path.GetFullPath(path);
                }
            }
        }

        return null;
    }

    private static IEnumerable<string> GetSearchDirectories()
    {
        yield return AppContext.BaseDirectory;

        foreach (var raw in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                     .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var directory = raw.Trim('"');
            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
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

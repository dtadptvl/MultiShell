using System.IO;

namespace MultiShell.Cli;

public static class CommandLineBuilder
{
    public static string Build(string executablePath, string arguments)
    {
        var extension = Path.GetExtension(executablePath);
        var quotedExecutable = Quote(executablePath);
        var suffix = string.IsNullOrWhiteSpace(arguments) ? string.Empty : " " + arguments.Trim();

        if (extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".bat", StringComparison.OrdinalIgnoreCase))
        {
            var inner = quotedExecutable + suffix;
            return $"cmd.exe /d /s /c \"{inner.Replace("\"", "\"\"")}\"";
        }

        if (extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase))
        {
            return $"powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File {quotedExecutable}{suffix}";
        }

        return quotedExecutable + suffix;
    }

    public static string Quote(string value) =>
        "\"" + value.Replace("\"", "\\\"") + "\"";
}

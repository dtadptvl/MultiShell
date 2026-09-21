using MultiShell.Models;

namespace MultiShell.Cli;

public static class CustomCommandBuilder
{
    public static string Build(string command, CustomCommandHost host)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new InvalidOperationException("Custom command is empty.");
        }

        return host switch
        {
            CustomCommandHost.Auto or CustomCommandHost.CommandPrompt =>
                $"cmd.exe /d /s /c \"{EscapeForDoubleQuotes(command)}\"",
            CustomCommandHost.WindowsPowerShell =>
                $"powershell.exe -NoLogo -NoProfile -Command \"{EscapeForDoubleQuotes(command)}\"",
            CustomCommandHost.PowerShell7 =>
                $"pwsh.exe -NoLogo -NoProfile -Command \"{EscapeForDoubleQuotes(command)}\"",
            CustomCommandHost.Wsl =>
                $"wsl.exe -- sh -lc \"{EscapeForDoubleQuotes(command)}\"",
            _ => throw new ArgumentOutOfRangeException(nameof(host))
        };
    }

    private static string EscapeForDoubleQuotes(string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}

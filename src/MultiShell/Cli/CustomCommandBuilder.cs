using System.Text;
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
                "cmd.exe /d /s /c " + command,
            CustomCommandHost.WindowsPowerShell =>
                BuildPowerShellCommand("powershell.exe", command),
            CustomCommandHost.PowerShell7 =>
                BuildPowerShellCommand("pwsh.exe", command),
            CustomCommandHost.Wsl =>
                $"wsl.exe -- sh -lc {QuoteWindowsArgument(command)}",
            _ => throw new ArgumentOutOfRangeException(nameof(host))
        };
    }

    private static string BuildPowerShellCommand(string executable, string command)
    {
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(command));
        return $"{executable} -NoLogo -NoProfile -EncodedCommand {encoded}";
    }

    private static string QuoteWindowsArgument(string value)
    {
        var builder = new StringBuilder();
        builder.Append('"');
        var backslashes = 0;

        foreach (var ch in value)
        {
            if (ch == '\\')
            {
                backslashes++;
                continue;
            }

            if (ch == '"')
            {
                builder.Append('\\', backslashes * 2 + 1);
                builder.Append('"');
                backslashes = 0;
                continue;
            }

            builder.Append('\\', backslashes);
            backslashes = 0;
            builder.Append(ch);
        }

        builder.Append('\\', backslashes * 2);
        builder.Append('"');
        return builder.ToString();
    }
}

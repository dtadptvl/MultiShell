namespace MultiShell.Cli;

public static class CliPresetCatalog
{
    public static readonly IReadOnlyList<CliPreset> All =
    [
        new("kilo", "Kilo Code", "AI Coding", ["kilo"], "", "--continue"),
        new("claude", "Claude Code", "AI Coding", ["claude"], "", "--continue",
            "--dangerously-skip-permissions", "--dangerously-skip-permissions --continue"),
        new("codex", "OpenAI Codex", "AI Coding", ["codex"], "", "resume --last",
            "--dangerously-bypass-approvals-and-sandbox",
            "--dangerously-bypass-approvals-and-sandbox resume --last"),
        new("gemini", "Gemini CLI", "AI Coding", ["gemini"], "", "--resume latest",
            "--approval-mode=yolo", "--approval-mode=yolo --resume latest"),
        new("opencode", "OpenCode", "AI Coding", ["opencode"], "", "--continue",
            "--auto", "--auto --continue"),
        new("copilot", "GitHub Copilot CLI", "AI Coding", ["copilot"], "", "--continue",
            "--allow-all", "--allow-all --continue"),
        new("cursor", "Cursor CLI", "AI Coding", ["agent", "cursor-agent"], "", "resume",
            "--force", "--force resume"),
        new("amp", "Amp", "AI Coding", ["amp"], "", "", ResumeSupported: false),
        new("aider", "Aider", "AI Coding", ["aider"], "", "--restore-chat-history",
            "--yes-always", "--yes-always --restore-chat-history"),
        new("cmd", "Command Prompt", "System", ["cmd.exe"], "/d", "/d", ResumeSupported: false),
        new("powershell", "Windows PowerShell", "System", ["powershell.exe"], "-NoLogo", "-NoLogo", ResumeSupported: false),
        new("pwsh", "PowerShell 7", "System", ["pwsh.exe"], "-NoLogo", "-NoLogo", ResumeSupported: false),
        new("wsl", "WSL", "System", ["wsl.exe"], "", "", ResumeSupported: false),
        new("custom", "Custom command", "Other", [], "", "", ResumeSupported: false, IsCustom: true)
    ];

    public static CliPreset Get(string id) =>
        All.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase))
        ?? throw new KeyNotFoundException($"Unknown CLI preset '{id}'.");
}

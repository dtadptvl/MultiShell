using System.Text.Json.Serialization;

namespace MultiShell.Models;

public enum ApprovalMode
{
    Standard,
    Full
}

public enum CustomCommandHost
{
    Auto,
    CommandPrompt,
    WindowsPowerShell,
    PowerShell7,
    Wsl
}

public sealed class ShellDefinition
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("folder")]
    public string Folder { get; init; } = string.Empty;

    [JsonPropertyName("presetId")]
    public string PresetId { get; init; } = string.Empty;

    [JsonPropertyName("approvalMode")]
    public ApprovalMode ApprovalMode { get; init; } = ApprovalMode.Standard;

    [JsonPropertyName("executablePath")]
    public string? ExecutablePath { get; init; }

    [JsonPropertyName("customCommand")]
    public string? CustomCommand { get; init; }

    [JsonPropertyName("customCommandHost")]
    public CustomCommandHost CustomCommandHost { get; init; } = CustomCommandHost.Auto;
}

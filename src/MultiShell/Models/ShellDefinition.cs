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
    public Guid Id { get; set; }

    [JsonPropertyName("folder")]
    public string Folder { get; set; } = string.Empty;

    [JsonPropertyName("presetId")]
    public string PresetId { get; set; } = string.Empty;

    [JsonPropertyName("approvalMode")]
    public ApprovalMode ApprovalMode { get; set; } = ApprovalMode.Standard;

    [JsonPropertyName("executablePath")]
    public string? ExecutablePath { get; set; }

    [JsonPropertyName("customCommand")]
    public string? CustomCommand { get; set; }

    [JsonPropertyName("customCommandHost")]
    public CustomCommandHost CustomCommandHost { get; set; } = CustomCommandHost.Auto;
}

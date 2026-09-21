using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MultiShell.Models;

namespace MultiShell.Services;

public sealed class UserPreferences
{
    public string? LastFolder { get; set; }
    public string LastPresetId { get; set; } = "kilo";
    public ApprovalMode LastApprovalMode { get; set; } = ApprovalMode.Standard;
    public string? LastCustomCommand { get; set; }
    public CustomCommandHost LastCustomCommandHost { get; set; } = CustomCommandHost.Auto;
    public Dictionary<string, string> PreferredExecutables { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> FullApprovalConfirmedPresetIds { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
    public long? TrayNoticeBootUnixSeconds { get; set; }
}

public sealed class PreferencesStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public string FilePath { get; } = Path.Combine(AppContext.BaseDirectory, "preferences.json");

    public UserPreferences Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return new UserPreferences();
            }

            using var stream = File.OpenRead(FilePath);
            var preferences = JsonSerializer.Deserialize<UserPreferences>(stream, JsonOptions)
                              ?? new UserPreferences();
            preferences.PreferredExecutables ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            preferences.FullApprovalConfirmedPresetIds ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return preferences;
        }
        catch
        {
            return new UserPreferences();
        }
    }

    public void Save(UserPreferences preferences)
    {
        var tempPath = FilePath + ".tmp";
        using (var stream = File.Create(tempPath))
        {
            JsonSerializer.Serialize(stream, preferences, JsonOptions);
        }

        File.Move(tempPath, FilePath, overwrite: true);
    }
}

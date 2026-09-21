using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MultiShell.Models;

namespace MultiShell.Services;

public sealed class ShellStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public string FilePath { get; } = Path.Combine(AppContext.BaseDirectory, "shells.json");

    public IReadOnlyList<ShellDefinition> Load()
    {
        if (!File.Exists(FilePath))
        {
            return Array.Empty<ShellDefinition>();
        }

        using var stream = File.OpenRead(FilePath);
        return JsonSerializer.Deserialize<List<ShellDefinition>>(stream, JsonOptions)
               ?? new List<ShellDefinition>();
    }

    public void Save(IEnumerable<ShellDefinition> shells)
    {
        var tempPath = FilePath + ".tmp";
        using (var stream = File.Create(tempPath))
        {
            JsonSerializer.Serialize(stream, shells, JsonOptions);
        }

        File.Move(tempPath, FilePath, overwrite: true);
    }
}

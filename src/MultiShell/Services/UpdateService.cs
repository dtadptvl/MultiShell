using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace MultiShell.Services;

public sealed record UpdateInfo(
    Version Version,
    string TagName,
    string ReleasePageUrl,
    string? AssetDownloadUrl);

public sealed class UpdateService
{
    private const string LatestReleaseApi =
        "https://api.github.com/repos/dtadptvl/MultiShell/releases/latest";
    private const string AssetName = "MultiShell-win-x64.zip";

    private readonly HttpClient _httpClient = new();

    public UpdateService()
    {
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MultiShell-Updater/1.0");
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
    }

    public async Task<UpdateInfo?> CheckAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(LatestReleaseApi, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = json.RootElement;

        var tag = root.GetProperty("tag_name").GetString() ?? string.Empty;
        if (!TryParseVersion(tag, out var latest))
        {
            return null;
        }

        var current = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0);
        if (latest <= current)
        {
            return null;
        }

        var page = root.GetProperty("html_url").GetString()
                   ?? "https://github.com/dtadptvl/MultiShell/releases";

        string? assetUrl = null;
        if (root.TryGetProperty("assets", out var assets))
        {
            foreach (var asset in assets.EnumerateArray())
            {
                if (!string.Equals(
                        asset.GetProperty("name").GetString(),
                        AssetName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                assetUrl = asset.GetProperty("browser_download_url").GetString();
                break;
            }
        }

        return new UpdateInfo(latest, tag, page, assetUrl);
    }

    public async Task StageAndLaunchUpdaterAsync(
        UpdateInfo update,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(update.AssetDownloadUrl))
        {
            throw new InvalidOperationException(
                $"Release {update.TagName} does not contain {AssetName}.");
        }

        var stagingRoot = Path.Combine(
            Path.GetTempPath(),
            "MultiShell-Update-" + Guid.NewGuid().ToString("N"));
        var archivePath = Path.Combine(stagingRoot, AssetName);
        var extractPath = Path.Combine(stagingRoot, "payload");
        Directory.CreateDirectory(stagingRoot);
        Directory.CreateDirectory(extractPath);

        using (var response = await _httpClient.GetAsync(
                   update.AssetDownloadUrl,
                   HttpCompletionOption.ResponseHeadersRead,
                   cancellationToken))
        {
            response.EnsureSuccessStatusCode();
            await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var output = File.Create(archivePath);
            await input.CopyToAsync(output, cancellationToken);
        }

        ZipFile.ExtractToDirectory(archivePath, extractPath, overwriteFiles: true);

        var stagedExe = Directory
            .EnumerateFiles(extractPath, "MultiShell.exe", SearchOption.AllDirectories)
            .FirstOrDefault()
            ?? throw new InvalidDataException("Update archive does not contain MultiShell.exe.");

        var payloadDirectory = Path.GetDirectoryName(stagedExe)!;
        var targetDirectory = AppContext.BaseDirectory.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);
        var targetExe = Path.Combine(targetDirectory, "MultiShell.exe");
        var scriptPath = Path.Combine(stagingRoot, "apply-update.ps1");

        var script = """
            param(
                [Parameter(Mandatory=$true)][int]$ProcessId,
                [Parameter(Mandatory=$true)][string]$Source,
                [Parameter(Mandatory=$true)][string]$Target,
                [Parameter(Mandatory=$true)][string]$TargetExe,
                [Parameter(Mandatory=$true)][string]$StagingRoot
            )
            $ErrorActionPreference = "Stop"
            try {
                Wait-Process -Id $ProcessId -ErrorAction SilentlyContinue
                New-Item -ItemType Directory -Force -Path $Target | Out-Null

                Get-ChildItem -LiteralPath $Source -Force | ForEach-Object {
                    if ($_.Name -notin @("shells.json", "preferences.json")) {
                        Copy-Item -LiteralPath $_.FullName -Destination $Target -Recurse -Force
                    }
                }

                Start-Process -FilePath $TargetExe
            }
            finally {
                Start-Sleep -Milliseconds 400
                Remove-Item -LiteralPath $StagingRoot -Recurse -Force -ErrorAction SilentlyContinue
            }
            """;
        await File.WriteAllTextAsync(scriptPath, script, cancellationToken);

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("-NoLogo");
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptPath);
        startInfo.ArgumentList.Add("-ProcessId");
        startInfo.ArgumentList.Add(Environment.ProcessId.ToString());
        startInfo.ArgumentList.Add("-Source");
        startInfo.ArgumentList.Add(payloadDirectory);
        startInfo.ArgumentList.Add("-Target");
        startInfo.ArgumentList.Add(targetDirectory);
        startInfo.ArgumentList.Add("-TargetExe");
        startInfo.ArgumentList.Add(targetExe);
        startInfo.ArgumentList.Add("-StagingRoot");
        startInfo.ArgumentList.Add(stagingRoot);

        _ = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not launch the update helper.");
    }

    private static bool TryParseVersion(string tag, out Version version)
    {
        var normalized = tag.Trim();
        if (normalized.StartsWith('v') || normalized.StartsWith('V'))
        {
            normalized = normalized[1..];
        }

        var prerelease = normalized.IndexOf('-');
        if (prerelease >= 0)
        {
            normalized = normalized[..prerelease];
        }

        return Version.TryParse(normalized, out version!);
    }
}

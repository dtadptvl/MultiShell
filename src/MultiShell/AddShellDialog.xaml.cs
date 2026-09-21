using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using MultiShell.Cli;
using MultiShell.Models;
using MultiShell.Services;

namespace MultiShell;

public sealed record AddShellRequest(
    string Folder,
    string PresetId,
    ApprovalMode ApprovalMode,
    string? ExecutablePath,
    string? CustomCommand,
    CustomCommandHost CustomCommandHost);

public partial class AddShellDialog : Window
{
    private readonly UserPreferences _preferences;
    private readonly IReadOnlyCollection<ShellDefinition> _existingShells;
    private readonly string? _fixedFolder;
    private bool _initializing;

    public AddShellDialog(
        UserPreferences preferences,
        IReadOnlyCollection<ShellDefinition> existingShells,
        string? fixedFolder = null)
    {
        _preferences = preferences;
        _existingShells = existingShells;
        _fixedFolder = fixedFolder;

        InitializeComponent();

        _initializing = true;
        FolderText.Text = fixedFolder ?? preferences.LastFolder ?? string.Empty;
        FolderText.IsReadOnly = fixedFolder is not null;
        BrowseFolderButton.IsEnabled = fixedFolder is null;
        CustomCommandText.Text = preferences.LastCustomCommand ?? string.Empty;
        CustomHostCombo.ItemsSource = Enum.GetValues<CustomCommandHost>();
        CustomHostCombo.SelectedItem = preferences.LastCustomCommandHost;
        PopulatePresets();
        _initializing = false;
        RefreshPresetUi();
    }

    public AddShellRequest? Request { get; private set; }

    private void PopulatePresets()
    {
        var folder = TryNormalizeFolder(FolderText.Text);
        var used = folder is null
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : _existingShells
                .Where(x => string.Equals(TryNormalizeFolder(x.Folder), folder, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.PresetId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var options = CliPresetCatalog.All
            .Where(preset => !used.Contains(preset.Id))
            .Select(preset => new PresetOption(preset, GetAvailabilityText(preset)))
            .ToList();

        PresetCombo.ItemsSource = options;

        var selected = options.FirstOrDefault(x =>
                           string.Equals(x.Preset.Id, _preferences.LastPresetId, StringComparison.OrdinalIgnoreCase))
                       ?? options.FirstOrDefault();

        PresetCombo.SelectedItem = selected;
    }

    private string GetAvailabilityText(CliPreset preset)
    {
        if (preset.IsCustom)
        {
            return string.Empty;
        }

        _preferences.PreferredExecutables.TryGetValue(preset.Id, out var preferred);
        return ExecutableResolver.FindAll(preset, preferred).Count == 0
            ? "Not available"
            : string.Empty;
    }

    private void RefreshPresetUi()
    {
        if (PresetCombo.SelectedItem is not PresetOption option)
        {
            AddButton.IsEnabled = false;
            return;
        }

        AddButton.IsEnabled = true;
        var preset = option.Preset;
        CustomPanel.Visibility = preset.IsCustom ? Visibility.Visible : Visibility.Collapsed;
        ExecutablePanel.Visibility = preset.IsCustom ? Visibility.Collapsed : Visibility.Visible;

        var approvalChoices = preset.SupportsFullApproval
            ? new[] { ApprovalMode.Standard, ApprovalMode.Full }
            : new[] { ApprovalMode.Standard };
        ApprovalCombo.ItemsSource = approvalChoices;
        ApprovalCombo.SelectedItem = approvalChoices.Contains(_preferences.LastApprovalMode)
            ? _preferences.LastApprovalMode
            : ApprovalMode.Standard;

        if (preset.IsCustom)
        {
            AvailabilityText.Text = string.Empty;
            return;
        }

        _preferences.PreferredExecutables.TryGetValue(preset.Id, out var preferred);
        var preferredResolved = string.IsNullOrWhiteSpace(preferred)
            ? null
            : ExecutableResolver.ResolveOne(preferred);
        var detected = ExecutableResolver.FindAll(preset, preferred);
        ExecutableCombo.ItemsSource = detected;

        if (detected.Count > 0)
        {
            ExecutableCombo.SelectedIndex = 0;
            if (!string.IsNullOrWhiteSpace(preferred) && preferredResolved is null)
            {
                AvailabilityText.Text = "The remembered executable is unavailable; using the detected executable instead.";
            }
            else
            {
                AvailabilityText.Text = detected.Count == 1
                    ? "Detected automatically."
                    : $"Detected {detected.Count} executables. The selected path will be remembered.";
            }
        }
        else
        {
            ExecutableCombo.Text = string.Empty;
            AvailabilityText.Text = "Not available. Browse to this CLI executable if it exists elsewhere.";
        }
    }

    private void OnPresetSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_initializing)
        {
            RefreshPresetUi();
        }
    }

    private void OnBrowseFolderClick(object sender, RoutedEventArgs e)
    {
        var picker = new OpenFolderDialog
        {
            Title = "Choose folder",
            Multiselect = false
        };

        if (picker.ShowDialog(this) != true)
        {
            return;
        }

        FolderText.Text = NormalizeFolder(picker.FolderName);
        _initializing = true;
        PopulatePresets();
        _initializing = false;
        RefreshPresetUi();
    }

    private void OnBrowseExecutableClick(object sender, RoutedEventArgs e)
    {
        var picker = new OpenFileDialog
        {
            Title = "Choose CLI executable",
            Filter = "Executables and command shims (*.exe;*.com;*.cmd;*.bat;*.ps1)|*.exe;*.com;*.cmd;*.bat;*.ps1|All files (*.*)|*.*"
        };

        if (picker.ShowDialog(this) == true)
        {
            ExecutableCombo.Text = picker.FileName;
            AvailabilityText.Text = "Using the selected executable path.";
        }
    }

    private void OnAddClick(object sender, RoutedEventArgs e)
    {
        var folder = TryNormalizeFolder(FolderText.Text);
        if (folder is null || !Directory.Exists(folder))
        {
            MessageBox.Show(this, "Choose an existing folder.", "Add Shell",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (PresetCombo.SelectedItem is not PresetOption option)
        {
            return;
        }

        var preset = option.Preset;
        if (_existingShells.Any(x =>
                string.Equals(TryNormalizeFolder(x.Folder), folder, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.PresetId, preset.Id, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show(this,
                $"{preset.Name} already exists for this folder. MultiShell allows one shell per CLI per folder.",
                "Add Shell", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var approval = ApprovalCombo.SelectedItem is ApprovalMode mode
            ? mode
            : ApprovalMode.Standard;

        string? executable = null;
        string? customCommand = null;
        var customHost = CustomHostCombo.SelectedItem is CustomCommandHost host
            ? host
            : CustomCommandHost.Auto;

        if (preset.IsCustom)
        {
            customCommand = CustomCommandText.Text.Trim();
            if (string.IsNullOrWhiteSpace(customCommand))
            {
                MessageBox.Show(this, "Enter a custom command.", "Add Shell",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }
        else
        {
            executable = ExecutableResolver.ResolveOne(ExecutableCombo.Text);
            if (executable is null)
            {
                MessageBox.Show(this,
                    $"{preset.Name} is not available. Choose a valid executable path.",
                    "Add Shell", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }

        Request = new AddShellRequest(
            folder,
            preset.Id,
            approval,
            executable,
            customCommand,
            customHost);
        DialogResult = true;
    }

    private static string NormalizeFolder(string folder)
    {
        var fullPath = Path.GetFullPath(folder);
        var root = Path.GetPathRoot(fullPath);

        if (!string.IsNullOrEmpty(root) &&
            string.Equals(fullPath, root, StringComparison.OrdinalIgnoreCase))
        {
            return root;
        }

        return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static string? TryNormalizeFolder(string? folder)
    {
        try
        {
            return string.IsNullOrWhiteSpace(folder) ? null : NormalizeFolder(folder);
        }
        catch
        {
            return null;
        }
    }

    private sealed record PresetOption(CliPreset Preset, string Availability)
    {
        public string DisplayText =>
            string.IsNullOrWhiteSpace(Availability)
                ? $"{Preset.Category} · {Preset.Name}"
                : $"{Preset.Category} · {Preset.Name} — {Availability}";
    }
}

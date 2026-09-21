using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Terminal.Wpf;
using MultiShell.Cli;
using MultiShell.Models;
using MultiShell.Services;
using MultiShell.Terminal;
using DrawingIcon = System.Drawing.Icon;
using Forms = System.Windows.Forms;

namespace MultiShell;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<ShellDefinition> _shells = [];
    private readonly ObservableCollection<FolderGroupViewModel> _groups = [];
    private readonly Dictionary<Guid, ShellSession> _sessions = [];
    private readonly ShellStore _shellStore = new();
    private readonly PreferencesStore _preferencesStore = new();
    private readonly UpdateService _updateService = new();
    private readonly UserPreferences _preferences;

    private Forms.NotifyIcon? _trayIcon;
    private DrawingIcon? _trayDrawingIcon;
    private bool _isShuttingDown;
    private bool _suppressApprovalChange;
    private bool _autoUpdateChecked;
    private Guid? _selectedShellId;

    public MainWindow()
    {
        InitializeComponent();
        _preferences = _preferencesStore.Load();
        FoldersTree.ItemsSource = _groups;

        LoadShells();
        InitializeTray();
        RebuildSidebar(_shells.FirstOrDefault()?.Id);
        RefreshSelectedShellUi();
    }

    public void RestoreFromExternalLaunch() => ShowFromTray();

    private ShellDefinition? SelectedShell =>
        _selectedShellId is Guid id
            ? _shells.FirstOrDefault(x => x.Id == id)
            : null;

    private ShellSession? SelectedSession =>
        SelectedShell is { } shell && _sessions.TryGetValue(shell.Id, out var session)
            ? session
            : null;

    private bool HasLiveSessions => _sessions.Values.Any(static session => session.IsLive);

    private void LoadShells()
    {
        try
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var shell in _shellStore.Load())
            {
                if (shell.Id == Guid.Empty ||
                    string.IsNullOrWhiteSpace(shell.Folder) ||
                    string.IsNullOrWhiteSpace(shell.PresetId))
                {
                    continue;
                }

                try
                {
                    _ = CliPresetCatalog.Get(shell.PresetId);
                }
                catch
                {
                    continue;
                }

                var folder = NormalizeFolder(shell.Folder);
                var key = folder + "\n" + shell.PresetId;
                if (!seen.Add(key))
                {
                    continue;
                }

                shell.Folder = folder;
                _shells.Add(shell);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $"Could not read shells.json.\n\n{ex.Message}",
                "MultiShell",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void InitializeTray()
    {
        _trayDrawingIcon = DrawingIcon.ExtractAssociatedIcon(Environment.ProcessPath!);
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Open", null, (_, _) => Dispatcher.Invoke(ShowFromTray));
        menu.Items.Add("Check for updates...", null, (_, _) => Dispatcher.Invoke(() => _ = CheckForUpdatesAsync(manual: true)));
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Quit", null, (_, _) => Dispatcher.Invoke(() => _ = QuitFromTrayAsync()));

        _trayIcon = new Forms.NotifyIcon
        {
            Text = "MultiShell",
            Icon = _trayDrawingIcon,
            Visible = true,
            ContextMenuStrip = menu
        };

        _trayIcon.DoubleClick += (_, _) => Dispatcher.Invoke(ShowFromTray);
    }

    private async void OnAddShellClick(object sender, RoutedEventArgs e) =>
        await ShowAddShellDialogAsync(null);

    private async void OnAddShellToFolderClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string folder })
        {
            e.Handled = true;
            await ShowAddShellDialogAsync(folder);
        }
    }

    private async void OnOpenWithAnotherShellClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem)
        {
            return;
        }

        var contextMenu = ItemsControl.ItemsControlFromItemContainer(menuItem) as ContextMenu;
        var item = (contextMenu?.PlacementTarget as FrameworkElement)?.DataContext as ShellItemViewModel;
        if (item is not null)
        {
            await ShowAddShellDialogAsync(item.Folder);
        }
    }

    private async Task ShowAddShellDialogAsync(string? fixedFolder)
    {
        var dialog = new AddShellDialog(_preferences, _shells, fixedFolder)
        {
            Owner = this
        };

        if (dialog.ShowDialog() != true || dialog.Request is not { } request)
        {
            return;
        }

        var preset = CliPresetCatalog.Get(request.PresetId);
        if (request.ApprovalMode == ApprovalMode.Full &&
            !ConfirmFullApprovalIfNeeded(preset))
        {
            return;
        }

        var shell = new ShellDefinition
        {
            Id = Guid.NewGuid(),
            Folder = request.Folder,
            PresetId = request.PresetId,
            ApprovalMode = request.ApprovalMode,
            ExecutablePath = request.ExecutablePath,
            CustomCommand = request.CustomCommand,
            CustomCommandHost = request.CustomCommandHost
        };

        _shells.Add(shell);
        RememberAddChoices(request);
        PersistShells();
        RebuildSidebar(shell.Id);

        await StartShellAsync(shell, resume: false);
    }

    private bool ConfirmFullApprovalIfNeeded(CliPreset preset)
    {
        if (_preferences.FullApprovalConfirmedPresetIds.Contains(preset.Id))
        {
            return true;
        }

        var result = MessageBox.Show(this,
            $"Full Approval lets {preset.Name} use its native session-local no-prompt mode. " +
            "It may edit files and run commands without asking first.\n\nEnable Full Approval for this CLI?",
            "Full Approval",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (result != MessageBoxResult.Yes)
        {
            return false;
        }

        _preferences.FullApprovalConfirmedPresetIds.Add(preset.Id);
        PersistPreferences();
        return true;
    }

    private void RememberAddChoices(AddShellRequest request)
    {
        _preferences.LastFolder = request.Folder;
        _preferences.LastPresetId = request.PresetId;
        _preferences.LastApprovalMode = request.ApprovalMode;
        _preferences.LastCustomCommand = request.CustomCommand;
        _preferences.LastCustomCommandHost = request.CustomCommandHost;

        if (!string.IsNullOrWhiteSpace(request.ExecutablePath))
        {
            _preferences.PreferredExecutables[request.PresetId] = request.ExecutablePath;
        }

        PersistPreferences();
    }

    private void OnTreeSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        _selectedShellId = e.NewValue is ShellItemViewModel item
            ? item.Definition.Id
            : null;

        RefreshSelectedShellUi();
        ShowSelectedTerminal();
    }

    private void OnOpenFolderClick(object sender, RoutedEventArgs e)
    {
        if (SelectedShell is not { } shell)
        {
            return;
        }

        try
        {
            var info = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                UseShellExecute = true
            };
            info.ArgumentList.Add(shell.Folder);
            Process.Start(info);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $"Could not open the folder.\n\n{ex.Message}",
                "MultiShell",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void OnStartResumeClick(object sender, RoutedEventArgs e)
    {
        if (SelectedShell is { } shell)
        {
            await StartShellAsync(shell, resume: true);
        }
    }

    private async void OnRestartClick(object sender, RoutedEventArgs e)
    {
        if (SelectedShell is not { } shell)
        {
            return;
        }

        var session = GetSession(shell);
        var oldView = session.View;

        try
        {
            SetActionButtonsEnabled(false);
            await session.StopAsync();
            DetachView(oldView);
            await session.StartAsync(
                resume: true,
                prepareViewAsync: PrepareTerminalViewAsync);
            ShowSelectedTerminal();
            await Dispatcher.InvokeAsync(() => session.View?.Focus(), DispatcherPriority.Input);
        }
        catch (Exception ex)
        {
            ShowSessionError($"Could not restart {CliPresetCatalog.Get(shell.PresetId).Name}.", ex);
        }
        finally
        {
            RefreshSelectedShellUi();
        }
    }

    private async void OnStopClick(object sender, RoutedEventArgs e)
    {
        if (SelectedSession is not { } session)
        {
            return;
        }

        try
        {
            SetActionButtonsEnabled(false);
            await session.StopAsync();
        }
        catch (Exception ex)
        {
            ShowSessionError("Could not stop this shell.", ex);
        }
        finally
        {
            RefreshSelectedShellUi();
        }
    }

    private async void OnRemoveClick(object sender, RoutedEventArgs e)
    {
        if (SelectedShell is not { } shell)
        {
            return;
        }

        var preset = CliPresetCatalog.Get(shell.PresetId);
        var session = _sessions.GetValueOrDefault(shell.Id);

        if (session?.IsLive == true)
        {
            var result = MessageBox.Show(this,
                $"{preset.Name} is running. Stop it and remove this shell from MultiShell?\n\n" +
                "This does not delete the folder or the CLI's own history.",
                "Remove shell",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                await session.StopAsync();
            }
            catch (Exception ex)
            {
                ShowSessionError("Could not stop this shell.", ex);
                return;
            }
        }

        if (session is not null)
        {
            DetachView(session.View);
            _sessions.Remove(shell.Id);
        }

        _shells.Remove(shell);
        _selectedShellId = null;
        PersistShells();
        RebuildSidebar(_shells.FirstOrDefault()?.Id);
        RefreshSelectedShellUi();
        ShowSelectedTerminal();
    }

    private void OnApprovalModeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressApprovalChange ||
            SelectedShell is not { } shell ||
            ApprovalModeCombo.SelectedItem is not ApprovalMode mode ||
            mode == shell.ApprovalMode)
        {
            return;
        }

        var preset = CliPresetCatalog.Get(shell.PresetId);
        if (mode == ApprovalMode.Full && !ConfirmFullApprovalIfNeeded(preset))
        {
            RefreshSelectedShellUi();
            return;
        }

        shell.ApprovalMode = mode;
        _preferences.LastApprovalMode = mode;
        PersistPreferences();
        PersistShells();
        RebuildSidebar(shell.Id);
        RefreshSelectedShellUi();
    }

    private async Task StartShellAsync(ShellDefinition shell, bool resume)
    {
        var session = GetSession(shell);
        _selectedShellId = shell.Id;

        if (session.IsLive)
        {
            RebuildSidebar(shell.Id);
            ShowSelectedTerminal();
            session.View?.Focus();
            return;
        }

        var oldView = session.View;

        try
        {
            SetActionButtonsEnabled(false);
            DetachView(oldView);
            await session.StartAsync(
                resume,
                prepareViewAsync: PrepareTerminalViewAsync);
            ShowSelectedTerminal();
            await Dispatcher.InvokeAsync(() => session.View?.Focus(), DispatcherPriority.Input);
        }
        catch (Exception ex)
        {
            ShowSessionError($"Could not start {CliPresetCatalog.Get(shell.PresetId).Name}.", ex);
        }
        finally
        {
            RebuildSidebar(shell.Id);
            RefreshSelectedShellUi();
        }
    }

    private async Task PrepareTerminalViewAsync(TerminalControl view)
    {
        AttachView(view);
        ShowSelectedTerminal();

        if (view.IsLoaded)
        {
            return;
        }

        var loaded = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        RoutedEventHandler? handler = null;
        handler = (_, _) => loaded.TrySetResult();

        view.Loaded += handler;
        try
        {
            await loaded.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            view.Loaded -= handler;
        }

        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Loaded);

        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
        while (view.NativeTerminalForTesting == IntPtr.Zero &&
               DateTime.UtcNow < deadline)
        {
            await Task.Delay(20);
        }

        if (view.NativeTerminalForTesting == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                "Native Windows Terminal control did not initialize.");
        }
    }

    private ShellSession GetSession(ShellDefinition shell)
    {
        if (_sessions.TryGetValue(shell.Id, out var existing))
        {
            return existing;
        }

        var session = new ShellSession(shell);
        session.StateChanged += (_, _) => Dispatcher.Invoke(() =>
        {
            RebuildSidebar(_selectedShellId);
            RefreshSelectedShellUi();
            ShowSelectedTerminal();
        });
        _sessions.Add(shell.Id, session);
        return session;
    }

    private void RebuildSidebar(Guid? selectShellId)
    {
        _groups.Clear();

        foreach (var folderGroup in _shells
                     .GroupBy(x => NormalizeFolder(x.Folder), StringComparer.OrdinalIgnoreCase)
                     .OrderBy(x => GetFolderName(x.Key), StringComparer.OrdinalIgnoreCase))
        {
            var group = new FolderGroupViewModel
            {
                Name = GetFolderName(folderGroup.Key),
                Folder = folderGroup.Key
            };

            foreach (var shell in folderGroup
                         .OrderBy(x => CliPresetCatalog.Get(x.PresetId).Name, StringComparer.OrdinalIgnoreCase))
            {
                var state = _sessions.TryGetValue(shell.Id, out var session)
                    ? session.State
                    : ShellSessionState.Stopped;
                var item = new ShellItemViewModel
                {
                    Definition = shell,
                    PresetName = CliPresetCatalog.Get(shell.PresetId).Name,
                    State = state,
                    IsSelected = selectShellId == shell.Id
                };
                group.Shells.Add(item);
            }

            _groups.Add(group);
        }

        _selectedShellId = selectShellId is Guid id && _shells.Any(x => x.Id == id)
            ? id
            : null;
    }

    private static string GetFolderName(string folder)
    {
        var name = new DirectoryInfo(folder).Name;
        return string.IsNullOrWhiteSpace(name) ? folder : name;
    }

    private void AttachView(UIElement? view)
    {
        if (view is null || TerminalHostGrid.Children.Contains(view))
        {
            return;
        }

        TerminalHostGrid.Children.Add(view);
    }

    private void DetachView(UIElement? view)
    {
        if (view is not null)
        {
            TerminalHostGrid.Children.Remove(view);
        }
    }

    private void ShowSelectedTerminal()
    {
        foreach (UIElement child in TerminalHostGrid.Children)
        {
            child.Visibility = Visibility.Hidden;
        }

        var view = SelectedSession?.View;
        if (view is not null && TerminalHostGrid.Children.Contains(view))
        {
            view.Visibility = Visibility.Visible;
            TerminalPlaceholder.Visibility = Visibility.Collapsed;
        }
        else
        {
            TerminalPlaceholder.Visibility = Visibility.Visible;
        }
    }

    private void RefreshSelectedShellUi()
    {
        var shell = SelectedShell;
        var session = SelectedSession;

        _suppressApprovalChange = true;
        try
        {
            if (shell is null)
            {
                ShellNameText.Text = "No shell selected";
                ShellFolderText.Text = string.Empty;
                ShellStatusText.Text = "Stopped";
                FullBadgeText.Visibility = Visibility.Collapsed;
                ApprovalModeCombo.ItemsSource = Array.Empty<ApprovalMode>();
                SetActionButtonsEnabled(false);
                TerminalPlaceholder.Visibility = Visibility.Visible;
                return;
            }

            var preset = CliPresetCatalog.Get(shell.PresetId);
            ShellNameText.Text = preset.Name;
            ShellFolderText.Text = shell.Folder;
            ShellFolderText.ToolTip = shell.Folder;
            ShellStatusText.Text = session?.State switch
            {
                ShellSessionState.Starting => "Starting",
                ShellSessionState.Running => "Running",
                _ => "Stopped"
            };
            FullBadgeText.Visibility = shell.ApprovalMode == ApprovalMode.Full
                ? Visibility.Visible
                : Visibility.Collapsed;

            var approvalChoices = preset.SupportsFullApproval
                ? new[] { ApprovalMode.Standard, ApprovalMode.Full }
                : new[] { ApprovalMode.Standard };
            ApprovalModeCombo.ItemsSource = approvalChoices;
            ApprovalModeCombo.SelectedItem = shell.ApprovalMode;

            var starting = session?.State == ShellSessionState.Starting;
            OpenFolderButton.IsEnabled = true;
            ApprovalModeCombo.IsEnabled = !starting;
            StartButton.IsEnabled = !starting && session?.State != ShellSessionState.Running;
            RestartButton.IsEnabled = !starting;
            StopButton.IsEnabled = session?.IsLive == true;
            RemoveButton.IsEnabled = !starting;
        }
        finally
        {
            _suppressApprovalChange = false;
        }
    }

    private void SetActionButtonsEnabled(bool enabled)
    {
        OpenFolderButton.IsEnabled = enabled;
        ApprovalModeCombo.IsEnabled = enabled;
        StartButton.IsEnabled = enabled;
        RestartButton.IsEnabled = enabled;
        StopButton.IsEnabled = enabled;
        RemoveButton.IsEnabled = enabled;
    }

    private void PersistShells()
    {
        try
        {
            _shellStore.Save(_shells);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $"Could not save shells.json.\n\n{ex.Message}",
                "MultiShell",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void PersistPreferences()
    {
        try
        {
            _preferencesStore.Save(_preferences);
        }
        catch
        {
        }
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

    private void ShowSessionError(string message, Exception ex)
    {
        MessageBox.Show(this,
            $"{message}\n\n{ex.Message}",
            "MultiShell",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private async void OnContentRendered(object? sender, EventArgs e)
    {
        if (_autoUpdateChecked)
        {
            return;
        }

        _autoUpdateChecked = true;
        await CheckForUpdatesAsync(manual: false);
    }

    private async Task CheckForUpdatesAsync(bool manual)
    {
        try
        {
            var update = await _updateService.CheckAsync();
            if (update is null)
            {
                if (manual)
                {
                    MessageBox.Show(this,
                        "MultiShell is up to date.",
                        "MultiShell Update",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(update.AssetDownloadUrl))
            {
                if (manual)
                {
                    MessageBox.Show(this,
                        $"MultiShell {update.Version} is available, but its portable update package is not attached to the release yet.",
                        "MultiShell Update",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                return;
            }

            var dialog = new UpdateDialog(update)
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            if (HasLiveSessions)
            {
                var confirm = MessageBox.Show(this,
                    "Updating will stop all running shells and restart MultiShell. Continue?",
                    "Update MultiShell",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);

                if (confirm != MessageBoxResult.Yes)
                {
                    return;
                }

                foreach (var session in _sessions.Values.Where(static x => x.IsLive).ToArray())
                {
                    await session.StopAsync();
                }
            }

            await _updateService.StageAndLaunchUpdaterAsync(update);
            BeginShutdown();
        }
        catch (Exception ex)
        {
            if (manual)
            {
                MessageBox.Show(this,
                    $"Could not check for or apply the update.\n\n{ex.Message}",
                    "MultiShell Update",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        if (!_isShuttingDown && WindowState == WindowState.Minimized)
        {
            HideToTray();
        }
    }

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        if (_isShuttingDown)
        {
            return;
        }

        if (HasLiveSessions)
        {
            e.Cancel = true;
            HideToTray();
            return;
        }

        BeginShutdown();
    }

    private void HideToTray()
    {
        Hide();
        ShowTrayNoticeOncePerBoot();
    }

    private void ShowTrayNoticeOncePerBoot()
    {
        if (_trayIcon is null)
        {
            return;
        }

        var boot = DateTimeOffset.UtcNow - TimeSpan.FromMilliseconds(Environment.TickCount64);
        var bootSeconds = boot.ToUnixTimeSeconds();
        var alreadyShownThisBoot =
            _preferences.TrayNoticeBootUnixSeconds is long previous &&
            Math.Abs(previous - bootSeconds) <= 120;

        if (alreadyShownThisBoot)
        {
            return;
        }

        _preferences.TrayNoticeBootUnixSeconds = bootSeconds;
        PersistPreferences();

        _trayIcon.BalloonTipTitle = "MultiShell";
        _trayIcon.BalloonTipText = "MultiShell is still running in the tray.";
        _trayIcon.BalloonTipIcon = Forms.ToolTipIcon.Info;
        _trayIcon.ShowBalloonTip(2500);
    }

    private void ShowFromTray()
    {
        if (!IsVisible)
        {
            Show();
        }

        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Activate();
        Topmost = true;
        Topmost = false;
        Focus();
    }

    private async Task QuitFromTrayAsync()
    {
        if (HasLiveSessions)
        {
            var result = MessageBox.Show(this,
                "Stop all running shells and exit MultiShell?",
                "Quit MultiShell",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            foreach (var session in _sessions.Values.Where(static x => x.IsLive).ToArray())
            {
                try
                {
                    await session.StopAsync();
                }
                catch
                {
                }
            }
        }

        BeginShutdown();
    }

    private void BeginShutdown()
    {
        if (_isShuttingDown)
        {
            return;
        }

        _isShuttingDown = true;

        if (_trayIcon is not null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
        }

        _trayDrawingIcon?.Dispose();
        _trayDrawingIcon = null;
        Application.Current.Shutdown();
    }
}

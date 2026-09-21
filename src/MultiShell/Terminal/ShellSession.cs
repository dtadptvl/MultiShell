using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Terminal.Wpf;
using MultiShell.Cli;
using MultiShell.Models;

namespace MultiShell.Terminal;

public enum ShellSessionState
{
    Stopped,
    Starting,
    Running
}

public sealed class ShellSession
{
    private int _generation;
    private TaskCompletionSource<uint>? _exitSignal;

    public ShellSession(ShellDefinition definition)
    {
        Definition = definition;
    }

    public ShellDefinition Definition { get; }
    public ShellSessionState State { get; private set; } = ShellSessionState.Stopped;
    public TerminalControl? View { get; private set; }
    public bool IsLive => State is ShellSessionState.Starting or ShellSessionState.Running;

    public event EventHandler? StateChanged;

    public async Task StartAsync(bool resume, Func<TerminalControl, Task> prepareViewAsync)
    {
        ArgumentNullException.ThrowIfNull(prepareViewAsync);

        if (IsLive)
        {
            return;
        }

        State = ShellSessionState.Starting;
        RaiseStateChanged();

        var generation = ++_generation;
        var view = CreateTerminalView();
        var exitSignal = new TaskCompletionSource<uint>(TaskCreationOptions.RunContinuationsAsynchronously);

        View = view;
        _exitSignal = exitSignal;
        view.SessionExited += OnSessionExited;

        try
        {
            await prepareViewAsync(view);
            view.SetTheme(CreateTheme(), "Cascadia Mono", 13);

            var commandLine = BuildCommandLine(resume);
            view.StartSession(commandLine, Definition.Folder);

            if (generation != _generation)
            {
                view.TerminateSession();
                return;
            }

            State = ShellSessionState.Running;
            RaiseStateChanged();
        }
        catch
        {
            CleanupFailedStart(view);
            throw;
        }
    }

    public async Task StopAsync()
    {
        var view = View;
        if (!IsLive && (view is null || !view.IsSessionRunning))
        {
            State = ShellSessionState.Stopped;
            RaiseStateChanged();
            return;
        }

        ++_generation;
        var exitSignal = _exitSignal;

        try
        {
            view?.TerminateSession();

            if (exitSignal is not null)
            {
                try
                {
                    await exitSignal.Task.WaitAsync(TimeSpan.FromSeconds(5));
                }
                catch
                {
                }
            }
        }
        finally
        {
            State = ShellSessionState.Stopped;
            RaiseStateChanged();
        }
    }

    private string BuildCommandLine(bool resume)
    {
        var preset = CliPresetCatalog.Get(Definition.PresetId);
        if (preset.IsCustom)
        {
            return CustomCommandBuilder.Build(
                Definition.CustomCommand ?? string.Empty,
                Definition.CustomCommandHost);
        }

        var executable = ExecutableResolver.ResolveOne(Definition.ExecutablePath ?? string.Empty)
                         ?? ExecutableResolver.FindAll(preset).FirstOrDefault()
                         ?? throw new FileNotFoundException(
                             $"{preset.Name} is not available. Choose its executable path in MultiShell.");

        var arguments = preset.GetArguments(resume, Definition.ApprovalMode);
        return CommandLineBuilder.Build(executable, arguments);
    }

    private void OnSessionExited(uint exitCode)
    {
        _exitSignal?.TrySetResult(exitCode);

        if (State == ShellSessionState.Stopped)
        {
            return;
        }

        State = ShellSessionState.Stopped;
        RaiseStateChanged();
    }

    private void CleanupFailedStart(TerminalControl view)
    {
        ++_generation;

        try
        {
            view.TerminateSession();
        }
        catch
        {
        }

        view.SessionExited -= OnSessionExited;
        _exitSignal = null;
        View = null;
        State = ShellSessionState.Stopped;
        RaiseStateChanged();
    }

    private static TerminalControl CreateTerminalView()
    {
        var view = new TerminalControl
        {
            AutoResize = true,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Focusable = true
        };

        KeyboardNavigation.SetTabNavigation(view, KeyboardNavigationMode.Contained);
        KeyboardNavigation.SetDirectionalNavigation(view, KeyboardNavigationMode.Contained);
        return view;
    }

    private static TerminalTheme CreateTheme() =>
        new()
        {
            DefaultBackground = 0x0C0C0C,
            DefaultForeground = 0xCCCCCC,
            DefaultSelectionBackground = 0x777777,
            CursorStyle = CursorStyle.BlinkingBar,
            ColorTable =
            [
                0x0C0C0C, 0x1F0FC5, 0x0EA113, 0x009CC1,
                0xDA3700, 0x981788, 0xDD963A, 0xCCCCCC,
                0x767676, 0x5648E7, 0x0CC616, 0xA5F1F9,
                0xFF783B, 0x9E00B4, 0xD6D661, 0xF2F2F2
            ]
        };

    private void RaiseStateChanged() => StateChanged?.Invoke(this, EventArgs.Empty);
}

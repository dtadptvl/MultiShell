using System.Threading;
using System.Windows;
using MultiShell.Terminal;

namespace MultiShell;

public partial class App : System.Windows.Application
{
    private const string MutexName = @"Local\MultiShell.SingleInstance.v1";
    private const string ActivationEventName = @"Local\MultiShell.Activate.v1";

    private Mutex? _instanceMutex;
    private EventWaitHandle? _activationEvent;
    private volatile bool _exiting;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        var smokeArgument = e.Args.FirstOrDefault(
            static x => x.StartsWith("--smoke-test", StringComparison.OrdinalIgnoreCase));
        if (smokeArgument is not null)
        {
            const string prefix = "--smoke-test=";
            var mode = smokeArgument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? smokeArgument[prefix.Length..]
                : "all";
            var exitCode = await TerminalSmokeTest.RunAsync(mode);
            Shutdown(exitCode);
            return;
        }

        _instanceMutex = new Mutex(true, MutexName, out var createdNew);
        if (!createdNew)
        {
            try
            {
                using var activation = EventWaitHandle.OpenExisting(ActivationEventName);
                activation.Set();
            }
            catch
            {
            }

            Shutdown();
            return;
        }

        _activationEvent = new EventWaitHandle(
            false,
            EventResetMode.AutoReset,
            ActivationEventName,
            out _);

        var window = new MainWindow();
        MainWindow = window;
        window.Show();

        _ = Task.Run(() =>
        {
            while (!_exiting)
            {
                try
                {
                    _activationEvent.WaitOne();
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                if (_exiting)
                {
                    return;
                }

                Dispatcher.Invoke(window.RestoreFromExternalLaunch);
            }
        });
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        _exiting = true;
        _activationEvent?.Dispose();
        _activationEvent = null;

        if (_instanceMutex is not null)
        {
            try
            {
                _instanceMutex.ReleaseMutex();
            }
            catch
            {
            }

            _instanceMutex.Dispose();
            _instanceMutex = null;
        }
    }
}
